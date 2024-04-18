using BeaconWPF.Data;
using BeaconWPF.Data.Bibles;
using BeaconWPF.Data.Songs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;

namespace BeaconWPF
{
    public static class Startup
    {
        public static IServiceProvider? Services { get; private set; }

        public static void Init()
        {
            var host = Host.CreateDefaultBuilder()
                           .ConfigureServices(WireupServices)
                           .Build();
            Services = host.Services;
        }

        private static void WireupServices(IServiceCollection services)
        {
            services.AddWpfBlazorWebView();
            services.AddSingleton<ISongService, SongService>();

            services.AddSingleton<IBibleService, BibleService>();

            services.AddSingleton<ICustomHttpFactory, HttpFactory>();
#if DEBUG
            services.AddBlazorWebViewDeveloperTools();
#endif
        }
    }
}
