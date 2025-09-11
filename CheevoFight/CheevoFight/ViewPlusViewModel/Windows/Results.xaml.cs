using System.Windows;
using System.Windows.Controls;

namespace CheevoFight.ViewPlusViewModel.Windows
{
    public partial class Results : Window
    {
        public Results(ViewModel viewModel)
        {
            this.InitializeComponent();
            this.DataContext = viewModel;
        }


        private void ComboBoxFriendSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModel;
            var selectedItem = (sender as ComboBox).SelectedItem;
            if (selectedItem is not null)
            {
                var steamName = selectedItem.ToString().Split(',')[0].Replace("[", string.Empty);
                viewModel.TagResults = ViewModel.TagResultsBySteamName[steamName];
            }
        }
    }
}