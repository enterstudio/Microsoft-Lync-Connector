using System.Collections.Generic;
using System.IO;
using System.Xml;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using System;
using System.Security.Cryptography.X509Certificates;
using Telligent.Evolution.Extensibility.Api.Entities.Version1;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensions.Lync.Plugins;
using Telligent.Evolution.Extensions.Lync.Utils;

namespace Telligent.Evolution.Extensions.Lync
{
    public static class LyncCollaboration
    {
        public enum PresenceEnum
        {
            Unknown = 0,
            Online = 3500,
            Away = 15500,
            Busy = 6500,
            Offline = 18000,
            DoNotDisturb = 9500,
            BeRightBack = 12500
        }

        internal static CollaborationPlatform Platform = null;
        internal static ApplicationEndpoint AppEndpoint = null;
        internal static UserEndpoint UserEndpoint = null;
        internal static RemotePresenceView RemotePresence = null;

        private const string UserAgent = "ZimbraCommunity";

        public static bool HasStarted
        {
            get { return AppEndpoint != null && AppEndpoint.State == LocalEndpointState.Established && UserEndpoint != null && UserEndpoint.State == LocalEndpointState.Established; }
        }

        public static void Start()
        {
            try
            {
                var host = Plugin.LyncPlugin.Configuration.GetString("host");
                var thumbprint = Plugin.LyncPlugin.Configuration.GetString("thumbprint");
                var gruu = Plugin.LyncPlugin.Configuration.GetString("gruu");
                var trustPort = Plugin.LyncPlugin.Configuration.GetInt("trustedPort");
                var appPort = Plugin.LyncPlugin.Configuration.GetInt("appPort");
                var sip = Plugin.LyncPlugin.Configuration.GetString("accountSip");

                var platformSettings = new ServerPlatformSettings(UserAgent, host, trustPort, gruu, CertificateUtil.GetCertificate(StoreName.My, StoreLocation.LocalMachine, thumbprint));

                Platform = new CollaborationPlatform(platformSettings);
                AppEndpoint = new ApplicationEndpoint(Platform, new ApplicationEndpointSettings(sip, host, appPort) { UseRegistration = true });

                Log("Starting Lync platform.");
                Platform.EndStartup(Platform.BeginStartup(null, null));
                Log("Lync platform started.");

                AppEndpoint.EndEstablish(AppEndpoint.BeginEstablish(null, null));

                UserEndpoint = new UserEndpoint(Platform, new UserEndpointSettings(sip, host, appPort) { AutomaticPresencePublicationEnabled = true });
                UserEndpoint.EndEstablish(UserEndpoint.BeginEstablish(null, null));

                RemotePresence = new RemotePresenceView(UserEndpoint, new RemotePresenceViewSettings());
                RemotePresence.PresenceNotificationReceived += PresenceNotificationReceived;
            }
            catch (Exception ex)
            {
                Error(ex.Message);
            }
        }

        private static void PresenceNotificationReceived(object sender, RemotePresentitiesNotificationEventArgs e)
        {
            ProcessPresenceNotification(e.Notifications);
        }

        internal static string ProcessPresenceNotification(IEnumerable<RemotePresentityNotification> remotePresentityNotifications, User user = null)
        {
            var presence = PresenceEnum.Unknown.ToString();

            try
            {
                var enumerator = remotePresentityNotifications.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    var notification = enumerator.Current;
                    foreach (var category in notification.Categories)
                    {
                        var xml = category.CreateInnerDataXml();
                        if (xml == null || xml.Length <= 0) continue;

                        var stateCategory = new XmlDocument();
                        var reader = new StringReader(xml);

                        stateCategory.Load(reader);

                        var availabilityNodeList = stateCategory.GetElementsByTagName("availability");
                        var sipAddress = notification.PresentityUri.ToLowerInvariant();

                        if (availabilityNodeList.Count <= 0) continue;

                        var availability = Convert.ToInt64(availabilityNodeList[0].InnerText);

                        presence = Enum.IsDefined(typeof(PresenceEnum), (int)availability) ? ((PresenceEnum)availability).ToString() : PresenceEnum.Unknown.ToString();

                        if (user != null)
                        {
                            var userid = user.Id.GetValueOrDefault();
                            Plugin.LyncPlugin.SocketController.Clients.Send(userid, "presence", new { presence, userid });
                            user.Presence(presence);   
                        }

                        Log(string.Format("Lync Presence {0} ({1})", sipAddress, presence));
                    }
                }
                return presence;
            }
            catch (Exception ex)
            {
                Error(ex.Message);
                return presence;
            }
        }

        private static void Error(string message)
        {
            PublicApi.Eventlogs.Write(message, new EventLogEntryWriteOptions { Category = "Lync", EventType = "Error" });
        }

        private static void Log(string message)
        {
            PublicApi.Eventlogs.Write(message, new EventLogEntryWriteOptions { Category = "Lync" });
        }
    }
}
