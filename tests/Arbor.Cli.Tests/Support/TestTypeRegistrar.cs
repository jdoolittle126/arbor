using System;
using System.Collections.Generic;
using Spectre.Console.Cli;

namespace Arbor.Cli.Tests.Support;

internal sealed class TestTypeRegistrar : ITypeRegistrar, ITypeResolver, IDisposable
{
    private readonly Dictionary<Type, Func<object>> _registrations = new();

    public ITypeResolver Build() => this;

    public void Register(Type service, Type implementation)
    {
        _registrations[service] = () => Activator.CreateInstance(implementation)!;
    }

    public void RegisterInstance(Type service, object implementation)
    {
        _registrations[service] = () => implementation;
    }

    public void RegisterLazy(Type service, Func<object> factory)
    {
        _registrations[service] = factory;
    }

    public object? Resolve(Type? type)
    {
        if (type is null)
        {
            return null;
        }

        if (_registrations.TryGetValue(type, out var factory))
        {
            return factory();
        }

        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
        {
            var elementType = type.GenericTypeArguments[0];
            return Array.CreateInstance(elementType, 0);
        }

        return null;
    }

    public void Dispose()
    {
    }
}
