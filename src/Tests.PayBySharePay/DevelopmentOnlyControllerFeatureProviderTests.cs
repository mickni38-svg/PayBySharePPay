using System.Reflection;
using Api.PayBySharePay.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace Tests.PayBySharePay;

public class DevelopmentOnlyControllerFeatureProviderTests
{
    [Fact]
    public void Development_RegistersDevController()
    {
        var feature = DiscoverControllers("Development");

        feature.Controllers
            .Select(controller => controller.AsType())
            .Should()
            .Contain(typeof(DevController));
    }

    [Theory]
    [InlineData("Simply")]
    [InlineData("Production")]
    [InlineData("Local")]
    [InlineData("Test")]
    [InlineData("")]
    [InlineData(null)]
    public void NonDevelopment_DoesNotRegisterDevController(string? environmentName)
    {
        var feature = DiscoverControllers(environmentName);

        feature.Controllers
            .Select(controller => controller.AsType())
            .Should()
            .NotContain(typeof(DevController));
    }

    [Theory]
    [InlineData("Development")]
    [InlineData("Simply")]
    [InlineData("Production")]
    public void EveryEnvironment_KeepsOrdinaryControllers(string environmentName)
    {
        var feature = DiscoverControllers(environmentName);

        feature.Controllers
            .Select(controller => controller.AsType())
            .Should()
            .Contain(typeof(OrdersController));
    }

    private static ControllerFeature DiscoverControllers(string? environmentName)
    {
        var manager = new ApplicationPartManager();
        manager.ApplicationParts.Add(
            new AssemblyPart(typeof(DevController).GetTypeInfo().Assembly));
        manager.FeatureProviders.Add(new ControllerFeatureProvider());
        manager.FeatureProviders.Add(
            new DevelopmentOnlyControllerFeatureProvider(environmentName));

        var feature = new ControllerFeature();
        manager.PopulateFeature(feature);
        return feature;
    }
}
