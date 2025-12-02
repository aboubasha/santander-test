namespace TestApi;

public class HnCacheService
{
    private readonly IHttpClientFactory _factory;
    private HnItem[]? _cache;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TimeSpan _refreshInterval = TimeSpan.FromMinutes(15);
    private DateTimeOffset _lastRefresh = DateTimeOffset.MinValue;

    public HnCacheService(IHttpClientFactory factory)
    {
        _factory = factory;
    }

    public async Task<object[]> GetStories()
    {
        if (_cache != null && DateTimeOffset.UtcNow - _lastRefresh < _refreshInterval)
        {
            return _cache;
        }

        // Use a semaphore so a single refresh runs at a time
        await _semaphore.WaitAsync();
        try
        {
            if (_cache != null && DateTimeOffset.UtcNow - _lastRefresh < _refreshInterval)
            {
                return _cache;
            }

            try
            {
                await RefreshInternal();
            }
            catch (Exception)
            {
                // Ensure cache is at least an empty array on failure
                _cache = _cache ?? Array.Empty<HnItem>();
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return _cache ?? Array.Empty<HnItem>();
    }

    private async Task RefreshInternal()
    {
        var client = _factory.CreateClient();
        client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");

        var ids = await client.GetFromJsonAsync<int[]>("v0/beststories.json");
        if (ids == null || ids.Length == 0)
        {
            // HN API returned no ids
            _cache = Array.Empty<HnItem>();
            return;
        }

        var tasks = ids.Select(async id =>
        {
            try
            {
                var item = await client.GetFromJsonAsync<HnItem>($"v0/item/{id}.json");
                return item;
            }
            catch (Exception ex)
            {
                // Failed to fetch item {Id} exception
                return null;
            }
        });

        var stories = await Task.WhenAll(tasks);
        var items = stories.OfType<HnItem>()
            .OrderByDescending(i => i.Score ?? 0)
            .ToArray();
        _cache = items;
        _lastRefresh = DateTimeOffset.UtcNow;
    }

    ~HnCacheService()
    {
    }
}
