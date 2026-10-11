
using Microsoft.EntityFrameworkCore;

namespace RaceDay_api_proj.Data
{
    public class ApplicationDBContext : DbContext
    {
        public ApplicationDBContext(
            DbContextOptions<ApplicationDBContext> options)
            : base(options)
        {
        }
    }
}
