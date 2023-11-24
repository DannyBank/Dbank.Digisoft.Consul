using Dbank.Digisoft.Config.Abstractions;
using Dbank.Digisoft.Config.Serivces;

namespace Dbank.Digisoft.Engine.Config
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddBusinessDI(this IServiceCollection services)
        {
            services.AddTransient<IFileHelper, FileHelper>();
            services.AddTransient<IKeyValueHelper, KeyValueHelper>();
            return services;
        }
    }
}