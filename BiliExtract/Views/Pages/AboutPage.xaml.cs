using BiliExtract.Lib.Utils;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace BiliExtract.Views.Pages;

public partial class AboutPage
{
    public AboutPage()
    {
        InitializeComponent();

        _applicationNameTextBlock.Text += $" {ApplicationInfo.VersionText}";
        _buildTextBlock.Text += $" {ApplicationInfo.BuildText}";
        _copyrightTextBlock.Text = ApplicationInfo.CopyrightText;

        return;
    }

    private void ApplicationDataFolderHyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(Folders.AppData))
        {
            return;
        }
        Process.Start("explorer", Folders.AppData);
        return;
    }

    private void ApplicationTempFolderHyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(Folders.Temp))
        {
            return;
        }
        Process.Start("explorer", Folders.Temp);
        return;
    }
}
