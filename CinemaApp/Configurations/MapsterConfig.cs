using Mapster;

namespace CinemaApp.Configurations
{
    public static class MapsterConfig
    {
        public static void RegisterMapsterConfig(this IServiceCollection services)
        {
            TypeAdapterConfig<ApplicationUser, ApplicationUserVM>
                .NewConfig()
                .Map(d => d.FullName, s => $"{s.FirstName} {s.LastName}").TwoWays();

        }
    }
}
