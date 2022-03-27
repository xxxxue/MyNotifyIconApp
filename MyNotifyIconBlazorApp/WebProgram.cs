using MyNotifyIconBlazorApp.Data;

public static class WebProgram
{
    /// <summary>
    /// 非常普通的一个 .NET 6 WebHost
    /// 只添加了一个 ApplicationStarted 启动浏览器的操作
    /// </summary>
    public static void RunWebHost(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        builder.Services.AddScoped<WeatherForecastService>();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");

#if DEBUG
        app.Run();
#else
        // 启动 web host 并 打开浏览器
        app.RunAndStartedOpenBrowser();
#endif
    }
}