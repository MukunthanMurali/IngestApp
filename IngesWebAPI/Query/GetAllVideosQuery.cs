using IngesWebAPI.Data;
using IngesWebAPI.Models;
using Microsoft.EntityFrameworkCore;

public class GetAllVideosQuery
{
    private readonly AppDbContext _ctx;
    public GetAllVideosQuery(AppDbContext ctx) => _ctx = ctx;

    public async Task<List<Video>> ExecuteAsync()
    {
        return await _ctx.HomepageTopVideos
                         .OrderByDescending(v => v.IngestionTime)
                         .ToListAsync();
    }
}
