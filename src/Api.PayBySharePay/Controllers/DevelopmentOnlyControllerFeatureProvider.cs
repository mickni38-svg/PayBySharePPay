using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Hosting;

namespace Api.PayBySharePay.Controllers;

public sealed class DevelopmentOnlyControllerFeatureProvider
    : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly bool _isDevelopment;

    public DevelopmentOnlyControllerFeatureProvider(string? environmentName)
    {
        _isDevelopment = string.Equals(
            environmentName,
            Environments.Development,
            StringComparison.OrdinalIgnoreCase);
    }

    public void PopulateFeature(
        IEnumerable<ApplicationPart> parts,
        ControllerFeature feature)
    {
        if (_isDevelopment)
            return;

        var devController = feature.Controllers
            .FirstOrDefault(controller => controller.AsType() == typeof(DevController));

        if (devController is not null)
            feature.Controllers.Remove(devController);
    }
}
