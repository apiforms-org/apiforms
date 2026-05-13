using System.Security.Claims;

namespace APIForms.Api.Security;

public static class TenantContext
{
    public static string GetTenantId(HttpContext httpContext)
    {
        var claimTenant = httpContext.User.FindFirstValue("tenantId");
        if (!string.IsNullOrWhiteSpace(claimTenant)) return claimTenant;

        if (httpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var values)
            && !string.IsNullOrWhiteSpace(values.ToString()))
        {
            return values.ToString();
        }

        throw new UnauthorizedAccessException("Missing tenantId claim");
    }
}
