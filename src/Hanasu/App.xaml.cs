using Crystal.Core;
using Crystal.Localization;
using Crystal.Messaging;
using Crystal.Navigation;
using Crystal.Services;
using Hanasu.Extensions;
using Hanasu.Model;
using Hanasu.SystemControllers;
using Hanasu.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Xml.Linq;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Search;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.ApplicationSettings;
using Windows.UI.Popups;
using Windows.System;
using Hanasu.Controls;
using Hanasu.Controls.Flyouts;

// The Blank Application template is documented at http://go.microsoft.com/fwlink/?LinkId=234227

namespace Hanasu
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : BaseCrystalApplication
    {

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            this.UnhandledException += App_UnhandledException;

#if DEBUG
            DebugSettings.EnableFrameRateCounter = true;
            //DebugSettings.IsOverdrawHeatMapEnabled = true;
            DebugSettings.IsBindingTracingEnabled = true;
#endif
        }

        void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("UnusualErrorHeader"),
                        LocalizationManager.GetLocalizedValue("UnusualErrorMsg") + Environment.NewLine + e.Message + Environment.NewLine + e.Exception.StackTrace);

            e.Handled = true;
        }

        public static Windows.Storage.StorageFolder AppFolder = null;
        public static AppSettings HanasuAppSettings = null;

        protected override async void PreStartup()
        {
            //set some Crystal initialization values.

            EnableSelfAssemblyResolution = true;
            EnableCrystalLocalization = true;

            try
            {
                ServiceManager.RegisterService<Hanasu.Services.MessageBoxService>();
            }
            catch (Exception)
            {
            }

            NetworkCostController.Initialize();

            await GetAppFolder();

            //try and load settings
            LoadSettings();

            base.PreStartup();
        }

        private static void LoadSettings()
        {
            //try and load settings
            HanasuAppSettings = new AppSettings();

            if (ApplicationData.Current.LocalSettings.Values.Count == 0)
            {
                //set default settings
                HanasuAppSettings.PreferedApplicationTheme = Enum.GetName(typeof(ApplicationTheme), ApplicationTheme.Dark);
                HanasuAppSettings.PreferedChromeBackgroundColor = Windows.UI.Color.FromArgb(255, 0, 20, 78).ToString();

                SaveSettings();
            }
            else
            {
                foreach (var setting in ApplicationData.Current.LocalSettings.Values)
                {
                    HanasuAppSettings.SetProperty(setting.Key, setting.Value);
                }
            }
        }

        private static void SaveSettings()
        {
            HanasuAppSettings.IteratePropertiesAndValues((property, value) =>
            {
                ApplicationData.Current.LocalSettings.Values[property] = value;
            });
            ApplicationData.Current.SignalDataChanged();
        }

        private static async System.Threading.Tasks.Task GetAppFolder()
        {
            try
            {
                AppFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFolderAsync("Hanasu");
            }
            catch { }

            if (AppFolder == null)
                AppFolder = await Windows.Storage.ApplicationData.Current.LocalFolder.CreateFolderAsync("Hanasu");
        }


        internal async Task LoadStations()
        {
            if (AvailableStations == null)
                AvailableStations = new ObservableCollection<Station>();
            else
                if (AvailableStations.Count > 0)
                    AvailableStations.Clear();

            if (NetworkCostController.IsConnectedToInternet)
            {
                StorageFile localRepo = null;

                try
                {
                    localRepo = await AppFolder.GetFileAsync("Stations.xml");
                }
                catch (Exception)
                {

                }

                try
                {
                    if (localRepo == null)
                    {
                        localRepo = await App.AppFolder.CreateFileAsync("Stations.xml");

                        var str = await localRepo.OpenAsync(FileAccessMode.ReadWrite);

                        var http = new HttpClient();
                        var data = await http.GetByteArrayAsync("https://raw.github.com/Amrykid/Hanasu/master/Stations.xml");

                        await str.WriteAsync(data.AsBuffer());
                        await str.FlushAsync();

                        str.Dispose();

                        http.Dispose();
                    }
                }
                catch (Exception)
                {
                    localRepo = null;
                }


                XDocument doc = null;

                if (localRepo == null)
                    using (var http = new HttpClient())
                    {
                        //making things asynchronous
                        doc = XDocument.Parse(await http.GetStringAsync("https://raw.github.com/Amrykid/Hanasu/master/Stations.xml")); //XDocument.Load("https://raw.github.com/Amrykid/Hanasu/master/Stations.xml");
                    }
                else
                    doc = XDocument.Load(localRepo.Path);

                var stationsElement = doc.Element("Stations");

                var stations = from x in stationsElement.Elements("Station")
                               where x.ContainsElement("StationType") ? x.Element("StationType").Value != "TV" : true
                               select new Station()
                               {
                                   Title = x.Element("Name").Value,
                                   StreamUrl = x.Element("DataSource").Value,
                                   PreprocessorFormat = x.ContainsElement("ExplicitExtension") ? x.Element("ExplicitExtension").Value : string.Empty,
                                   ImageUrl = x.ContainsElement("Logo") ? x.Element("Logo").Value : null,
                                   UnlocalizedFormat = x.Element("Format").Value,
                                   Format = LocalizationManager.GetLocalizedValue("Group" + x.Element("Format").Value),
                                   Subtitle = LocalizationManager.GetLocalizedValue("StationSubtitle"),
                                   ServerType = x.ContainsElement("ServerType") ? x.Element("ServerType").Value : "Raw",
                                   HomepageUrl = x.ContainsElement("Homepage") ? new Uri(x.Element("Homepage").Value) : null
                               };

                foreach (var x in stations)
                    AvailableStations.Add(x);
            }
            else
            {
                Crystal.Services.ServiceManager.Resolve<Crystal.Services.IMessageBoxService>()
                    .ShowMessage(
                        LocalizationManager.GetLocalizedValue("InternetConnectionHeader"),
                        LocalizationManager.GetLocalizedValue("NoInternetConnectionMsg"));
            }

            //          <Stations>
            //<Station>
            //  <Name>AnimeNfo</Name>
            //  <DataSource>http://www.animenfo.com/radio/listen.m3u</DataSource>
            //  <Homepage>http://www.animenfo.com/</Homepage>
            //  <Format>Anime</Format>
            //  <City>Tokyo, Japan</City>
            //  <Language>English</Language>
            //  <Cacheable>True</Cacheable>
            //  <ExplicitExtension>.m3u</ExplicitExtension>
            //  <Schedule type="page">http://www.animenfo.com/radio/schedule.php</Schedule>
            //  <Logo>http://d1i6vahw24eb07.cloudfront.net/s54119q.png</Logo>
            //  <ServerType>Shoutcast</ServerType>
            //</Station>

        }

        public ObservableCollection<Station> AvailableStations { get; set; }


        protected override async void PostStartupSearchActivated(SearchActivatedEventArgs args)
        {
            SetupSettingsCharm();

            RootFrame.Style = Resources["RootFrameStyle"] as Style; // Fixes background audio issue across pages 
            // http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/241ba3b4-3e2a-4f9b-a704-87c7b1be7988/

            await LoadStations();

            if (RootFrame.CurrentSourcePageType == null)
                //App was just activated via search but it wasn't already running. We go to the main page first to have it as the home window.
                NavigationService.NavigateTo<MainPageViewModel>(new KeyValuePair<string, string>("search", args.QueryText));
            else
                NavigationService.NavigateTo<SearchPageViewModel>(new KeyValuePair<string, string>("query", args.QueryText));

            // Ensure the current window is active
            Window.Current.Activate();

        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        //protected override void OnLaunched(LaunchActivatedEventArgs args)
        protected override void PostStartupNormalLaunch(LaunchActivatedEventArgs args)
        {
            SetupSettingsCharm();

            RootFrame.Style = Resources["RootFrameStyle"] as Style; // Fixes background audio issue across pages http://social.msdn.microsoft.com/Forums/en-US/winappswithcsharp/thread/241ba3b4-3e2a-4f9b-a704-87c7b1be7988/

            Frame rootFrame = RootFrame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                if (args.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            NavigationService.NavigateTo<MainPageViewModel>(new KeyValuePair<string, string>("args", args.Arguments));

            // Ensure the current window is active
            Window.Current.Activate();
        }

        private void SetupSettingsCharm()
        {
            SettingsPane.GetForCurrentView().CommandsRequested += App_CommandsRequested;
        }

        void App_CommandsRequested(SettingsPane sender, SettingsPaneCommandsRequestedEventArgs args)
        {
            //http://rohitshaw.com/2012/11/06/adding-a-settings-panel-in-charms-in-windows-8-app-about-and-privacy-policy/

            // Add an About command
            var about = new SettingsCommand("about", "About", (handler) =>
            {
                var settings = new Flyout(Flyout.ShowSettingsCharmBackButtonAction);
                settings.Header = handler.Label;
                settings.Background = new SolidColorBrush(Hanasu.Tools.ColorHelper.GetColorFromHexString(HanasuAppSettings.PreferedChromeBackgroundColor));
                settings.FlyoutContent = new Hanasu.View.Flyouts.AboutFlyoutControl();
                //settings.Content = new About();
                //settings.HeaderBrush = new SolidColorBrush(_background);
                //settings.Background = new SolidColorBrush(_background);
                //settings.HeaderText = "About";
                //settings.IsOpen = true;
                settings.Show();
            });
            args.Request.ApplicationCommands.Add(about);


            var preferences = new SettingsCommand("settings", "Preferences", (handler) =>
                {
                    var settings = new Flyout(Flyout.ShowSettingsCharmBackButtonAction);
                    settings.Header = handler.Label;
                    settings.Background = new SolidColorBrush(Hanasu.Tools.ColorHelper.GetColorFromHexString(HanasuAppSettings.PreferedChromeBackgroundColor));
                    settings.FlyoutContent = new Hanasu.View.Flyouts.AboutFlyoutControl();
                    settings.Show();
                });
            args.Request.ApplicationCommands.Add(preferences);


            // Adding a Privacy Policy
            var privacy = new SettingsCommand("privacypolicy", "Privacy Policy", OpenPrivacyPolicy);
            args.Request.ApplicationCommands.Add(privacy);

        }

        private async void OpenPrivacyPolicy(IUICommand command)
        {
            return;

            Uri uri = new Uri("http://Add A Link for your Privacy policy");
            await Launcher.LaunchUriAsync(uri);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: Save application state and stop any background activity

            SaveSettings();

            deferral.Complete();
        }

        public Windows.Media.PlayTo.PlayToManager ptm = null;

        public MediaElement MediaElement
        {
            get
            {
                DependencyObject rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);

                return (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
            }
        }

        public MainPageViewModel MainPageViewModel { get; set; }

    }
}
