using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text;
using PolicyEventHub.Extensions;
using HttpLoggingOptions = PolicyEventHub.Framework.Configurations.HttpLoggingOptions;

namespace PolicyEventHub.Framework.Middlewares
{
    public class RequestResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger;
        private readonly HttpLoggingOptions _options;

        public RequestResponseLoggingMiddleware(
            RequestDelegate next,
            ILogger<RequestResponseLoggingMiddleware> logger,
            IOptions<HttpLoggingOptions> options)
        {
            _next = next;
            _logger = logger;
            _options = options.Value;
        }

        public async Task Invoke(HttpContext context)
        {
            if (_options.ShouldSkip(context))
            {
                await _next(context);
                return;
            }

            var sw = Stopwatch.StartNew();
            var correlationId = context.GetCorrelationId();

            string? requestBody = null;
            string? responseBody = null;

            try
            {
                if (_options.LogRequestBody)
                {
                    requestBody = await TryReadRequestBodyAsync(context).ConfigureAwait(false);
                }

                var originalBodyStream = context.Response.Body;

                await using var responseBuffer = new MemoryStream();
                context.Response.Body = responseBuffer;

                try
                {
                    await _next(context);
                }
                finally
                {
                    if (_options.LogResponseBody)
                    {
                        responseBody = await TryReadResponseBodyAsync(context.Response, responseBuffer).ConfigureAwait(false);
                    }

                    responseBuffer.Position = 0;
                    await responseBuffer.CopyToAsync(originalBodyStream).ConfigureAwait(false);
                    context.Response.Body = originalBodyStream;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while processing HTTP request. CorrelationId={CorrelationId}", correlationId);
                throw;
            }
            finally
            {
                sw.Stop();

                var request = context.Request;
                var response = context.Response;

                var requestHeaders = GetHeaderSnapshot(request.Headers);
                var responseHeaders = GetHeaderSnapshot(response.Headers);

                _logger.LogInformation(
                    "HTTP {Method} {Path} responded {StatusCode} in {ElapsedMs} ms. CorrelationId={CorrelationId} " +
                    "RequestHeaders={RequestHeaders} ResponseHeaders={ResponseHeaders} RequestBody={RequestBody} ResponseBody={ResponseBody}",
                    request.Method,
                    request.Path.Value,
                    response.StatusCode,
                    sw.ElapsedMilliseconds,
                    correlationId,
                    requestHeaders,
                    responseHeaders,
                    requestBody,
                    responseBody);
            }
        }

        #region Helpers

        private IDictionary<string, string> GetHeaderSnapshot(IHeaderDictionary headers)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var kvp in headers)
            {
                var key = kvp.Key;
                var value = _options.SensitiveHeaders.Contains(key)
                    ? "***REDACTED***"
                    : kvp.Value.ToString();

                dict[key] = value;
            }

            return dict;
        }

        private async Task<string?> TryReadRequestBodyAsync(HttpContext context)
        {
            var request = context.Request;

            if (request.Body == null || !request.Body.CanRead)
                return null;

            if (!IsTextLike(request.ContentType))
                return null;

            request.EnableBuffering();

            try
            {
                using var buffer = new MemoryStream();

                await CopyToLimitedAsync(request.Body, buffer, _options.MaxBodySizeToLogBytes);
                request.Body.Position = 0;

                if (buffer.Length == 0)
                    return null;

                buffer.Position = 0;
                var bodyText = await new StreamReader(buffer, Encoding.UTF8).ReadToEndAsync();

                if (IsJson(request.ContentType))
                {
                    return RedactJson(bodyText);
                }

                return bodyText;
            }
            catch
            {
                // Do not break the pipeline because of logging
                return null;
            }
        }

        private async Task<string?> TryReadResponseBodyAsync(HttpResponse response, MemoryStream buffer)
        {
            if (buffer.Length == 0)
                return null;

            if (!IsTextLike(response.ContentType))
                return null;

            try
            {
                // response has already been written to buffer by pipeline
                var limited = new MemoryStream();
                buffer.Position = 0;
                await CopyToLimitedAsync(buffer, limited, _options.MaxBodySizeToLogBytes);
                limited.Position = 0;

                var bodyText = await new StreamReader(limited, Encoding.UTF8).ReadToEndAsync();

                if (IsJson(response.ContentType))
                {
                    return RedactJson(bodyText);
                }

                return bodyText;
            }
            catch
            {
                return null;
            }
        }

        private bool IsTextLike(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            var mediaType = contentType.Split(';', 2)[0].Trim();
            return _options.TextLikeContentTypes.Contains(mediaType);
        }

        private static bool IsJson(string? contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            var mediaType = contentType.Split(';', 2)[0].Trim();
            return mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase) ||
                   mediaType.Equals("application/problem+json", StringComparison.OrdinalIgnoreCase);
        }

        private async Task CopyToLimitedAsync(Stream source, Stream destination, long maxBytes)
        {
            var buffer = new byte[8 * 1024];
            long totalRead = 0;
            int read;

            while ((read = await source.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalRead += read;

                if (totalRead > maxBytes)
                {
                    await destination.WriteAsync(buffer.AsMemory(0, (int)(read - (totalRead - maxBytes))));
                    break;
                }

                await destination.WriteAsync(buffer.AsMemory(0, read));
            }
        }

        private string RedactJson(string json)
        {
            try
            {
                var node = JsonNode.Parse(json);
                if (node is null) return json;

                RedactNode(node);
                return node.ToJsonString();
            }
            catch
            {
                return json;
            }
        }

        private void RedactNode(JsonNode? node)
        {
            if (node is JsonObject obj)
            {
                foreach (var kvp in obj.ToList())
                {
                    var key = kvp.Key;
                    var child = kvp.Value;

                    if (child is JsonValue && _options.BodyRedactionKeys.Contains(key))
                    {
                        obj[key] = "***REDACTED***";
                    }
                    else
                    {
                        RedactNode(child);
                    }
                }
            }
            else if (node is JsonArray arr)
            {
                foreach (var child in arr)
                {
                    RedactNode(child);
                }
            }
        }

        #endregion
    }
}
