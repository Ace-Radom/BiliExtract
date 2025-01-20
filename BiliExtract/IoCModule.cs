using Autofac;
using BiliExtract.Lib.Extensions;
using BiliExtract.Managers;

namespace BiliExtract;

public class IoCModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<ThemeManager>();
        builder.Register<ThemeManagerV2>().AutoActivate();

        return;
    }
}
