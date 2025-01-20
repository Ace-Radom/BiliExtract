using Autofac;
using System;

namespace BiliExtract.Lib;

public static class IoCContainer
{
    private static readonly object Lock = new();

    private static IContainer? _container;

    public static bool IsInitialized { get; private set; }

    public static void Initialize(params Module[] modules)
    {
        lock (Lock)
        {
            if (_container is not null)
            {
                throw new InvalidOperationException("IoCContainer already initialized");
            }

            var builder = new ContainerBuilder();
            foreach (var module in modules)
            {
                builder.RegisterModule(module);
            }

            _container = builder.Build();
            IsInitialized = true;
        }
        return;
    }

    public static T Resolve<T>() where T : notnull
    {
        lock (Lock)
        {
            if (_container is null)
            {
                throw new InvalidOperationException($"IoCContainer not initialized [type={nameof(T)}]");
            }
            return _container.Resolve<T>();
        }
    }

    public static T? TryResolve<T>() where T : class
    {
        lock (Lock)
        {
            if (_container is null)
            {
                throw new InvalidOperationException($"IoCContainer not initialized [type={nameof(T)}]");
            }
            _ = _container.TryResolve(out T? value);
            return value;
        }
    }
}
