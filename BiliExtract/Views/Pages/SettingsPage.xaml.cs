using BiliExtract.Controls.Extensions;
using BiliExtract.Extensions;
using BiliExtract.Lib;
using BiliExtract.Lib.Extensions;
using BiliExtract.Lib.Settings;
using BiliExtract.Managers;
using BiliExtract.ViewModels.Pages;
using BiliExtract.Views.Windows.Settings;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using System.Threading.Tasks;

namespace BiliExtract.Views.Pages
{
    public partial class SettingsPage : INavigableView<SettingsPageViewModel>
    {
        private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
        private readonly ThemeManagerV2 _themeManager = IoCContainer.Resolve<ThemeManagerV2>();

        private bool _isRefreshing;

        public SettingsPageViewModel ViewModel { get; }

        public SettingsPage(SettingsPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();

            IsVisibleChanged += SettingsPage_IsVisibleChangedAsync;
            _themeManager.ThemeApplied += ThemeManager_ThemeApplied;

            return;
        }

        private async void SettingsPage_IsVisibleChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisible)
            {
                await RefreshAsync();
            }
            return;
        }

        private void ThemeManager_ThemeApplied(object? sender, EventArgs e)
        {
            if (!_isRefreshing)
            {
                UpdateAccentColorPicker();
            }
            return;
        }

        private Task RefreshAsync()
        {
            _isRefreshing = true;

            _themeComboBox.SetItems(Enum.GetValues<Theme>(), _settings.Data.Theme, t => t.GetDisplayName());
            _themeComboBox.Visibility = Visibility.Visible;

            _accentColorComboBox.SetItems(Enum.GetValues<AccentColorSource>(), _settings.Data.AccentColorSource, t => t.GetDisplayName());
            _accentColorComboBox.Visibility = Visibility.Visible;
            UpdateAccentColorPicker();

            _useBuiltInAdbToggleSwitch.IsChecked = _settings.Data.UseBuiltInAdb;
            _useBuiltInAdbToggleSwitch.Visibility = Visibility.Visible;
            if (_settings.Data.UseBuiltInAdb)
            {
                _externalAdbPathCardControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                _externalAdbPathCardControl.Visibility = Visibility.Visible;
                _externalAdbPathTextBox.Text = _settings.Data.ExternalAdbPath;
                _externalAdbPathTextBox.Visibility = Visibility.Visible;
            }

            _useBuiltInFFmpegToggleSwitch.IsChecked = _settings.Data.UseBuiltInFFmpeg;
            _useBuiltInFFmpegToggleSwitch.Visibility = Visibility.Visible;
            if (_settings.Data.UseBuiltInFFmpeg)
            {
                _useBuiltInFFmpegCardControl.Margin = new(0, 0, 0, 24);
                _externalFFmpegPathCardControl.Visibility = Visibility.Collapsed;
            }
            else
            {
                _useBuiltInFFmpegCardControl.Margin = new(0, 0, 0, 8);
                _externalFFmpegPathCardControl.Margin = new(0, 0, 0, 24);
                _externalFFmpegPathCardControl.Visibility = Visibility.Visible;
                _externalFFmpegPathTextBox.Text = _settings.Data.ExternalFFmpegPath;
                _externalFFmpegPathTextBox.Visibility = Visibility.Visible;
            }

            _isRefreshing = false;
            return Task.CompletedTask;
        }

        private void UpdateAccentColorPicker()
        {
            _accentColorPicker.Visibility = _settings.Data.AccentColorSource == AccentColorSource.Custom ? Visibility.Visible : Visibility.Collapsed;
            _accentColorPicker.SelectedColor = _themeManager.GetAccentColor().ToColor();
            return;
        }

        private void AccentColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            if (!_accentColorComboBox.TryGetSelectedItem(out AccentColorSource state))
            {
                return;
            }

            _settings.Data.AccentColorSource = state;
            _settings.SynchronizeData();

            UpdateAccentColorPicker();
            _themeManager.Apply();

            return;
        }

        private void AccentColorPicker_ColorChangedDelayed(object sender, EventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            if (_settings.Data.AccentColorSource != AccentColorSource.Custom)
            {
                return;
            }

            _settings.Data.AccentColor = _accentColorPicker.SelectedColor.ToRGBColor();
            _settings.SynchronizeData();

            _themeManager.Apply();

            return;
        }

        private void AdvancedAdbSettingsCardAction_Click(object sender, RoutedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            var window = new AdbSettingsWindow { Owner = Window.GetWindow(this) };
            window.Show();

            return;
        }

        private void ExternalAdbPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            var text = _externalAdbPathTextBox.Text;
            if (text is null)
            {
                return;
            }

            _settings.Data.ExternalAdbPath = text;
            _settings.SynchronizeData();

            return;
        }

        private void ExternalFFmpegPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            var text = _externalFFmpegPathTextBox.Text;
            if (text is null)
            {
                return;
            }

            _settings.Data.ExternalFFmpegPath = text;
            _settings.SynchronizeData();

            return;
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
            _themeManager.Apply();

            return;
        }

        private void UseBuiltInAdbToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            var state = _useBuiltInAdbToggleSwitch.IsChecked;
            if (state is null)
            {
                return;
            }

            _settings.Data.UseBuiltInAdb = state.Value;
            _settings.SynchronizeData();
            _ = RefreshAsync();

            return;
        }

        private void UseBuiltInFFmpegToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            if (_isRefreshing)
            {
                return;
            }

            var state = _useBuiltInFFmpegToggleSwitch.IsChecked;
            if (state is null)
            {
                return;
            }

            _settings.Data.UseBuiltInFFmpeg = state.Value;
            _settings.SynchronizeData();
            _ = RefreshAsync();

            return;
        }
    }
}
