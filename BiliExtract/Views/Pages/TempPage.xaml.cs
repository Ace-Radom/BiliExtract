using BiliExtract.Lib;
using BiliExtract.Lib.Managers;
using BiliExtract.Lib.Settings;
using BiliExtract.Lib.Utils;
using BiliExtract.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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

        _isRefreshing = false;
        return Task.CompletedTask;
    }
}
