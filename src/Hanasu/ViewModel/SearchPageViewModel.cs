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
    public class SearchPageViewModel: Crystal.Dynamic.AutoIPNotifyingBaseViewModel
    {
        public override void OnNavigatedTo(dynamic argument = null)
        {
            //http://stackoverflow.com/questions/11812059/windows-8-metro-focus-on-grid
            CoreWindow.GetForCurrentThread().KeyDown += SearchPageViewModel_KeyDown;

            var args = (KeyValuePair<string, string>)argument[0];

            Stations = new ObservableCollection<Station>();

            SetupSearch();

            UpdateSearchQuery(args.Value);
        }

        void SearchPageViewModel_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            int keyCode = (int)args.VirtualKey;
            if (keyCode == 0
                || (keyCode > 0 && keyCode < 32)
                || (keyCode > 33 && keyCode < 47)
                || (keyCode > 91 && keyCode < 165)
                || keyCode == 91 || keyCode > 166)
                return;


            string initialchar = Enum.GetName(typeof(Windows.System.VirtualKey), args.VirtualKey);

            SetupSearch(initialchar);
        }

        private void SetupSearch(string initial = null)
        {
            var searchPanel = SearchPane.GetForCurrentView();

            searchPanel.QuerySubmitted += delegate(SearchPane s, SearchPaneQuerySubmittedEventArgs e)
            {
                UpdateSearchQuery(e.QueryText);
            };

            searchPanel.SuggestionsRequested += delegate(SearchPane s, SearchPaneSuggestionsRequestedEventArgs e)
            {
                //http://weblogs.asp.net/nmarun/archive/2012/09/28/implementing-search-contract-in-windows-8-application.aspx

                IEnumerable<string> names = from suggestion in ((App)App.Current).AvailableStations
                                            where suggestion.Title.StartsWith(e.QueryText,
                                                                       StringComparison.CurrentCultureIgnoreCase)
                                            select suggestion.Title;

                // Take(5) is implemented because the SearchPane 
                // can show a maximum of 5 suggestions
                // passing a larger collection will only show the first 5
                e.Request.SearchSuggestionCollection.AppendQuerySuggestions(names.Take(5));
            };

            if (initial != null)
                searchPanel.Show(initial);
        }

        [MessageHandler("UpdateSearchQuery")]
        public void UpdateSearchQuery(string query)
        {
            SearchQuery = query;

            TitleString = string.Format(
                LocalizationManager.GetLocalizedValue("SearchQueryTitleFormat"),
                query);

            RaisePropertyChanged(x => this.TitleString);

            IEnumerable<Station> names = from suggestion in ((App)App.Current).AvailableStations
                                         where suggestion.Title.StartsWith(query,
                                                                    StringComparison.CurrentCultureIgnoreCase)
                                         select suggestion;

            if (Stations == null)
                Stations = new ObservableCollection<Station>();
            else
                Stations.Clear();

            foreach (var stat in names)
                Stations.Add(stat);

            RaisePropertyChanged(x => this.Stations);
        }

        public string SearchQuery { get; set; }

        public string TitleString { get; set; }

        public ObservableCollection<Station> Stations { get; set; }
    }
}
