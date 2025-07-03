using IngesWebAPI.Data;

public class UpdateVideoCommand
{
    private readonly AppDbContext _ctx;
    public UpdateVideoCommand(AppDbContext ctx) => _ctx = ctx;

    public async Task<bool> ExecuteAsync(Guid id, string title, string summary)
    {
        var video = await _ctx.HomepageTopVideos.FindAsync(id);
        if (video == null) return false;

        video.Title = title;
        video.Summary = summary;
        video.IngestionTime = DateTime.UtcNow;

        await _ctx.SaveChangesAsync();
        return true;
    }
}
