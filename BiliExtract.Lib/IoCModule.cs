using Autofac;
using BiliExtract.Lib.Adb;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Listener;
using BiliExtract.Lib.Managers;
using BiliExtract.Lib.Settings;

namespace BiliExtract.Lib;

public class IoCModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.Register<AdbServer>();

        builder.Register<SystemThemeListener>().AutoActivateListener();
        builder.Register<TempFolderListener>().AutoActivateListener();

        builder.Register<TempManager>();

        builder.Register<AdbSettings>();
        builder.Register<ApplicationSettings>();
        builder.Register<LockedTempHandlesSettings>();
        builder.Register<TextStyleSettings>();

        return;
    }
}
