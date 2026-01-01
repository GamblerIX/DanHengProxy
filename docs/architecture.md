# 架构设计文档

> 本文档描述 DanHengProxy 的整体架构和核心组件。

## 项目概述

DanHengProxy 是一个 HTTP/HTTPS 代理工具，主要用于拦截和重定向游戏客户端的网络请求到私有服务器。

## 技术栈

| 组件 | 技术 |
|------|------|
| 运行时 | .NET 8.0 |
| 编译模式 | AOT (Ahead-of-Time) |
| 代理库 | Titanium.Web.Proxy (via Unobtanium.Web.Proxy) |
| 序列化 | System.Text.Json (Source Generator) |

## 项目结构

```
DanHengProxy/
├── DanHengProxy.sln          # Visual Studio 解决方案
├── DanHengProxy.csproj       # 项目文件
├── Program.cs                # 程序入口点
├── ProxyService.cs           # 代理服务核心实现
├── ProxyConfig.cs            # 配置数据模型
├── ProxyConfigContext.cs     # JSON 序列化上下文
├── config.tmpl.json          # 配置模板
├── DanHeng.ico               # 程序图标
├── LICENSE                   # 许可证
├── README.md                 # 项目说明
└── docs/                     # 开发文档
    ├── architecture.md       # 架构设计（本文档）
    ├── development.md        # 开发指南
    └── api-reference.md      # API 参考
```

## 核心组件

### 1. Program.cs - 程序入口

**职责：**
- 初始化控制台界面
- 加载配置文件
- 检测系统代理冲突
- 管理程序生命周期

**关键流程：**
```
Main()
  ├── PrintBanner()           # 显示程序横幅
  ├── CheckProxy()            # 检查系统代理
  ├── InitConfig()            # 初始化配置文件
  ├── 加载配置并创建 ProxyService
  └── 注册退出事件处理
```

### 2. ProxyService.cs - 代理服务

**职责：**
- 创建和管理代理服务器实例
- 处理 HTTP/HTTPS 请求拦截
- 执行域名匹配和 URL 重定向
- 阻止遥测请求

**核心事件处理器：**

| 事件 | 方法 | 说明 |
|------|------|------|
| BeforeTunnelConnectRequest | `BeforeTunnelConnectRequest` | 决定是否解密 HTTPS 流量 |
| BeforeRequest | `BeforeRequest` | 处理请求重定向和阻止 |
| ServerCertificateValidationCallback | `OnCertValidation` | SSL 证书验证 |

**请求处理流程：**
```
收到请求
  ├── 检查域名是否在忽略列表 → 是 → 透传
  ├── 检查域名是否需要重定向
  │     ├── 是 → 检查 URL 是否需要阻止
  │     │         ├── 是 → 返回 404
  │     │         └── 否 → 重定向到目标服务器
  │     └── 否 → 检查 URL 是否需要强制重定向
  │               ├── 是 → 执行重定向
  │               └── 否 → 透传
  └── 完成
```

### 3. ProxyConfig.cs - 配置模型

**配置分类：**

| 分类 | 配置项 | 说明 |
|------|--------|------|
| 目标服务器 | `DestinationHost`, `DestinationPort` | 私服地址和端口 |
| SSL 配置 | `EnableSsl`, `ValidateServerCertificate` | HTTPS 连接设置 |
| 代理绑定 | `ProxyBindPort` | 本地代理端口 |
| 域名过滤 | `RedirectDomains`, `AlwaysIgnoreDomains` | 域名规则 |
| URL 过滤 | `ForceRedirectOnUrlContains`, `BlockUrls` | URL 规则 |

### 4. ProxyConfigContext.cs - JSON 序列化

**用途：**
- 为 AOT 编译提供 JSON 序列化支持
- 使用 .NET 8 的 Source Generator 特性
- 允许配置文件包含注释和尾随逗号

## 数据流

```
┌─────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   Game      │────▶│  DanHengProxy   │────▶│  Private Server │
│   Client    │◀────│  (System Proxy) │◀────│  (Destination)  │
└─────────────┘     └─────────────────┘     └─────────────────┘
                           │
                           │ 拦截并处理
                           ▼
                    ┌─────────────────┐
                    │   匹配规则:     │
                    │ - 域名白名单    │
                    │ - 重定向域名    │
                    │ - 阻止 URL      │
                    │ - 强制重定向    │
                    └─────────────────┘
```

## SSL/TLS 处理

### HTTPS 解密流程

1. **隧道连接请求** - 客户端请求建立 HTTPS 隧道
2. **域名检查** - 判断是否需要解密此域名的流量
3. **证书生成** - 如需解密，生成伪造的服务器证书
4. **中间人代理** - 解密、检查、重定向请求
5. **重新加密** - 使用目标服务器证书重新加密（如启用 SSL）

### SSL 配置建议

| 场景 | EnableSsl | ValidateServerCertificate |
|------|-----------|---------------------------|
| 开发环境（自签名证书） | `true` | `false` |
| 生产环境（正式证书） | `true` | `true` |
| HTTP 服务器 | `false` | - |

## 性能考虑

- **AOT 编译** - 启动速度快，无 JIT 开销
- **异步处理** - 所有请求处理都是异步的
- **HashSet 查找** - BlockUrls 使用 HashSet 实现 O(1) 查找
- **延迟初始化** - 配置文件仅在需要时加载
