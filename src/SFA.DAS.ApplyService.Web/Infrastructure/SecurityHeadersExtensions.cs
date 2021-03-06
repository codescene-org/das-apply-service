using Microsoft.AspNetCore.Builder;

namespace SFA.DAS.ApplyService.Web.Infrastructure
{
    public static class SecurityHeadersExtensions
    {
        public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                context.Response.Headers["X-Frame-Options"] = "SAMEORIGIN";
                context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
                context.Response.Headers["X-Content-Type-Options"] = "nosniff";
                context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; img-src 'self' *.google-analytics.com; script-src 'self' 'unsafe-inline' *.googletagmanager.com *.postcodeanywhere.co.uk *.google-analytics.com *.googleapis.com; font-src 'self' data:;";
                context.Response.Headers["Referrer-Policy"] = "strict-origin";
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                await next();
            });

            return app;
        }
    }
}
