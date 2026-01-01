// ============================================================================
// 文件: ProxyConfig.cs
// 描述: 代理服务配置数据模型
// 相关文件:
//   - Program.cs           : 使用此配置初始化代理服务
//   - ProxyService.cs      : 读取配置进行请求处理
//   - ProxyConfigContext.cs: JSON 序列化上下文
//   - config.tmpl.json     : 配置模板文件
// ============================================================================

namespace DanHengProxy;

/// <summary>
/// 代理服务配置类
/// 定义了代理服务运行所需的所有配置项
/// </summary>
public class ProxyConfig
{
    // ========================================================================
    // 目标服务器配置
    // ========================================================================
    
    /// <summary>
    /// 目标重定向主机地址
    /// 所有匹配的请求将被转发到此主机
    /// </summary>
    public required string DestinationHost { get; set; }
    
    /// <summary>
    /// 目标重定向端口
    /// </summary>
    public required int DestinationPort { get; set; }
    
    // ========================================================================
    // SSL/TLS 配置
    // ========================================================================
    
    /// <summary>
    /// 是否启用 SSL/TLS 连接到目标服务器
    /// 当为 true 时，将使用 HTTPS 连接目标服务器
    /// </summary>
    public bool EnableSsl { get; set; } = false;
    
    /// <summary>
    /// 是否验证目标服务器的 SSL 证书
    /// 设为 false 可以允许自签名证书（开发环境使用）
    /// </summary>
    public bool ValidateServerCertificate { get; set; } = false;
    
    // ========================================================================
    // 代理绑定配置
    // ========================================================================
    
    /// <summary>
    /// 代理服务绑定端口
    /// 设为 0 时将随机选择一个可用端口
    /// </summary>
    public int ProxyBindPort { get; set; } = 0;
    
    // ========================================================================
    // 域名过滤配置
    // ========================================================================
    
    /// <summary>
    /// 需要重定向的域名列表
    /// 匹配规则：请求域名以列表中的任一项结尾时触发重定向
    /// 例如: ".mihoyo.com" 会匹配 "api.mihoyo.com"
    /// </summary>
    public List<string> RedirectDomains { get; set; } = [];
    
    /// <summary>
    /// 始终忽略的域名列表（白名单）
    /// 优先级高于 RedirectDomains
    /// 用于排除特定子域名，如更新服务器
    /// </summary>
    public List<string> AlwaysIgnoreDomains { get; set; } = [];
    
    // ========================================================================
    // URL 过滤配置
    // ========================================================================
    
    /// <summary>
    /// 强制重定向的 URL 关键字列表
    /// 当 URL 路径包含这些关键字时，无论域名是否匹配都会重定向
    /// </summary>
    public List<string> ForceRedirectOnUrlContains { get; set; } = [];
    
    /// <summary>
    /// 需要阻止的 URL 路径集合
    /// 匹配的请求将返回 404 响应
    /// 用于阻止遥测和日志上报
    /// </summary>
    public HashSet<string> BlockUrls { get; set; } = [];
}
