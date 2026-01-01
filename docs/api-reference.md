# API 参考

> 本文档提供 DanHengProxy 的配置项和代码接口的详细说明。

## 命令行参数参考

程序支持通过命令行参数配置运行模式和覆盖配置文件中的设置。

### 参数列表

| 参数 | 缩写 | 类型 | 说明 |
|------|------|------|------|
| `--headless` | `-H` | 布尔 | 启用无头模式，跳过所有交互确认 |
| `--quiet` | `-q` | 布尔 | 静默模式，只输出错误和关键信息 |
| `--host` | 无 | 字符串 | 覆盖目标服务器地址（优先于配置文件） |
| `--port` | `-p` | 整数 | 覆盖目标服务器端口（优先于配置文件） |
| `--ssl` | 无 | 布尔 | 启用 SSL（等同于 `EnableSsl=true`） |
| `--no-ssl` | 无 | 布尔 | 禁用 SSL（等同于 `EnableSsl=false`） |
| `--help` | `-?` | 布尔 | 显示帮助信息并退出 |

### 参数格式

支持两种参数格式：

```bash
# 空格分隔格式
DanHengProxy.exe --host 127.0.0.1 --port 443

# 等号格式
DanHengProxy.exe --host=127.0.0.1 --port=443
```

### 优先级

命令行参数 > 配置文件 (`config.json`)

---

## 配置文件参考

配置文件采用 JSON 格式，支持注释和尾随逗号。

### 完整配置结构

```json
{
  // 目标服务器配置
  "DestinationHost": "string",
  "DestinationPort": "number",
  
  // SSL 配置
  "EnableSsl": "boolean",
  "ValidateServerCertificate": "boolean",
  
  // 代理绑定配置
  "ProxyBindPort": "number",
  
  // 域名过滤
  "RedirectDomains": ["string"],
  "AlwaysIgnoreDomains": ["string"],
  
  // URL 过滤
  "ForceRedirectOnUrlContains": ["string"],
  "BlockUrls": ["string"]
}
```

---

## 目标服务器配置

### DestinationHost

| 属性 | 值 |
|------|------|
| 类型 | `string` |
| 必需 | ✅ 是 |
| 默认值 | `"localhost"` |

目标服务器的主机名或 IP 地址。

**示例：**
```json
"DestinationHost": "192.168.1.100"
```

---

### DestinationPort

| 属性 | 值 |
|------|------|
| 类型 | `number` |
| 必需 | ✅ 是 |
| 默认值 | `443` |
| 范围 | 1-65535 |

目标服务器的端口号。

**示例：**
```json
"DestinationPort": 21000
```

---

## SSL/TLS 配置

### EnableSsl

| 属性 | 值 |
|------|------|
| 类型 | `boolean` |
| 必需 | ❌ 否 |
| 默认值 | `false` |

是否使用 HTTPS 连接目标服务器。

| 值 | 行为 |
|------|------|
| `true` | 使用 `https://` 协议连接目标服务器 |
| `false` | 使用 `http://` 协议连接目标服务器 |

**示例：**
```json
"EnableSsl": true
```

---

### ValidateServerCertificate

| 属性 | 值 |
|------|------|
| 类型 | `boolean` |
| 必需 | ❌ 否 |
| 默认值 | `false` |

是否验证目标服务器的 SSL 证书。

| 值 | 行为 |
|------|------|
| `true` | 验证证书，无效证书将导致连接失败 |
| `false` | 跳过证书验证，允许自签名证书 |

> ⚠️ **安全提示**：生产环境建议设为 `true`。

**示例：**
```json
"ValidateServerCertificate": false
```

---

## 代理绑定配置

### ProxyBindPort

| 属性 | 值 |
|------|------|
| 类型 | `number` |
| 必需 | ❌ 否 |
| 默认值 | `0` |
| 范围 | 0-65535 |

代理服务器绑定的本地端口。

| 值 | 行为 |
|------|------|
| `0` | 自动选择 10000-60000 范围内的随机端口 |
| 其他 | 使用指定端口 |

**示例：**
```json
"ProxyBindPort": 8080
```

---

## 域名过滤配置

### RedirectDomains

| 属性 | 值 |
|------|------|
| 类型 | `string[]` |
| 必需 | ❌ 否 |
| 默认值 | `[]` |

需要重定向的域名后缀列表。

**匹配规则：** 请求域名以列表中的任一项**结尾**时触发重定向。

**示例：**
```json
"RedirectDomains": [
  ".mihoyo.com",
  ".hoyoverse.com"
]
```

| 请求域名 | 是否匹配 |
|----------|----------|
| `api.mihoyo.com` | ✅ 匹配 `.mihoyo.com` |
| `sdk.hoyoverse.com` | ✅ 匹配 `.hoyoverse.com` |
| `google.com` | ❌ 不匹配 |

---

### AlwaysIgnoreDomains

| 属性 | 值 |
|------|------|
| 类型 | `string[]` |
| 必需 | ❌ 否 |
| 默认值 | `[]` |

始终忽略的域名列表（白名单）。

**优先级：** 高于 `RedirectDomains`。

**用途：** 排除特定子域名，如游戏资源下载服务器。

**示例：**
```json
"AlwaysIgnoreDomains": [
  "autopatchcn.bhsr.com",
  "autopatchos.starrails.com"
]
```

---

## URL 过滤配置

### ForceRedirectOnUrlContains

| 属性 | 值 |
|------|------|
| 类型 | `string[]` |
| 必需 | ❌ 否 |
| 默认值 | `[]` |

强制重定向的 URL 关键字列表。

**匹配规则：** URL 路径**包含**列表中的任一项时，无论域名是否匹配都会重定向。

**示例：**
```json
"ForceRedirectOnUrlContains": [
  "query_dispatch",
  "query_gateway"
]
```

| URL | 是否匹配 |
|-----|----------|
| `/query_dispatch?version=1.0` | ✅ 包含 `query_dispatch` |
| `/api/data` | ❌ 不匹配 |

---

### BlockUrls

| 属性 | 值 |
|------|------|
| 类型 | `string[]` |
| 必需 | ❌ 否 |
| 默认值 | `[]` |

需要阻止的 URL 路径列表。

**匹配规则：** 请求路径**完全等于**列表中的任一项时，返回 404 响应。

**用途：** 阻止遥测、日志上报等请求。

**示例：**
```json
"BlockUrls": [
  "/sdk/dataUpload",
  "/crash/dataUpload",
  "/log"
]
```

---

## 代码接口参考

### ProxyService 类

#### 构造函数

```csharp
public ProxyService(
    string targetRedirectHost,
    int targetRedirectPort,
    ProxyConfig conf
)
```

| 参数 | 类型 | 说明 |
|------|------|------|
| targetRedirectHost | `string` | 目标重定向主机 |
| targetRedirectPort | `int` | 目标重定向端口 |
| conf | `ProxyConfig` | 代理配置对象 |

#### Shutdown 方法

```csharp
public void Shutdown()
```

关闭代理服务，释放资源，清理系统代理设置。

---

### ProxyConfig 类

```csharp
public class ProxyConfig
{
    // 目标服务器
    public required string DestinationHost { get; set; }
    public required int DestinationPort { get; set; }
    
    // SSL 配置
    public bool EnableSsl { get; set; } = false;
    public bool ValidateServerCertificate { get; set; } = false;
    
    // 代理绑定
    public int ProxyBindPort { get; set; } = 0;
    
    // 域名过滤
    public List<string> RedirectDomains { get; set; } = [];
    public List<string> AlwaysIgnoreDomains { get; set; } = [];
    
    // URL 过滤
    public List<string> ForceRedirectOnUrlContains { get; set; } = [];
    public HashSet<string> BlockUrls { get; set; } = [];
}
```

---

## 配置验证规则

| 项目 | 验证规则 |
|------|----------|
| DestinationHost | 不能为空 |
| DestinationPort | 必须在 1-65535 范围内 |
| ProxyBindPort | 必须在 0-65535 范围内 |
| RedirectDomains | 建议以 `.` 开头以匹配子域名 |

---

## 配置模板

完整的配置模板请参见项目根目录的 [config.tmpl.json](../config.tmpl.json) 文件。
