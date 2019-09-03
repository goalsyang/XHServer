using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectManageServer.Common.Filter

{
    public class CustomerActionFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //throw new System.NotImplementedException();
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            var flag = context.HttpContext.Request.Path.Value.ToLower() == "/api/Authorization/AllErrorHandler";

            if (flag)
            {
                context.HttpContext.Response.StatusCode = 200;
            }
        }
    }
}
