using BiliExtract.Lib;
using BiliExtract.Lib.Managers;
using BiliExtract.Managers;
using BiliExtract.Views.Windows;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace BiliExtract
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            IoCContainer.Initialize(
                new Lib.IoCModule(),
                new IoCModule()
            );

            await IoCContainer.Resolve<TempManager>().StartBackgroundTimerAsync();

            var mainWindow = new MainWindow
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            MainWindow = mainWindow;

            IoCContainer.Resolve<ThemeManager>().Apply();

            mainWindow.Show();

            return;
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
