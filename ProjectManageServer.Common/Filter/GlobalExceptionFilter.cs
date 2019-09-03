using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectManageServer.Common.Filter
{
    public class GlobalExceptionFilter : IExceptionFilter
    {

        public void OnException(ExceptionContext context)
        { 
            var json = new AjaxRspJson { RspCode = RspStatus.Failed, RspMsg = context.Exception.Message };

            context.Result = new ApplicationErrorResult(json);

            context.ExceptionHandled = true;
        }

        public class ApplicationErrorResult : ObjectResult
        {
            public ApplicationErrorResult(object value) : base(value)
            {
                StatusCode = (int)System.Net.HttpStatusCode.OK;
            }
        }


    }
}
