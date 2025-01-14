using BiliExtract.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace BiliExtract.Views.Pages
{
    public partial class DataPage : INavigableView<DataPageViewModel>
    {
        public DataPageViewModel ViewModel { get; }

        public DataPage(DataPageViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this;

            InitializeComponent();
        }
    }
}
