using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace CheevoFight
{
    public class ResultsManager : INotifyPropertyChanged
    {
        private int progressBarValue;
        public int ProgressBarValue
        {
            get => this.progressBarValue;
            set
            {
                this.progressBarValue = value;
                this.NotifyPropertyChanged(nameof(this.ProgressBarValue));
                this.NotifyPropertyChanged(nameof(this.LabelContentTag));
            }
        }
        private int progressBarMaximum;
        public int ProgressBarMaximum
        {
            get => this.progressBarMaximum;
            set
            {
                this.progressBarMaximum = value;
                this.NotifyPropertyChanged(nameof(this.ProgressBarMaximum));
                this.NotifyPropertyChanged(nameof(this.LabelContentTag));
            }
        }
        private string? labelContentTag;
        public string? LabelContentTag
        {
            get => this.labelContentTag;
            set
            {
                this.labelContentTag = value;
                this.NotifyPropertyChanged(nameof(this.TagLabelContent));
            }
        }
        public string TagLabelContent => this.LabelContentTag + ": " + this.ProgressBarValue.ToString() + "%";
        public ObservableCollection<Image> CapsuleImages { get; set; }
        public static Dictionary<string, ObservableCollection<ResultsManager>> TagResultsByFriendSteamId { get; set; }


        public event PropertyChangedEventHandler? PropertyChanged;


        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        static ResultsManager()
        {
            ResultsManager.TagResultsByFriendSteamId = new Dictionary<string, ObservableCollection<ResultsManager>>();
        }


        public ResultsManager(
            string friendSteamId,
            string tag,
            int percentage,
            HashSet<string> pathsCapsuleImages,
            Dictionary<string, string> gameNameByCapsuleImagePath            
        )
        {
            if (ResultsManager.TagResultsByFriendSteamId.TryGetValue(friendSteamId, out var tagResults))
            {
                this.LabelContentTag = tag;
                this.ProgressBarValue = percentage;
                this.progressBarMaximum = 100;
                this.CapsuleImages = new ObservableCollection<Image>();

                foreach (var pathCapsuleImage in pathsCapsuleImages)
                {
                    var imageCapsule = new Image();
                    var bitmapImageCapsule = new BitmapImage();
                    bitmapImageCapsule.BeginInit();
                    bitmapImageCapsule.UriSource = new Uri(pathCapsuleImage);
                    bitmapImageCapsule.EndInit();
                    imageCapsule.Source = bitmapImageCapsule;
                    imageCapsule.ToolTip = gameNameByCapsuleImagePath[pathCapsuleImage];
                    ToolTipService.SetInitialShowDelay(imageCapsule, 0);
                    ToolTipService.SetBetweenShowDelay(imageCapsule, 0);
                    this.CapsuleImages.Add(imageCapsule);
                }

                tagResults.Add(this);
            }
            else
            {
                this.LabelContentTag = tag;
                this.ProgressBarValue = percentage;
                this.progressBarMaximum = 100;
                this.CapsuleImages = new ObservableCollection<Image>();

                foreach (var pathCapsuleImage in pathsCapsuleImages)
                {
                    var imageCapsule = new Image();
                    var bitmapImageCapsule = new BitmapImage();
                    bitmapImageCapsule.BeginInit();
                    bitmapImageCapsule.UriSource = new Uri(pathCapsuleImage);
                    bitmapImageCapsule.EndInit();
                    imageCapsule.Source = bitmapImageCapsule;
                    imageCapsule.ToolTip = gameNameByCapsuleImagePath[pathCapsuleImage];
                    ToolTipService.SetInitialShowDelay(imageCapsule, 0);
                    ToolTipService.SetBetweenShowDelay(imageCapsule, 0);
                    this.CapsuleImages.Add(imageCapsule);
                }

                ResultsManager.TagResultsByFriendSteamId.Add(friendSteamId, new ObservableCollection<ResultsManager> { this });
            }
        }
    }
}
