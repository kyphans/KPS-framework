using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Platform.Core;

public interface IModule
{
    string Key { get; }
    void ConfigureServices(IServiceCollection services);
    void ConfigureEndpoints(IEndpointRouteBuilder endpoints);
}
