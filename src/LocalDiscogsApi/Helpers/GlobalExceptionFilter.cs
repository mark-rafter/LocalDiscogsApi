using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace LocalDiscogsApi.Helpers
{
    public class GlobalExceptionFilter : IExceptionFilter // IAsyncExceptionFilter
    {
        public GlobalExceptionFilter()
        {
        }

        public void OnException(ExceptionContext context)
        {
            var result = new JsonResult(context.Exception.Message);

            switch (context.Exception)
            {
                default:
                    result.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Result = result;
        }
    }
}