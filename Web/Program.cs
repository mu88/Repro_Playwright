using Microsoft.Playwright;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<IScreenshotCreator, NewScreenshotCreator>();
builder.Services.AddHostedService<BackgroundScreenshotCreator>();

var app = builder.Build();
await app.RunAsync();

internal class BackgroundScreenshotCreator(IScreenshotCreator screenshotCreator) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer timer = new(TimeSpan.FromSeconds(15));
        while (await timer.WaitForNextTickAsync(stoppingToken)) await screenshotCreator.CreateScreenshotAsync(800, 600);
    }
}

public interface IScreenshotCreator
{
    Task CreateScreenshotAsync(uint width, uint height);
}

internal class NewScreenshotCreator(ILogger<NewScreenshotCreator> logger) : IScreenshotCreator
{
    public async Task CreateScreenshotAsync(uint width, uint height)
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        await page.SetViewportSizeAsync((int)width, (int)height);
        await page.GotoAsync("https://playwright.dev/dotnet/");
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = "Screenshot.png", Type = ScreenshotType.Png });
        logger.LogInformation("Screenshot created");
    }
}