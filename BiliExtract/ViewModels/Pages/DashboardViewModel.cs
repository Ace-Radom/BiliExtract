using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BiliExtract.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty]
        private int _counter = 0;

        [RelayCommand]
        private void OnCounterIncrement()
        {
            Counter++;
        }
    }
}
