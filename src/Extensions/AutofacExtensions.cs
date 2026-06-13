using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;

namespace SomeCatIDK.PirateJim.Extensions;

public static class AutofacExtensions
{
    public static IRegistrationBuilder<TLimit, TActivatorData, TStyle> InstancePerServiceLifetime<TLimit, TActivatorData, TStyle>
        (this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration, ServiceLifetime serviceLifetime)
    {
        return serviceLifetime switch
        {
            ServiceLifetime.Singleton => registration.SingleInstance(),
            ServiceLifetime.Scoped => registration.InstancePerLifetimeScope(),
            ServiceLifetime.Transient => registration.InstancePerDependency(),
            _ => registration
        };
    }
}