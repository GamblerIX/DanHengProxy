// ============================================================================
// 文件: ProxyService.cs
// 描述: 代理服务核心实现 - 处理 HTTP/HTTPS 请求拦截和重定向
// 相关文件:
//   - Program.cs     : 创建和管理此服务实例
//   - ProxyConfig.cs : 配置数据模型
// ============================================================================

namespace DanHengProxy
{
    using System;
    using System.Net;
    using System.Net.Security;
    using System.Text;
    using System.Threading.Tasks;
    using Titanium.Web.Proxy;
    using Titanium.Web.Proxy.EventArguments;
    using Titanium.Web.Proxy.Models;

    /// <summary>
    /// 代理服务类
    /// 负责拦截 HTTP/HTTPS 请求并根据配置进行重定向或阻止
    /// </summary>
    internal class ProxyService
    {
        // ====================================================================
        // 私有字段
        // ====================================================================
        
        /// <summary>代理配置</summary>
        private readonly ProxyConfig _conf;
        
        /// <summary>Web 代理服务器实例</summary>
        private readonly ProxyServer _webProxyServer;
        
        /// <summary>目标重定向主机</summary>
        private readonly string _targetRedirectHost;
        
        /// <summary>目标重定向端口</summary>
        private readonly int _targetRedirectPort;
        
        /// <summary>是否使用 SSL 连接目标服务器</summary>
        private readonly bool _enableSsl;

        // ====================================================================
        // 构造函数
        // ====================================================================
        
        /// <summary>
        /// 初始化代理服务
        /// </summary>
        /// <param name="targetRedirectHost">目标重定向主机</param>
        /// <param name="targetRedirectPort">目标重定向端口</param>
        /// <param name="conf">代理配置</param>
        public ProxyService(string targetRedirectHost, int targetRedirectPort, ProxyConfig conf)
        {
            _conf = conf;
            _targetRedirectHost = targetRedirectHost;
            _targetRedirectPort = targetRedirectPort;
            _enableSsl = conf.EnableSsl;
            
            // 初始化代理服务器
            _webProxyServer = new ProxyServer();
            
            // 确保根证书存在（用于 HTTPS 解密）
            _webProxyServer.CertificateManager.EnsureRootCertificate();

            // 注册事件处理器
            _webProxyServer.BeforeRequest += BeforeRequest;
            _webProxyServer.ServerCertificateValidationCallback += OnCertValidation;

            // 确定绑定端口
            int port = conf.ProxyBindPort == 0 
                ? Random.Shared.Next(10000, 60000) 
                : conf.ProxyBindPort;
            
            // 设置并启动代理端点
            SetEndPoint(new ExplicitProxyEndPoint(IPAddress.Any, port, true));
            
            Console.WriteLine($"代理已绑定到端口: {port}");
        }

        // ====================================================================
        // 私有方法 - 代理端点设置
        // ====================================================================
        
        /// <summary>
        /// 设置代理端点并启动服务
        /// </summary>
        /// <param name="explicitEP">显式代理端点</param>
        private void SetEndPoint(ExplicitProxyEndPoint explicitEP)
        {
            // 注册隧道连接请求事件（用于 HTTPS）
            explicitEP.BeforeTunnelConnectRequest += BeforeTunnelConnectRequest;

            // 添加端点并启动服务器
            _webProxyServer.AddEndPoint(explicitEP);
            _webProxyServer.Start();

            // 在 Windows 上设置系统代理
            if (OperatingSystem.IsWindows())
            {
                _webProxyServer.SetAsSystemHttpProxy(explicitEP);
                _webProxyServer.SetAsSystemHttpsProxy(explicitEP);
            }
        }

        /// <summary>
        /// 关闭代理服务
        /// </summary>
        public void Shutdown()
        {
            _webProxyServer.Stop();
            _webProxyServer.Dispose();
        }

        // ====================================================================
        // 事件处理器
        // ====================================================================
        
        /// <summary>
        /// HTTPS 隧道连接请求前的处理
        /// 决定是否解密 SSL 流量
        /// </summary>
        private Task BeforeTunnelConnectRequest(object sender, TunnelConnectSessionEventArgs args)
        {
            string hostname = args.HttpClient.Request.RequestUri.Host;
            
            // 只对需要重定向的域名解密 SSL
            args.DecryptSsl = ShouldRedirect(hostname);

            return Task.CompletedTask;
        }

        /// <summary>
        /// SSL 证书验证回调
        /// </summary>
        private Task OnCertValidation(object sender, CertificateValidationEventArgs args)
        {
            // 如果配置允许跳过证书验证，或证书本身有效
            if (!_conf.ValidateServerCertificate || args.SslPolicyErrors == SslPolicyErrors.None)
            {
                args.IsValid = true;
            }

            return Task.CompletedTask;
        }
        
        /// <summary>
        /// HTTP 请求前的处理
        /// 根据配置进行重定向或阻止
        /// </summary>
        private Task BeforeRequest(object sender, SessionEventArgs args)
        {
            string hostname = args.HttpClient.Request.RequestUri.Host;
            string requestUrl = args.HttpClient.Request.Url;
            
            // 检查是否需要重定向
            if (ShouldRedirect(hostname) || ShouldForceRedirect(args.HttpClient.Request.RequestUri.AbsolutePath))
            {
                // 构建目标 URL（支持 SSL）
                string scheme = _enableSsl ? "https" : "http";
                Uri local = new($"{scheme}://{_targetRedirectHost}:{_targetRedirectPort}/");

                Uri builtUrl = new UriBuilder(requestUrl)
                {
                    Scheme = local.Scheme,
                    Host = local.Host,
                    Port = local.Port
                }.Uri;

                string replacedUrl = builtUrl.ToString();
                
                // 检查是否需要阻止此请求
                if (ShouldBlock(builtUrl))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[BLOCKED] {args.HttpClient.Request.RequestUri.AbsolutePath}");
                    Console.ResetColor();
                    
                    args.Respond(new Titanium.Web.Proxy.Http.Response(Encoding.UTF8.GetBytes("Blocked"))
                    {
                        StatusCode = 404,
                        StatusDescription = "Blocked",
                    }, true);
                    
                    return Task.CompletedTask;
                }

                // 执行重定向
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"[REDIRECT] {hostname} -> {_targetRedirectHost}:{_targetRedirectPort}");
                Console.ResetColor();
                
                args.HttpClient.Request.Url = replacedUrl;
            }

            return Task.CompletedTask;
        }

        // ====================================================================
        // 辅助方法 - 判断逻辑
        // ====================================================================
        
        /// <summary>
        /// 判断 URL 路径是否需要强制重定向
        /// </summary>
        /// <param name="path">URL 路径</param>
        /// <returns>是否需要强制重定向</returns>
        private bool ShouldForceRedirect(string path)
        {
            foreach (var keyword in _conf.ForceRedirectOnUrlContains)
            {
                if (path.Contains(keyword)) 
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断 URL 是否应该被阻止
        /// </summary>
        /// <param name="uri">请求 URI</param>
        /// <returns>是否应该阻止</returns>
        private bool ShouldBlock(Uri uri)
        {
            var path = uri.AbsolutePath;
            return _conf.BlockUrls.Contains(path);
        }

        /// <summary>
        /// 判断主机名是否需要重定向
        /// </summary>
        /// <param name="hostname">主机名</param>
        /// <returns>是否需要重定向</returns>
        private bool ShouldRedirect(string hostname)
        {
            // 移除端口号（如果有）
            if (hostname.Contains(':'))
                hostname = hostname[..hostname.IndexOf(':')];
            
            // 首先检查白名单（优先级最高）
            foreach (string domain in _conf.AlwaysIgnoreDomains)
            {
                if (hostname.EndsWith(domain))
                {
                    return false;
                }
            }
            
            // 检查是否匹配重定向域名
            foreach (string domain in _conf.RedirectDomains)
            {
                if (hostname.EndsWith(domain))
                    return true;
            }

            return false;
        }
    }
}
