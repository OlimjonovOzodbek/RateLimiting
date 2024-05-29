using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RateLimiting.API
{
    public class RateLimiter
    {
        private int _requestLimit;
        private readonly RequestDelegate _next;
        private DateTime _resetTime;

        public RateLimiter(RequestDelegate next)
        {
            _requestLimit = 0;
            _next = next;
            _resetTime = DateTime.UtcNow.AddSeconds(20);
        }

        public async Task InvokeAsync(HttpContext context)
        { 
            var currentTime = DateTime.UtcNow;

            if (currentTime >= _resetTime && _requestLimit != 0)
            {
                _requestLimit = 0;
                _resetTime = currentTime.AddSeconds(20);
            }

            if (_requestLimit < 5)
            {
                _requestLimit++;
                await _next(context);
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.Response.WriteAsync("Rate limit exceeded. Try again later.");
                return;
            }
        }
    }
}
