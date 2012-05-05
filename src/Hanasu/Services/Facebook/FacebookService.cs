using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hanasu.Services.Settings;
using Hanasu.Services.Events;
using System.Xml.Linq;
using System.Dynamic;
using Facebook;
using System.Windows;
using System.Windows.Controls;

namespace Hanasu.Services.Facebook
{
    public class FacebookService : IStaticService, IShareProviderService
    {
        #region Hidden
        private const long APPID = 379696665416160;
        #endregion

        static FacebookService()
        {
            if (!IsInitialized)
                Initialize();
        }
        public static bool IsInitialized { get; private set; }
        internal static FacebookClient fb = null;
        public static void Initialize()
        {
            if (IsInitialized)
                return;

            FacebookEnabled = false;

            Instance = new FacebookService();
            //Instance.Initialize();

            EventService.AttachHandler(EventType.Settings_Loaded, OnSettingsLoaded);
            EventService.AttachHandler(EventType.Settings_Saving, OnSettingsSaving);
            EventService.AttachHandler(EventType.Settings_Created, OnSettingsSaving);
            EventService.AttachHandler(EventType.Song_Liked, OnSongLiked);
            IsInitialized = true;
        }
        private static void OnSongLiked(EventInfo ei)
        {
            if (FacebookEnabled)
            {
                if (Instance.NeedsToAuth)
                {
                    DoFirstTimeAuth();

                    fb = new FacebookClient(FBAccessToken);
                }

                if (Instance.NeedsToAuth)
                {
                    Hanasu.Services.Notifications.NotificationsService.AddNotification("Facebook Authorization Failed",
                        "Was unable to authenicate with Facebook.", 3000, true, Notifications.NotificationType.Error);
                    return; //Auth failed. Stop
                }
                else
                {
                    //Post song

                    MainWindow.SongLikedEventInfo sl = (MainWindow.SongLikedEventInfo)ei;


                    dynamic parameters = new ExpandoObject();
                    parameters.message = "I liked [" + sl.CurrentSong.TrackTitle + " by " + sl.CurrentSong.Artist + "].";
                    parameters.picture = "https://github.com/Amrykid/Hanasu/raw/master/res/metro.icons/black/favs.png";
                    parameters.link = sl.CurrentStation.Homepage.ToString();
                    parameters.name = "Streaming [" + sl.CurrentStation.Name + "] powered by Hanasu.";


                    dynamic dlHanasu = new ExpandoObject();
                    dlHanasu.name = "Download Hanasu";
                    dlHanasu.link = "https://github.com/Amrykid/Hanasu/downloads";

                    /*dynamic visitStation = new ExpandoObject();
                    visitStation.name = "Go To " + sl.CurrentStation.Name;
                    visitStation.link = sl.CurrentStation.Homepage.ToString(); */

                    parameters.actions = new ExpandoObject[] { dlHanasu };

                    fb.PostTaskAsync("me/feed", parameters);

                    Hanasu.Services.Notifications.NotificationsService.AddNotification("Song shared",
                        "Post to Facebook was completed.", 3000, true, Notifications.NotificationType.Information);
                }
            }
        }

        private static void OnSettingsLoaded(EventInfo ei)
        {
            Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo sdei = (Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo)ei;

            FacebookEnabled = bool.Parse(sdei.SettingsElement.Element("UseFacebook").Value);
            FBAccessToken = sdei.SettingsElement.Element("FBAccessToken").Value;

            if (FacebookEnabled && String.IsNullOrEmpty(FBAccessToken) == false)
                fb = new FacebookClient(FBAccessToken);
        }
        private static void OnSettingsSaving(EventInfo ei)
        {
            Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo sdei = (Hanasu.Services.Settings.SettingsService.SettingsDataEventInfo)ei;

            sdei.SettingsElement.Add(
                        new XElement("UseFacebook", FacebookEnabled.ToString()),
                        new XElement("FBAccessToken", FBAccessToken));
        }

        public static bool FacebookEnabled { get; set; }
        public static string FBAccessToken { get; set; }

        public static FacebookService Instance { get; set; }

        public string SiteName
        {
            get { return "Facebook"; }
        }

        public bool Share(object data)
        {
            throw new NotImplementedException();
        }

        public bool NeedsToAuth
        {
            get { return String.IsNullOrEmpty(FBAccessToken); }
        }

        public void Initalize()
        {
            //Not needed.
            return;
        }

        public bool UseFacebook
        {
            get { return FacebookEnabled; }
        }


        private static string GetLoginUrl()
        {
            dynamic parameters = new ExpandoObject();
            parameters.client_id = APPID;
            parameters.redirect_uri = "https://www.facebook.com/connect/login_success.html";

            // The requested response: an access token (token), an authorization code (code), or both (code token).
            parameters.response_type = "token";

            // list of additional display modes can be found at http://developers.facebook.com/docs/reference/dialogs/#display
            parameters.display = "popup";

            parameters.scope = "publish_stream";

            // generate the login url
            var tmp = new FacebookClient();
            return tmp.GetLoginUrl(parameters).ToString();
        }
        private static void DoFirstTimeAuth()
        {
            try
            {
                //base on code from http://blog.prabir.me/post/Facebook-CSharp-SDK-Writing-your-First-Facebook-Application-v6.aspx

                System.Windows.Window wc = new System.Windows.Window();
                wc.WindowState = System.Windows.WindowState.Normal;
                wc.WindowStyle = System.Windows.WindowStyle.ToolWindow;
                wc.ResizeMode = System.Windows.ResizeMode.NoResize;
                wc.Owner = Application.Current.MainWindow;
                wc.Height = Application.Current.MainWindow.ActualHeight;
                wc.Width = Application.Current.MainWindow.ActualWidth;
                wc.WindowStartupLocation = WindowStartupLocation.CenterOwner;

                var web = new WebBrowser();
                wc.Content = web;


                System.Windows.Navigation.NavigatedEventHandler handler = null;
                handler = new System.Windows.Navigation.NavigatedEventHandler((object sender, System.Windows.Navigation.NavigationEventArgs e) =>
                  {

                      var fb = new FacebookClient();
                      FacebookOAuthResult result;
                      if (fb.TryParseOAuthCallbackUrl(e.Uri, out result))
                      {
                          // The url is the result of OAuth 2.0 authentication

                          if (result.IsSuccess)
                          {
                              var accesstoken = result.AccessToken;

                              FBAccessToken = accesstoken;

                              fb = new FacebookClient(FBAccessToken);

                              wc.DialogResult = true;
                          }
                          else
                          {
                              var errorDescription = result.ErrorDescription;
                              var errorReason = result.ErrorReason;

                              wc.DialogResult = false;
                          }
                      }
                      else
                      {
                          // The url is NOT the result of OAuth 2.0 authentication.
                      }
                  });

                web.Navigated += handler;

                web.Navigate(GetLoginUrl());

                wc.ShowDialog();

                web.Navigated -= handler;
            }
            catch (Exception)
            {
            }
        }
    }
}
