using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;

namespace CheevoFight.ViewPlusViewModel
{
    public class ViewModel : INotifyPropertyChanged
    {
        public static string SteamWebAPIKey { get; set; }
        public static string SteamProfileURL { get; set; }
        public static bool CacheCapsulesAndTags { get; set; }
        public static ObservableCollection<ProgressBarManager> ProgressBars { get; set; }
        private ObservableCollection<ResultsManager> tagResults;
        public ObservableCollection<ResultsManager> TagResults
        {
            get => this.tagResults;
            set
            {
                this.tagResults = value;
                this.NotifyPropertyChanged(nameof(this.TagResults));
            }
        }
        private ObservableCollection<ResultsManager> searchResults;
        public ObservableCollection<ResultsManager> SearchResults
        {
            get => this.searchResults;
            set
            {
                this.searchResults = value;
                this.NotifyPropertyChanged(nameof(this.SearchResults));
            }
        }
        public static ObservableCollection<KeyValuePair<string, Image>> AvatarsBySteamName { get; set; }
        public static Dictionary<string, ObservableCollection<ResultsManager>> TagResultsBySteamName { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        static ViewModel()
        {
            ViewModel.SteamWebAPIKey = string.Empty;
            ViewModel.SteamProfileURL = string.Empty;
            ViewModel.CacheCapsulesAndTags = false;
            ViewModel.ProgressBars = new ObservableCollection<ProgressBarManager>();
            ViewModel.TagResultsBySteamName = new Dictionary<string, ObservableCollection<ResultsManager>>();
            ViewModel.AvatarsBySteamName = new ObservableCollection<KeyValuePair<string, Image>>();
        }
    }
}
