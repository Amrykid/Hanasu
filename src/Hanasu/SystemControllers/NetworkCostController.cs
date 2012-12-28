using Crystal.Localization;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Controls;
namespace Hanasu.SystemControllers
{
    public static class NetworkCostController
    {
        public static bool Initialized { get; private set; }

        public static void Initialize()
        {
            if (Initialized) return;

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;

            DetectStatus();

            Initialized = true;
        }

        private static void DetectStatus()
        {
            ConnectionProfile InternetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();

            IsConnectedToNetwork = InternetConnectionProfile != null;

            if (InternetConnectionProfile == null)
            {
                //not connected to internet

                IsConnectedToInternet = false;
            }
            else
            {
                //is connected to internet

                if (InternetConnectionProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess)
                    IsConnectedToInternet = true;
                else
                    IsConnectedToInternet = false;



                //Detect behaviors


                var connCost = InternetConnectionProfile.GetConnectionCost();

                if ((connCost.NetworkCostType == NetworkCostType.Unrestricted || connCost.NetworkCostType == NetworkCostType.Unknown)
                    && connCost.Roaming == false)
                {
                    CurrentNetworkingBehavior = NetworkingBehavior.Normal;
                }
                else if ((connCost.NetworkCostType == NetworkCostType.Fixed || connCost.NetworkCostType == NetworkCostType.Variable)
                    && ((connCost.Roaming == false || connCost.OverDataLimit == false)))
                {
                    CurrentNetworkingBehavior = NetworkingBehavior.Conservative;
                }
                else if (connCost.Roaming || connCost.OverDataLimit)
                {
                    CurrentNetworkingBehavior = NetworkingBehavior.Opt_In;
                }

                ApproachingDataLimit = connCost.ApproachingDataLimit;

                if (connCost.ApproachingDataLimit)
                {
                    if (ApproachingDataLimitEvent != null)
                        ApproachingDataLimitEvent();
                }
            }

        }

        static void NetworkInformation_NetworkStatusChanged(object sender)
        {
            DetectStatus();
        }

        public static NetworkingBehavior CurrentNetworkingBehavior { get; private set; }

        public delegate void ApproachingDataLimitHandler();
        public static event ApproachingDataLimitHandler ApproachingDataLimitEvent;

        public static bool ApproachingDataLimit { get; private set; }
        public static bool IsConnectedToNetwork { get; private set; }
        public static bool IsConnectedToInternet { get; private set; }

        static string GetDataPlanStatusInfo(DataPlanStatus dataPlan)
        {
            string dataplanStatusInfo = string.Empty;
            dataplanStatusInfo = "Dataplan Status Information:\n";
            dataplanStatusInfo += "====================\n";

            if (dataPlan.DataPlanUsage != null)
            {
                dataplanStatusInfo += "Usage In Megabytes : " + dataPlan.DataPlanUsage.MegabytesUsed + "\n";
                dataplanStatusInfo += "Last Sync Time : " + dataPlan.DataPlanUsage.LastSyncTime + "\n";
            }
            else
            {
                dataplanStatusInfo += "Usage In Megabytes : Not Defined\n";
            }

            ulong? inboundBandwidth = dataPlan.InboundBitsPerSecond;
            if (inboundBandwidth.HasValue)
            {
                dataplanStatusInfo += "InboundBitsPerSecond : " + inboundBandwidth + "\n";
            }
            else
            {
                dataplanStatusInfo += "InboundBitsPerSecond : Not Defined\n";
            }

            ulong? outboundBandwidth = dataPlan.OutboundBitsPerSecond;
            if (outboundBandwidth.HasValue)
            {
                dataplanStatusInfo += "OutboundBitsPerSecond : " + outboundBandwidth + "\n";
            }
            else
            {
                dataplanStatusInfo += "OutboundBitsPerSecond : Not Defined\n";
            }

            uint? dataLimit = dataPlan.DataLimitInMegabytes;
            if (dataLimit.HasValue)
            {
                dataplanStatusInfo += "DataLimitInMegabytes : " + dataLimit + "\n";
            }
            else
            {
                dataplanStatusInfo += "DataLimitInMegabytes : Not Defined\n";
            }

            System.DateTimeOffset? nextBillingCycle = dataPlan.NextBillingCycle;
            if (nextBillingCycle.HasValue)
            {
                dataplanStatusInfo += "NextBillingCycle : " + nextBillingCycle + "\n";
            }
            else
            {
                dataplanStatusInfo += "NextBillingCycle : Not Defined\n";
            }

            uint? downloadFileSize = dataPlan.MaxTransferSizeInMegabytes;
            if (downloadFileSize.HasValue)
            {
                dataplanStatusInfo += "MaxDownloadFileSizeInMegabytes : " + downloadFileSize + "\n";
            }
            else
            {
                dataplanStatusInfo += "MaxDownloadFileSizeInMegabytes : Not Defined\n";
            }
            return dataplanStatusInfo;
        }

    }

    /// <summary>
    /// Behaviors as defined here: http://msdn.microsoft.com/en-us/library/windows/apps/xaml/jj835821.aspx
    /// </summary>
    public enum NetworkingBehavior
    {
        /// <summary>
        /// In Normal scenarios, your app should not implement restrictions. The connection should be treated as Unlimited in cost, and Unrestricted by usage charges and capacity constraints.
        /// 
        /// Examples:
        ///     Play an entire HD movie.
        ///     Download a large file without restrictions or UI prompts.
        /// </summary>
        Normal = 1,
        /// <summary>
        /// In conservative scenarios, the app should implement restrictions for optimizing network usage to handle transfers over metered networks.
        /// Examples:
        ///     Play movies in lower resolutions.
        ///     Delay non-critical downloads.
        ///     Avoid pre-fetching of information over a network.
        ///      Switch to a header-only mode when retrieving email messages.
        /// </summary>
        Conservative = 2,
        /// <summary>
        /// For opt-in scenarios, your app should handle cases where the network access cost is significantly higher than the plan cost. For example, when a user is roaming, a mobile carrier may charge a higher rate data usage.
        /// Examples:
        ///     Prompt the user before accessing the network.
        ///     Suspend all background data network activities.
        /// </summary>
        Opt_In = 3
    }
}
