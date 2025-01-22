using Autofac;
using BiliExtract.Lib.Extensions;
using BiliExtract.Managers;

namespace BiliExtract;

public class IoCModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<RichLogViewStyleManager>();
        builder.Register<ThemeManager>().AutoActivate();

        return;
    }
}
