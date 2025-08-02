namespace TaskManagementAPI.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            // Log request
            _logger.LogInformation("Request {Method} {Path} started", 
                context.Request.Method, 
                context.Request.Path);

            try
            {
                await _next(context);
            }
            finally
            {
                stopwatch.Stop();
                
                // Log response
                _logger.LogInformation("Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
        }
    }
}
