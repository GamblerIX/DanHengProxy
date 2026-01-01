// ============================================================================
// 文件: Program.cs
// 描述: DanHengProxy 程序入口点
// 相关文件:
//   - ProxyService.cs      : 代理服务核心实现
//   - ProxyConfig.cs       : 配置数据模型
//   - ProxyConfigContext.cs: JSON 序列化上下文
//   - config.json          : 运行时配置文件
// ============================================================================

using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace DanHengProxy
{
    /// <summary>
    /// DanHengProxy 主程序类
    /// 负责初始化配置、启动代理服务、处理程序生命周期
    /// </summary>
    internal static class Program
    {
        // ====================================================================
        // 常量定义 - 便于统一修改
        // ====================================================================
        
        /// <summary>控制台窗口标题</summary>
        private const string Title = "DanHengProxy";
        
        /// <summary>运行时配置文件路径</summary>
        private const string ConfigPath = "config.json";
        
        /// <summary>配置模板文件路径</summary>
        private const string ConfigTemplatePath = "config.tmpl.json";

        // ====================================================================
        // 静态字段
        // ====================================================================
        
        /// <summary>代理服务实例</summary>
        private static ProxyService s_proxyService = null!;
        
        /// <summary>是否已执行清理操作（防止重复清理）</summary>
        private static bool s_cleanedUp = false;
        
        // ====================================================================
        // 主程序入口
        // ====================================================================
        
        /// <summary>
        /// 程序主入口点
        /// </summary>
        /// <param name="args">命令行参数（暂未使用）</param>
        private static void Main(string[] args)
        {
            // 设置控制台标题
            Console.Title = Title;
            PrintBanner();
            
            // 检查是否有其他代理软件在运行
            CheckProxy();
            
            // 初始化配置文件
            InitConfig();

            // 加载配置
            var conf = JsonSerializer.Deserialize(
                File.ReadAllText(ConfigPath), 
                ProxyConfigContext.Default.ProxyConfig
            ) ?? throw new FileLoadException("请正确配置 config.json 文件。");
            
            // 启动代理服务
            s_proxyService = new ProxyService(conf.DestinationHost, conf.DestinationPort, conf);
            
            // 显示运行状态
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✓ 代理服务已启动");
            Console.WriteLine($"  目标服务器: {(conf.EnableSsl ? "https" : "http")}://{conf.DestinationHost}:{conf.DestinationPort}");
            Console.WriteLine($"  SSL 模式: {(conf.EnableSsl ? "已启用" : "已禁用")}");
            Console.ResetColor();
            Console.WriteLine("\n按 Ctrl+C 停止代理服务...\n");
            
            // 注册退出事件处理
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            Console.CancelKeyPress += OnProcessExit;

            // 阻塞主线程，保持程序运行
            Thread.Sleep(-1);
        }

        // ====================================================================
        // 辅助方法
        // ====================================================================

        /// <summary>
        /// 打印程序横幅
        /// </summary>
        private static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
  ____              _   _                   ____                      
 |  _ \  __ _ _ __ | | | | ___ _ __   __ _ |  _ \ _ __ _____  ___   _ 
 | | | |/ _` | '_ \| |_| |/ _ \ '_ \ / _` || |_) | '__/ _ \ \/ / | | |
 | |_| | (_| | | | |  _  |  __/ | | | (_| ||  __/| | | (_) >  <| |_| |
 |____/ \__,_|_| |_|_| |_|\___|_| |_|\__, ||_|   |_|  \___/_/\_\\__, |
                                     |___/                      |___/ 
");
            Console.ResetColor();
            Console.WriteLine("  Version 2.1.0 - HTTPS Proxy with SSL Support\n");
        }

        /// <summary>
        /// 初始化配置文件
        /// 如果配置文件不存在，从模板复制一份
        /// </summary>
        private static void InitConfig()
        {
            if (!File.Exists(ConfigPath))
            {
                if (!File.Exists(ConfigTemplatePath))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("错误: 找不到配置模板文件 config.tmpl.json");
                    Console.ResetColor();
                    Environment.Exit(1);
                }
                File.Copy(ConfigTemplatePath, ConfigPath);
                Console.WriteLine($"已创建配置文件: {ConfigPath}");
            }
        }

        /// <summary>
        /// 程序退出事件处理
        /// 确保代理服务正确关闭
        /// </summary>
        private static void OnProcessExit(object? sender, EventArgs? args)
        {
            if (s_cleanedUp) return;
            
            Console.WriteLine("\n正在关闭代理服务...");
            s_proxyService?.Shutdown();
            s_cleanedUp = true;
            Console.WriteLine("代理服务已停止。");
        }

        /// <summary>
        /// 检查是否有其他代理软件正在运行
        /// 如果检测到其他代理，提示用户确认
        /// </summary>
        public static void CheckProxy()
        {
            try
            {
                string? proxyInfo = GetProxyInfo();
                if (proxyInfo != null)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("⚠ 检测到系统代理");
                    Console.ResetColor();
                    Console.WriteLine($"  当前系统代理: {proxyInfo}");
                    Console.WriteLine("  如果您正在使用 Clash、V2RayN、Fiddler 等代理软件，");
                    Console.WriteLine("  请先关闭它们以确保 DanHengProxy 能够正常工作。");
                    Console.WriteLine("\n如果您确认没有问题，请按任意键继续...");
                    Console.ReadKey(true);
                    Console.WriteLine();
                }
            }
            catch (NullReferenceException)
            {
                // 忽略空引用异常
            }
        }

        /// <summary>
        /// 获取系统当前代理信息
        /// </summary>
        /// <returns>代理地址字符串，格式为 "IP:Port"；如果没有代理则返回 null</returns>
        public static string? GetProxyInfo()
        {
            try
            {
                IWebProxy proxy = WebRequest.GetSystemWebProxy();
                Uri testUri = new("https://www.cloudflare.com");
                Uri? proxyUri = proxy.GetProxy(testUri);
                
                // 如果代理地址等于原始地址，说明没有使用代理
                if (proxyUri == null || proxyUri.Equals(testUri)) 
                    return null;

                return $"{proxyUri.Host}:{proxyUri.Port}";
            }
            catch
            {
                return null;
            }
        }
    }
}
