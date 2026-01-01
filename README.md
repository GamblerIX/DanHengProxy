# DanHengProxy

<p align="center">
  <img src="DanHeng.ico" alt="DanHengProxy Logo" width="128" height="128">
</p>

<p align="center">
  <strong>一款为DanHengServer量身定制的简单、高效游戏重定向工具</strong>
</p>

---

## 🚀 快速开始

### 1. 下载程序

在 [Releases 页面](https://github.com/GamblerIX/DanHengProxy/releases) 下载适合你系统的压缩包。

### 2. 简单三步走

1. **解压并运行**：双击运行 `DanHengProxy.exe`（Windows 用户）。程序会提示你它是第一次运行。
2. **修改设置**：程序目录下会生成一个 `config.json`。用记事本打开它，修改 `DestinationHost` 为你的目标服务器地址。
3. **重新启动**：关掉程序再开一次，看到“代理已启动”的提示后，直接启动游戏即可！

> 💡 **小提示**：如果你的服务器需要 HTTPS，请确保配置文件中的 `"EnableSsl": true`。

## ⚠️ 使用注意

- **关闭其他代理**：运行前请先关掉 Clash、V2Ray、Fiddler 等可能会抢占封包的软件。
- **正常关闭**：想要停止时，请在窗口按 `Ctrl+C` 或者直接点叉叉。程序会自动帮你把系统网络设置改回来。
- **证书问题**：如果游戏提示连接不安全，通常是因为服务器证书问题，你可以在配置中将 `ValidateServerCertificate` 设为 `false`。

## 🤖 无头模式（脚本集成）

适合一键启动脚本或自动化场景，无需用户交互。

### 命令行参数

| 参数 | 说明 |
|------|------|
| `--headless`, `-H` | 无头模式，跳过所有交互确认 |
| `--quiet`, `-q` | 静默模式，只输出关键信息 |
| `--host <地址>` | 覆盖目标服务器地址 |
| `--port`, `-p <端口>` | 覆盖目标服务器端口 |
| `--ssl` | 启用 SSL/HTTPS 连接 |
| `--no-ssl` | 禁用 SSL/HTTPS 连接 |
| `--help` | 显示帮助信息 |

### 使用示例

```bash
# 无头模式启动（使用 config.json 配置）
DanHengProxy.exe --headless

# 无头模式 + 静默模式
DanHengProxy.exe -H -q

# 覆盖服务器配置（一键启动）
DanHengProxy.exe -H --host 192.168.1.100 --port 21000 --ssl
```

### 批处理脚本示例

```batch
@echo off
start "" /B DanHengProxy.exe -H -q --host 127.0.0.1 --port 443 --ssl
echo 代理已启动，按任意键停止...
pause > nul
taskkill /IM DanHengProxy.exe /F > nul 2>&1
```

## 📂 进阶与开发

如果你是开发者或者想要更改更高级的设置：

- [详细配置手册](./docs/api-reference.md) - 每一个配置项的详细含义。
- [开发与构建指南](./docs/development.md) - 如何从源代码自己编译这个程序。
- [架构原理](./docs/architecture.md) - 这个程序是怎么工作的。

---

## 许可证

本项目基于 [GNU AGPLv3](LICENSE) 许可证开源。
