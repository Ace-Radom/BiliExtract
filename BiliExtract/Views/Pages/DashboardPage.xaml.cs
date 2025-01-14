using BiliExtract.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BiliExtract.Views.Pages
{
    public partial class DashboardPage : INavigableView<DashboardPageViewModel>
    {
        public DashboardPageViewModel ViewModel { get; }

        public DashboardPage(DashboardPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
