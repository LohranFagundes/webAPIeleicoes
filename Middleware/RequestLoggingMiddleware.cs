using System.Diagnostics;
using System.Text;

namespace ElectionApi.Net.Middleware;

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
        var stopwatch = Stopwatch.StartNew();
        
        // Log request
        await LogRequest(context);
        
        // Capture original body stream
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;
        
        await _next(context);
        
        stopwatch.Stop();
        
        // Log response
        await LogResponse(context, stopwatch.ElapsedMilliseconds);
        
        // Copy response back to original stream
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private async Task LogRequest(HttpContext context)
    {
        var request = context.Request;
        
        var requestInfo = new
        {
            Method = request.Method,
            Path = request.Path,
            QueryString = request.QueryString.ToString(),
            Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            UserAgent = request.Headers["User-Agent"].ToString(),
            IpAddress = context.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Incoming Request: {@RequestInfo}", requestInfo);

        // Log request body for POST/PUT requests (be careful with sensitive data)
        if (request.Method == "POST" || request.Method == "PUT")
        {
            request.EnableBuffering();
            var body = await ReadStreamInChunks(request.Body);
            request.Body.Position = 0;
            
            if (!string.IsNullOrEmpty(body) && !body.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation("Request Body: {Body}", body);
            }
        }
    }

    private async Task LogResponse(HttpContext context, long elapsedMs)
    {
        var response = context.Response;
        
        response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        var responseInfo = new
        {
            StatusCode = response.StatusCode,
            Headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
            ElapsedMs = elapsedMs,
            Timestamp = DateTime.UtcNow
        };

        _logger.LogInformation("Outgoing Response: {@ResponseInfo}", responseInfo);
        
        // Log response body if not too large
        if (responseBody.Length < 10000)
        {
            _logger.LogInformation("Response Body: {Body}", responseBody);
        }
    }

    private static async Task<string> ReadStreamInChunks(Stream stream)
    {
        const int readChunkBufferLength = 4096;
        stream.Seek(0, SeekOrigin.Begin);
        
        using var textWriter = new StringWriter();
        using var reader = new StreamReader(stream);
        
        var readChunk = new char[readChunkBufferLength];
        int readChunkLength;
        
        do
        {
            readChunkLength = await reader.ReadBlockAsync(readChunk, 0, readChunkBufferLength);
            textWriter.Write(readChunk, 0, readChunkLength);
        } while (readChunkLength > 0);
        
        return textWriter.ToString();
    }
}