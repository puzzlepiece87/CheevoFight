namespace CheevoFight.ViewPlusViewModel.Windows;

using CheevoFight.Tools;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


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
            this.UpdateViewWithSearchResults();
        }
    }


    private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        this.UpdateViewWithSearchResults();
    }


    public void UpdateViewWithSearchResults()
    {
        var viewModel = this.DataContext as ViewModel;
        var searchTerm = this.TextBoxSearch.Text;

        if (string.IsNullOrEmpty(searchTerm))
        {
            viewModel.SearchResults = new ObservableCollection<ResultsManager>(viewModel.TagResults);
        }
        else
        {
            viewModel.SearchResults.Clear();
            foreach (var tagRow in viewModel.TagResults)
            {
                if (tagRow.TagTextBlockContent.Contains(searchTerm))
                {
                    viewModel.SearchResults.Add(tagRow);
                }
            }

            foreach (var keyValuePair in Calculations.TagsByGameName)
            {
                var gameName = keyValuePair.Key;
                if (gameName.ContainsCaseInsensitive(searchTerm))
                {
                    var tagsForThisGame = keyValuePair.Value;
                    
                    foreach (var tagRow in viewModel.TagResults)
                    {
                        var tag = tagRow.TagTextBlockContent.Left(tagRow.TagTextBlockContent.LastIndexOf(':'));
                        if (tagsForThisGame.Contains(tag))
                        {
                            viewModel.SearchResults.Add(tagRow);
                        }
                    }
                }
            }
        }
    }
}