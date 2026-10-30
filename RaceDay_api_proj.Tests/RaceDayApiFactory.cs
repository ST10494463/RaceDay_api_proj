
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay_api_proj.Data;

namespace RaceDay_api_proj.Tests
{
    public class RaceDayApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<DbContextOptions<ApplicationDBContext>>();
                services.RemoveAll<ApplicationDBContext>();

                services.AddDbContext<ApplicationDBContext>(options =>
                {
                    options.UseInMemoryDatabase(
                        Guid.NewGuid().ToString());
                });
            });
        }
    }
}
