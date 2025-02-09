using BiliExtract.Lib;
using BiliExtract.Lib.Managers;
using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using BiliExtract.Resources;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BiliExtract.Views.Pages;

public partial class TempPage
{
    private readonly ApplicationSettings _settings = IoCContainer.Resolve<ApplicationSettings>();
    private readonly TempManager _tempManager = IoCContainer.Resolve<TempManager>();

    private bool _isRefreshing;

    public TempPage()
    {
        InitializeComponent();

        IsVisibleChanged += TempPage_IsVisibleChangedAsync;
        _tempManager.DataChanged += TempManager_DataChangedAsync;

        return;
    }

    private async void TempManager_DataChangedAsync(object? sender, EventArgs e)
    {
        if (!_isRefreshing && IsVisible)
        {
            await Dispatcher.InvokeAsync(async () => await RefreshAsync());
        }
        return;
    }

    private async void TempPage_IsVisibleChangedAsync(object sender, DependencyPropertyChangedEventArgs e)
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

        _totalTempStorageUsageTextBlock.Text = string.Format(
            Resource.TempPage_TotalTempStorageUsageTextBlock_Text,
            FileSize.ConvertDataSizeByteToString(_tempManager.StorageUsageByte, _settings.Data.DataSizePrefix)
        );
        _totalTempStorageUsageTextBlock.Visibility = Visibility.Visible;
        _tempInUseTextBlock.Text = string.Format(
            Resource.TempPage_TempInUseTextBlock_Text,
            FileSize.ConvertDataSizeByteToString(_tempManager.StorageInUseUsageByte, _settings.Data.DataSizePrefix)
        );
        _tempInUseTextBlock.Visibility = Visibility.Visible;
        _tempReleasedTextBlock.Text = string.Format(
            Resource.TempPage_TempReleasedTextBlock_Text,
            FileSize.ConvertDataSizeByteToString(_tempManager.StorageReleasedUsageByte, _settings.Data.DataSizePrefix)
        );
        _tempReleasedTextBlock.Visibility = Visibility.Visible;

        _normalTempUsageTextBlock.Text = string.Format(
            Resource.TempPage_NormalLockedReleasedTempUsageTextBlock_Text,
            _tempManager.NormalTempFileHandleCount,
            FileSize.ConvertDataSizeByteToString(_tempManager.StorageNormalUsageByte, _settings.Data.DataSizePrefix)
        );
        _normalTempUsageTextBlock.Visibility = Visibility.Visible;
        _lockedTempUsageTextBlock.Text = string.Format(
            Resource.TempPage_NormalLockedReleasedTempUsageTextBlock_Text,
            _tempManager.LockedTempFileHandleCount,
            FileSize.ConvertDataSizeByteToString(_tempManager.StorageLockedUsageByte, _settings.Data.DataSizePrefix)
        );
        _lockedTempUsageTextBlock.Visibility = Visibility.Visible;
        _releasedTempUsageTextBlock.Text = string.Format(
            Resource.TempPage_NormalLockedReleasedTempUsageTextBlock_Text,
            _tempManager.ReleasedTempFileHandleCount,
            FileSize.ConvertDataSizeByteToString(_tempManager.StorageReleasedUsageByte, _settings.Data.DataSizePrefix)
        );
        _releasedTempUsageTextBlock.Visibility = Visibility.Visible;

        _lastCleanupTimeTextBlock.Text = _tempManager.LastCleanupDateTime.ToString();
        _lastCleanupTimeTextBlock.Visibility = Visibility.Visible;
        _nextCleanupTimeTextBlock.Text = _tempManager.NextCleanupDateTime.ToString();
        _nextCleanupTimeTextBlock.Visibility = Visibility.Visible;
        _lastStorageUsageRefreshTimeTextBlock.Text = _tempManager.LastStorageUsageRefreshDateTime.ToString();
        _lastStorageUsageRefreshTimeTextBlock.Visibility = Visibility.Visible;
        _nextStorageUsageRefreshTimeTextBlock.Text = _tempManager.NextStorageUsageRefreshDateTime.ToString();
        _nextStorageUsageRefreshTimeTextBlock.Visibility = Visibility.Visible;

        _isRefreshing = false;
        return Task.CompletedTask;
    }
}
