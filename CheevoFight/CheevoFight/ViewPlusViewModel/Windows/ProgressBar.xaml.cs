using System.Windows;

namespace CheevoFight.ViewPlusViewModel.Windows
{
    public partial class ProgressBar : Window
    {
        public ProgressBar(ViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
        }
    }
}