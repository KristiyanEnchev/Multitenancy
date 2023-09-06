namespace Multitenant.WEB.Extensions.Healtchecks
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Multitenant.Application.Interfaces.Utility.Serializer;
    using Newtonsoft.Json;

    internal class CustomHealthCheckResponseWriter
    {
        public class Healthresult
        {
            public string? Status { get; set; }
            public int TotalChecks { get; set; }
            public List<Entry> Entries { get; set; }
        }

        public class Entry
        {
            public string? Name { get; set; } = null;
            public TimeSpan? Duration { get; set; }
            //public string? Description { get; set; }
            public string? Status { get; set; }
            public string? Exception { get; set; }
        }


        public static Task WriteResponse(HttpContext httpContext, HealthReport result, ISerializerService jsonSerializer)
        {
            var response = new Healthresult
            {
                Status = result.Status.ToString(),
                TotalChecks = result.Entries.Count,
                Entries = result.Entries.Select(e => new Entry
                {
                    Name = e.Key,
                    Status = e.Value.Status.ToString(),
                    Duration = e.Value.Duration,
                    Exception = e.Value.Exception?.Message
                }).ToList()
            };
            //var response = new
            //{
            //    Status = result.Status.ToString(),
            //    TotalChecks = result.Entries.Count,
            //    Entries = result.Entries.Select(e => new
            //    {
            //        Name = e.Key,
            //        Status = e.Value.Status.ToString(),
            //        Description = e.Value.Description,
            //        Exception = e.Value.Exception?.Message
            //    })
            //};



            var jsonResponse = jsonSerializer.Serialize(response);
            //var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);
            httpContext.Response.ContentType = "application/json";

            if (result.Status != HealthStatus.Healthy)
            {
                httpContext.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            }

            return httpContext.Response.WriteAsync(jsonResponse);
        }
    }
}
