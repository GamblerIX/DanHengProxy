# 开发指南

> 本文档介绍如何设置开发环境、构建和调试 DanHengProxy。

## 环境要求

### 必需

| 工具 | 版本 | 说明 |
|------|------|------|
| .NET SDK | 8.0+ | [下载地址](https://dotnet.microsoft.com/download/dotnet/8.0) |
| Git | 任意 | 版本控制 |

### 推荐

| 工具 | 说明 |
|------|------|
| Visual Studio 2022 | Windows 推荐 IDE |
| Visual Studio Code | 跨平台轻量编辑器 |
| JetBrains Rider | 跨平台专业 IDE |

## 快速开始

### 1. 克隆仓库

```bash
git clone https://github.com/GamblerIX/DanHengProxy.git
cd DanHengProxy
```

### 2. 还原依赖

```bash
dotnet restore
```

### 3. 调试构建

```bash
dotnet build
```

### 4. 运行项目

```bash
dotnet run
```

## 构建选项

### 调试构建

```bash
dotnet build -c Debug
```

输出目录：`bin/Debug/net8.0/`

### 发布构建

```bash
dotnet build -c Release
```

输出目录：`bin/Release/net8.0/`

### AOT 发布

```bash
dotnet publish -c Release
```

输出目录：`bin/Release/net8.0/publish/`

> ⚠️ AOT 编译需要对应平台的原生工具链。在 Windows 上需要安装 Visual Studio C++ 工具集。

### 跨平台发布

```bash
# Windows x64
dotnet publish -c Release -r win-x64

# Linux x64
dotnet publish -c Release -r linux-x64

# macOS x64
dotnet publish -c Release -r osx-x64

# macOS ARM64 (Apple Silicon)
dotnet publish -c Release -r osx-arm64
```

## 项目结构

```
DanHengProxy/
├── DanHengProxy.sln          # 解决方案文件
├── DanHengProxy.csproj       # 项目文件
│
├── Program.cs                # 程序入口
│   └── Main()                # 入口方法
│   └── ParseArgs()           # 命令行参数解析
│   └── ApplyCommandLineOverrides() # 应用参数覆盖
│   └── CheckProxy()          # 代理检测
│   └── InitConfig()          # 配置初始化
│   └── PrintHelp()           # 帮助信息
│
├── ProxyService.cs           # 代理服务
│   └── SetEndPoint()         # 端点设置
│   └── BeforeRequest()       # 请求处理
│   └── ShouldRedirect()      # 域名判断
│
├── ProxyConfig.cs            # 配置模型
│   └── SSL 配置
│   └── 域名配置
│   └── URL 配置
│
├── ProxyConfigContext.cs     # JSON 上下文
│
└── config.tmpl.json          # 配置模板
```

## 调试技巧

### Visual Studio

1. 打开 `DanHengProxy.sln`
2. 设置断点
3. 按 `F5` 开始调试

### VS Code

1. 安装 C# 扩展
2. 打开项目文件夹
3. 按 `F5` 选择 .NET Core 调试

### 命令行调试

```bash
# 启用详细日志
dotnet run --verbosity detailed
```

## 代码规范

### 命名约定

| 类型 | 约定 | 示例 |
|------|------|------|
| 类 | PascalCase | `ProxyService` |
| 方法 | PascalCase | `ShouldRedirect` |
| 私有字段 | _camelCase | `_webProxyServer` |
| 静态字段 | s_camelCase | `s_proxyService` |
| 常量 | PascalCase | `ConfigPath` |
| 参数 | camelCase | `targetHost` |

### 注释规范

所有公共成员必须包含 XML 文档注释：

```csharp
/// <summary>
/// 判断主机名是否需要重定向
/// </summary>
/// <param name="hostname">主机名</param>
/// <returns>是否需要重定向</returns>
private bool ShouldRedirect(string hostname)
```

### 文件头注释

每个源文件应包含文件头注释：

```csharp
// ============================================================================
// 文件: FileName.cs
// 描述: 文件功能描述
// 相关文件:
//   - RelatedFile1.cs : 说明
//   - RelatedFile2.cs : 说明
// ============================================================================
```

## 添加新功能

### 示例：添加新的配置项

1. **修改 ProxyConfig.cs**

```csharp
/// <summary>
/// 新配置项说明
/// </summary>
public bool NewFeatureEnabled { get; set; } = false;
```

2. **更新 config.tmpl.json**

```json
{
  // 新功能开关
  "NewFeatureEnabled": false
}
```

3. **在 ProxyService.cs 中使用**

```csharp
if (_conf.NewFeatureEnabled)
{
    // 新功能逻辑
}
```

4. **更新文档**

- 更新 `README.md` 配置说明
- 更新 `docs/api-reference.md`

## 常见问题

### AOT 编译失败

确保安装了 Visual Studio C++ 工具集：

1. 打开 Visual Studio Installer
2. 选择"使用 C++ 的桌面开发"工作负载
3. 确保勾选"MSVC v143"和"Windows SDK"

### JSON 序列化错误

AOT 模式下必须使用源生成器。确保：

1. `ProxyConfigContext` 类包含 `[JsonSerializable(typeof(ProxyConfig))]` 特性
2. 使用 `ProxyConfigContext.Default.ProxyConfig` 进行序列化

### 系统代理未生效

检查是否有其他代理软件占用。可以手动设置：

1. 打开 Windows 设置 → 网络 → 代理
2. 手动设置代理服务器地址和端口

## 贡献指南

1. Fork 本仓库
2. 创建功能分支：`git checkout -b feature/new-feature`
3. 提交更改：`git commit -am 'Add new feature'`
4. 推送分支：`git push origin feature/new-feature`
5. 创建 Pull Request
