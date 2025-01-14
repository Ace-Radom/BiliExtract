using Autofac;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Settings;

namespace BiliExtract.Lib;

public class IoCModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<ApplicationSettings>();

        return;
    }
}
