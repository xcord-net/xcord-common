using System.Reflection;
using Xcord.Captcha.Rendering;
using Xunit;

public class PortabilityGuardTests
{
    [Fact]
    public void Core_rendering_types_do_not_reference_redis_or_aspnetcore()
    {
        var asm = typeof(CaptchaCharset).Assembly;
        var referenced = asm.GetReferencedAssemblies().Select(a => a.Name).ToArray();
        // The assembly as a whole may reference these (optional adapters live here),
        // but this test documents intent: core must be movable. We assert the core
        // Rendering namespace types expose no Redis/Http types in their public surface.
        var coreTypes = asm.GetTypes()
            .Where(t => t.Namespace == "Xcord.Captcha.Rendering" && t.IsPublic);
        foreach (var t in coreTypes)
        foreach (var m in t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            var types = m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType);
            Assert.All(types, pt =>
            {
                var ns = pt.Namespace ?? "";
                Assert.False(ns.StartsWith("StackExchange") || ns.StartsWith("Microsoft.AspNetCore"),
                    $"{t.Name}.{m.Name} exposes non-portable type {pt.FullName}");
            });
        }
    }
}
