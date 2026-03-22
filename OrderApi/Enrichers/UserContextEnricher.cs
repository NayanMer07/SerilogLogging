using Serilog.Core;
using Serilog.Events;

namespace OrderApi.Enrichers
{
    public class UserContextEnricher(IHttpContextAccessor accessor) : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory factory)
        {
            var user = accessor?.HttpContext?.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                logEvent.AddPropertyIfAbsent(
                    factory.CreateProperty("UserId",
                        user.FindFirst("sub")?.Value));
                logEvent.AddPropertyIfAbsent(
                    factory.CreateProperty("UserEmail",
                        user.FindFirst("email")?.Value));
            }
        }
    }
}
