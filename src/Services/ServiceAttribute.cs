using Microsoft.Extensions.DependencyInjection;
using System;

namespace SomeCatIDK.PirateJim.Services;

/// <summary>
/// Register a class as a Service.
/// Services are not created automatically, if you want a Service to be created after the Bot has loaded inherit <see cref="IInitializableService"/>
/// </summary>
/// <param name="serviceLifetime"></param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class ServiceAttribute(ServiceLifetime serviceLifetime = ServiceLifetime.Scoped) : Attribute
{
    public ServiceLifetime Lifetime { get; } = serviceLifetime;
}