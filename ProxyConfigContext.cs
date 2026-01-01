// ============================================================================
// 文件: ProxyConfigContext.cs
// 描述: JSON 序列化上下文（用于 AOT 编译支持）
// 相关文件:
//   - ProxyConfig.cs : 被序列化的配置类
//   - Program.cs     : 使用此上下文进行配置反序列化
// ============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace DanHengProxy;

#if NET8_0_OR_GREATER
/// <summary>
/// JSON 序列化源生成器上下文
/// 用于支持 AOT (Ahead-of-Time) 编译
/// 
/// 说明：
/// - .NET 8+ 的 AOT 编译要求使用源生成器进行 JSON 序列化
/// - 此类由编译器自动生成实现代码
/// - 允许 JSON 中包含尾随逗号和注释
/// </summary>
[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,           // 允许 JSON 尾随逗号
    ReadCommentHandling = JsonCommentHandling.Skip,  // 跳过 JSON 注释
    PropertyNameCaseInsensitive = true    // 属性名不区分大小写
)]
[JsonSerializable(typeof(ProxyConfig))]
internal partial class ProxyConfigContext : JsonSerializerContext
{
}
#endif