using BiliExtract.Controls.Extensions;
using BiliExtract.ViewModels.Pages;
using BiliExtract.Lib;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using System.Threading.Tasks;

namespace BiliExtract.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsViewModel>
    {
        private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();

        private bool _isRefreshing;

        public SettingsViewModel ViewModel { get; }

        public SettingsPage(SettingsViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            IsVisibleChanged += SettingsPage_IsVisibleChanged;

            return;
        }

        private async void SettingsPage_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                await RefreshAsync();
            }
            return;
        }

        private Task RefreshAsync()
        {
            _isRefreshing = true;

            _themeComboBox.SetItems(Enum.GetValues<Theme>(), _settings.Data.Theme, t => t.GetDisplayName());
            _themeComboBox.Visibility = Visibility.Visible;

            _isRefreshing = false;
            return Task.CompletedTask;
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            if (!_themeComboBox.TryGetSelectedItem(out Theme state))
            {
                return;
            }

            _settings.Data.Theme = state;
            _settings.SynchronizeData();

            return;
        }
    }
}
