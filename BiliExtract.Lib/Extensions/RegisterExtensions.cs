using Autofac;
using Autofac.Builder;
using BiliExtract.Lib.Listener;
using System;

namespace BiliExtract.Lib.Extensions;

public static class RegisterExtensions
{
    public static void AutoActivateListener<T>(this IRegistrationBuilder<IListener<T>, ConcreteReflectionActivatorData, SingleRegistrationStyle> registration) where T : EventArgs
    {
        registration.OnActivating(e => e.Instance.StartAsync().AsValueTask()).AutoActivate();
    }
}
