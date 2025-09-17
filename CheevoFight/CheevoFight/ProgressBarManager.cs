namespace CheevoFight;

using CheevoFight.ViewPlusViewModel;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;


public class ProgressBarManager : INotifyPropertyChanged
{
    private int progressBarValue;
    public int ProgressBarValue
    {
        get => this.progressBarValue;
        set
        {
            this.progressBarValue = value;
            this.NotifyPropertyChanged(nameof(this.ProgressBarValue));
            this.NotifyPropertyChanged(nameof(this.TextBlockContent));
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
            this.NotifyPropertyChanged(nameof(this.TextBlockContent));
        }
    }
    private string? textBlockContentTask;
    public string? TextBlockContentTask
    {
        get => this.textBlockContentTask;
        set
        {
            this.textBlockContentTask = value;
            this.NotifyPropertyChanged(nameof(this.TextBlockContent));
        }
    }
    public string TextBlockContent => this.TextBlockContentTask + ": " + this.ProgressBarValue.ToString() + " / " + this.ProgressBarMaximum.ToString();
    public static ConcurrentDictionary<string, ProgressBarManager> ProgressBarsByName { get; set; }
    private object ProgressBarLock { get; set; }


    public event PropertyChangedEventHandler? PropertyChanged;


    private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }


    /// <summary>
    /// Manages the values, maximums, and textblocks of stacked displayed progress bars
    /// </summary>
    static ProgressBarManager()
    {
        ProgressBarManager.ProgressBarsByName = new ConcurrentDictionary<string, ProgressBarManager>();
    }


    /// <summary>
    /// Manages the values, maximums, and textblocks of stacked displayed progress bars
    /// </summary>
    /// <param name="name"></param>
    /// <exception cref="ArgumentException">Thrown if there is already a progress bar by that name</exception>
    public ProgressBarManager(string name)
    {
        if (!ProgressBarManager.ProgressBarsByName.TryAdd(name, this))
        {
            throw new ArgumentException("There is already a progress bar by name " + name);
        }

        ViewModel.ProgressBars.Add(this);
        this.ProgressBarLock = new object();
    }


    /// <summary>
    /// Increases the value of the progress bar named in the argument
    /// </summary>
    /// <param name="progressBarName"></param>
    /// <param name="amountToIncrement">Optional argument to change the progress bar value by values other than 1</param>
    /// <exception cref="ArgumentException">Thrown if no progress bar is found by that name</exception>
    public static void IncrementValue(string progressBarName, int amountToIncrement = 1)
    {
        if (!ProgressBarsByName.TryGetValue(progressBarName, out var progressBar))
        {
            throw new ArgumentException("No progress bar exists by name " + progressBarName);

        }

        lock (progressBar.ProgressBarLock)
        {
            progressBar.ProgressBarValue += amountToIncrement;
        }
    }


    /// <summary>
    /// Increases the maximum of the progress bar named in the argument
    /// </summary>
    /// <param name="progressBarName"></param>
    /// <param name="amountToIncrement"></param>
    /// <exception cref="ArgumentException">Thrown if no progress bar is found by that name</exception>
    public static void IncrementMaximum(string progressBarName, int amountToIncrement)
    {
        if (!ProgressBarsByName.TryGetValue(progressBarName, out var progressBar))
        {
            throw new ArgumentException("No progress bar exists by name " + progressBarName);

        }

        lock (progressBar.ProgressBarLock)
        {
            progressBar.ProgressBarMaximum += amountToIncrement;
        }
    }


    /// <summary>
    /// Sets the task displayed in TextBlock text of the progress bar named in the argument
    /// </summary>
    /// <param name="progressBarName"></param>
    /// <param name="taskTextToDisplay"></param>
    /// <exception cref="ArgumentException">Thrown if no progress bar is found by that name</exception>
    public static void SetTextBlockContentTask(string progressBarName, string taskTextToDisplay)
    {
        if (!ProgressBarsByName.TryGetValue(progressBarName, out var progressBar))
        {
            throw new ArgumentException("No progress bar exists by name " + progressBarName);

        }

        progressBar.TextBlockContentTask = taskTextToDisplay;
    }
}
