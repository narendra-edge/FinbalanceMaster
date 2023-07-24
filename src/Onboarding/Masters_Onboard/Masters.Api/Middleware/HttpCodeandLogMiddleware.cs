using Masters.Core.Exceptions;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System.Net;
using System.Security.Authentication;
using System.Text;

namespace Masters.Api.Middleware
{
    public static class HttpStatusCodeExceptionMiddlewareExtentions
    {
       public static IApplicationBuilder UseHttpCodeAndLogMiddleware(this IApplicationBuilder builder)
       {
        return builder.UseMiddleware<HttpCodeandLogMiddleware>();
       }

    }
    public  class HttpCodeandLogMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HttpCodeandLogMiddleware> _logger;

        public HttpCodeandLogMiddleware(RequestDelegate next, ILogger<HttpCodeandLogMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

    public async Task Invoke(HttpContext httpContext)
        {
            if (httpContext == null) 
            {
                return;
            }
            try
            {
                httpContext.Request.EnableBuffering();
                await _next(httpContext);
                
            }
            catch (Exception exception)
            {
                var response = httpContext.Response;
                response.ContentType = "application/json";
                switch (exception)
                {
                    case BadHttpRequestException e:
                        //Custom Application Error
                        httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await WriteAndLogResponseasync(exception, httpContext,HttpStatusCode.BadRequest,LogLevel.Error,"BadRequestException" + e.Message);
                        break;
                    case NotFoundException e:
                        //Not Found Error
                        httpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        await WriteAndLogResponseasync(exception, httpContext, HttpStatusCode.NotFound, LogLevel.Error, "NotFoundException" + e.Message);
                        break;
                    case ValidationException e:
                        // Validation Error
                        httpContext.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
                        await WriteAndLogResponseasync(exception, httpContext, HttpStatusCode.UnprocessableEntity, LogLevel.Error, "ValidationException" + e.Message);
                        break;
                    case UnauthorizedAccessException e:
                        //Authentication Error
                        httpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        await WriteAndLogResponseasync(exception, httpContext, HttpStatusCode.Unauthorized, LogLevel.Error, "AuthenticationException" + e.Message);
                        break;
                    default:
                        // Unhaddled Error
                        await WriteAndLogResponseasync(exception, httpContext, HttpStatusCode.InternalServerError, LogLevel.Error, "ServerError!");
                        break;
                }                
            }
        }
        public async Task WriteAndLogResponseasync(
            Exception exception,
            HttpContext httpContext,
            HttpStatusCode httpStatusCode,
            LogLevel logLevel,
            string alternateMessage = null)
        {
            string requestBody = string.Empty;
            if(httpContext.Request.Body.CanSeek  && httpContext.Request.Body.Length >0)
            {
                httpContext.Request.Body.Seek(0, System.IO.SeekOrigin.Begin);
                using (var sr = new System.IO.StreamReader(httpContext.Request.Body))
                {
                    var streamoutput = sr.ReadToEndAsync();
                    requestBody = JsonConvert.DeserializeObject(streamoutput.Result).ToString();
                }
            }
            StringValues authorization;
            httpContext.Request.Headers.TryGetValue("Authorization", out authorization);

            var customDetails = new StringBuilder();
            customDetails
            .Append("\n  Service Url       :").Append(httpContext.Request.Path.ToString())
            .Append("\n  Request Method    :").Append(httpContext.Request?.Method)
            .Append("\n  Request Body      :").Append(requestBody)
            .Append("\n  Authorization     :"). Append(authorization)
            .Append("\n  Content-Type      :").Append(httpContext.Request.Headers["Content-Type"].ToString())
            .Append("\n  Cookie            :").Append(httpContext.Request.Headers["Cookie"].ToString())
            .Append("\n  Host              :").Append(httpContext.Request.Headers["Host"].ToString())
            .Append("\n  Referer           :").Append(httpContext.Request.Headers["Referer"].ToString())
            .Append("\n  Origin            :").Append(httpContext.Request.Headers["Origin"].ToString())
            .Append("\n  User-Agent        :").Append(httpContext.Request.Headers["User-Agent"].ToString())
            .Append("\n  Error Message     :").Append(exception.Message);

            _logger.Log(logLevel,exception,customDetails.ToString());

            if (httpContext.Response.HasStarted) 
            {
                _logger.LogError("The response has already started,the http stauts code middleware will not be excecuted.");
                return;
            }

            string responseMessage = JsonConvert.SerializeObject(
                new
                {
                    Message = string.IsNullOrWhiteSpace(exception.Message) ? alternateMessage : exception.Message,
                });

            httpContext.Response.Clear();
            httpContext.Response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
            httpContext.Response.StatusCode =(int)httpStatusCode;
            await httpContext.Response.WriteAsync(responseMessage, Encoding.UTF8);
        }
    }
}
