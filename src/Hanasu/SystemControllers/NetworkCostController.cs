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
            }

            Initialized = true;
        }

        static void NetworkInformation_NetworkStatusChanged(object sender)
        {
            string connectionProfileInfo = string.Empty;

            // get the ConnectionProfile that is currently used to connect to the Internet                
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

                var connCost = InternetConnectionProfile.GetConnectionCost();

                ApproachingDataLimit = connCost.ApproachingDataLimit;

                if (connCost.ApproachingDataLimit)
                {
                    if (ApproachingDataLimitEvent != null)
                        ApproachingDataLimitEvent();
                }

            }
        }

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
}
