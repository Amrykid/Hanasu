using Crystal.Localization;
using Crystal.Messaging;
using Hanasu.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Search;
using Windows.UI.Core;

namespace Hanasu.ViewModel
{
    /// <summary>
    /// The view model for the search page
    /// </summary>
    public class SearchPageViewModel : Crystal.Dynamic.AutoIPNotifyingBaseViewModel
    {
        public override void OnNavigatedTo(dynamic argument = null)
        {
            //http://stackoverflow.com/questions/11812059/windows-8-metro-focus-on-grid
            coreWindow = CoreWindow.GetForCurrentThread();
            coreWindow.KeyDown += SearchPageViewModel_KeyDown; //handle global down

            var args = (KeyValuePair<string, string>)argument[0];

            Stations = new ObservableCollection<Station>();

            SetupSearch(args.Value);

            UpdateSearchQuery(args.Value);
        }

        CoreWindow coreWindow = null;

        public override void OnNavigatedFrom()
        {
            if (coreWindow != null)
                coreWindow.KeyDown -= SearchPageViewModel_KeyDown;

            searchPanel.QuerySubmitted -= searchPanel_QuerySubmitted;

            searchPanel.SuggestionsRequested -= searchPanel_SuggestionsRequested;
        }

        void SearchPageViewModel_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            int keyCode = (int)args.VirtualKey;
            if (keyCode == 0
                || (keyCode > 0 && keyCode < 32)
                || (keyCode > 33 && keyCode < 47)
                || (keyCode > 91 && keyCode < 165)
                || keyCode == 91 || keyCode > 166)
                return; //if the entered key is not a letter/number, ignore it.


            string initialchar = Enum.GetName(typeof(Windows.System.VirtualKey), args.VirtualKey);

            SetupSearch(initialchar); //Start the search procedure
        }
        private SearchPane searchPanel = null;
        private void SetupSearch(string initial = null)
        {
            if (searchPanel == null)
            {
                searchPanel = SearchPane.GetForCurrentView(); //grab the search pane
                searchPanel.PlaceholderText = LocalizationManager.GetLocalizedValue("SearchPanePlaceholder"); //grab the localized place holder text
                if (initial != null)
                    searchPanel.Show(initial); //show the searchpanel if it doesn't have a value already.
            }

            searchPanel.QuerySubmitted += searchPanel_QuerySubmitted;

            searchPanel.SuggestionsRequested += searchPanel_SuggestionsRequested;

            //if (initial != null)
            //    searchPanel.Show(initial);
        }

        void searchPanel_SuggestionsRequested(SearchPane s, SearchPaneSuggestionsRequestedEventArgs e)
        {
            //provide search suggestions to the search pane

            //http://weblogs.asp.net/nmarun/archive/2012/09/28/implementing-search-contract-in-windows-8-application.aspx

            IEnumerable<string> names = from suggestion in ((App)App.Current).AvailableStations
                                        where suggestion.Title.StartsWith(e.QueryText,
                                                                   StringComparison.CurrentCultureIgnoreCase)
                                        select suggestion.Title;

            // Take(5) is implemented because the SearchPane 
            // can show a maximum of 5 suggestions
            // passing a larger collection will only show the first 5
            e.Request.SearchSuggestionCollection.AppendQuerySuggestions(names.Take(5));
        }

        void searchPanel_QuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            UpdateSearchQuery(args.QueryText); //update the search when the user presses enter in the search pane
        }

        [MessageHandler("UpdateSearchQuery")]
        public void UpdateSearchQuery(string query)
        {
            //updates the search page.

            SearchQuery = query; //grab the query

            TitleString = string.Format(
                LocalizationManager.GetLocalizedValue("SearchQueryTitleFormat"),
                query); //set the page title to a localized string

            RaisePropertyChanged(x => this.TitleString);

            IEnumerable<Station> names = from suggestion in ((App)App.Current).AvailableStations
                                         where suggestion.Title.StartsWith(query,
                                                                    StringComparison.CurrentCultureIgnoreCase)
                                         select suggestion; //fetch all matching stations

            if (Stations == null)
                Stations = new ObservableCollection<Station>(); //create a new station collection if the stations is null
            else
                Stations.Clear();

            foreach (var stat in names)
            {
                stat.StationDisplay = StationDisplayType.Small;
                Stations.Add(stat);
            }

            RaisePropertyChanged(x => this.Stations);
        }

        public string SearchQuery { get; set; }

        public string TitleString { get; set; }

        public ObservableCollection<Station> Stations { get; set; }
    }
}
