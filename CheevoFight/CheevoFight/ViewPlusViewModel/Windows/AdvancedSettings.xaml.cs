namespace CheevoFight;

using CheevoFight.ViewPlusViewModel;
using System.Windows;


public partial class AdvancedSettings : Window
{
    public AdvancedSettings(ViewModel viewModel)
    {
        this.InitializeComponent();
        this.DataContext = viewModel;
    }


    private async void Save_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.CacheCapsulesAndTags = (bool)this.CheckBoxCacheImagesAndTags.IsChecked;

        this.Close();
    }
}