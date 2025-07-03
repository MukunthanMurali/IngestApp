using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

public class IngestTopVideosFunction
{
    private static readonly HttpClient client = new HttpClient();
    private readonly ILogger _logger;

    public IngestTopVideosFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<IngestTopVideosFunction>();
    }

    [Function("IngestTopVideos")]
    public async Task RunAsync([TimerTrigger("0/5 * * * * *")] TimerInfo timerInfo)
    {
        try
        {
            var url = Environment.GetEnvironmentVariable("SOURCE_URL");
            var json = await client.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var connStr = Environment.GetEnvironmentVariable("SQL_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connStr))
            {
                _logger.LogError("SQL_CONNECTION_STRING is not configured.");
                return;
            }

            using var conn = new SqlConnection(connStr);
            await conn.OpenAsync();

            var videos = doc.RootElement.GetProperty("homepage_top_videos").EnumerateArray();

            foreach (var v in videos)
            {
                var id = Guid.NewGuid();

                string title = v.TryGetProperty("title", out var titleProp) && titleProp.ValueKind == JsonValueKind.String
                    ? titleProp.GetString() ?? ""
                    : "";

                string summary = v.TryGetProperty("summary", out var summaryProp) && summaryProp.ValueKind == JsonValueKind.String
                    ? summaryProp.GetString() ?? ""
                    : "";

                string facebook = "";
                string twitter = "";

                if (v.TryGetProperty("share_links", out var shareLinksProp) && shareLinksProp.ValueKind == JsonValueKind.Object)
                {
                    if (shareLinksProp.TryGetProperty("facebook", out var fbProp) && fbProp.ValueKind == JsonValueKind.String)
                        facebook = fbProp.GetString() ?? "";

                    if (shareLinksProp.TryGetProperty("twitter", out var twProp) && twProp.ValueKind == JsonValueKind.String)
                        twitter = twProp.GetString() ?? "";
                }

                var cmd = new SqlCommand(@"
              MERGE HomepageTopVideos AS target
                USING (SELECT @Title AS Title, @Summary AS Summary, @Facebook AS Facebook, @Twitter AS Twitter) AS source
                ON target.Title = source.Title
                   AND target.Summary = source.Summary
                   AND target.Facebook = source.Facebook
                   AND target.Twitter = source.Twitter
                WHEN MATCHED THEN
                    UPDATE SET Id = @Id, IngestionTime = GETUTCDATE()
                WHEN NOT MATCHED THEN
                    INSERT (Id, Title, Summary, Facebook, Twitter, IngestionTime)
                    VALUES (@Id, @Title, @Summary, @Facebook, @Twitter, GETUTCDATE());", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Title", title);
                cmd.Parameters.AddWithValue("@Summary", summary);
                cmd.Parameters.AddWithValue("@Facebook", facebook);
                cmd.Parameters.AddWithValue("@Twitter", twitter);

                await cmd.ExecuteNonQueryAsync();
            }

            _logger.LogInformation($"Ingested top videos at {DateTime.UtcNow}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during ingestion: {ex.Message}");
        }
    }
}
