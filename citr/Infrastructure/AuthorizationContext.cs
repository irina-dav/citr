using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace citr.Infrastructure
{
    public class AuthorizationContext
    {
    }

    public class HFAuthorizationFilter : IDashboardAuthorizationFilter
    {

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            if (httpContext.User.IsInRole("Admins"))
            {
                return true;
            }

            return false;
        }
    }
}
