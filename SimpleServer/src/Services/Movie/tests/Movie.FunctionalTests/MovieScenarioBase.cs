using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Movie.FunctionalTests
{
    public class MovieScenarioBase
    {
        private class MovieApplication : WebApplicationFactory<Program>
        {
            public TestServer CreateServer()
            {
                return Server;
            }
            protected override IHost CreateHost(IHostBuilder builder)
            {
                builder.ConfigureServices(services => services.AddSingleton<IStartupFilter, AuthStartupFilter>());

                builder.ConfigureAppConfiguration(c =>
                {
                    var directory = Path.GetDirectoryName(typeof(MovieScenarioBase).Assembly.Location)!;

                    c.AddJsonFile(Path.Combine(directory, "appsettings.Movie.json"), optional: false);
                });

                return base.CreateHost(builder);
            }
        }


        public TestServer CreateServer()
        {
            var factory = new MovieApplication();
            return factory.Server;
        }

        public static class Get
        {
            public static string Movies() => "api/movie";
        }

        private class AuthStartupFilter : IStartupFilter
        {
            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                return app =>
                {
                    app.UseMiddleware<AutoAuthorizeMiddleware>();

                    next(app);
                };
            }
        }
    }
}