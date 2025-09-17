using CheevoFight.ViewPlusViewModel;
using System.Windows;
using System.Windows.Controls;

namespace CheevoFight
{
    public partial class Landing : Window
    {
        public static readonly DependencyProperty SteamWebAPIKeyProperty = DependencyProperty.Register(
            "SteamWebAPIKey", typeof(string), typeof(Landing)
        );
        public static readonly DependencyProperty SteamProfileURLProperty = DependencyProperty.Register(
            "SteamProfileURL", typeof(string), typeof(Landing)
        );


        public Landing()
        {
            this.InitializeComponent();
            var viewModel = new ViewModel();
            this.DataContext = viewModel;
        }


        private void SeeIfStartButtonCanBeEnabled(object sender, dynamic e)
        {
            this.ButtonStart.IsEnabled = !Validation.GetHasError(this.TextBoxSteamWebAPIKey) && !Validation.GetHasError(this.TextBoxSteamProfileURL);
        }


        private void AdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as ViewModel;

            var advancedSettings = new AdvancedSettings(viewModel);
            advancedSettings.ShowDialog();
        }


        private async void StartExit_Click(object sender, RoutedEventArgs e)
        {
            if (sender == this.ButtonStart)
            {
                var viewModel = this.DataContext as ViewModel;
                await Calculations.CompileData(viewModel);
            }

            this.Close();
        }
    }
}