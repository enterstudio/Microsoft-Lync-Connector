using System;
using Microsoft.Rtc.Collaboration;
using Microsoft.Rtc.Collaboration.Presence;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensions.Lync.Plugins;
using User = Telligent.Evolution.Extensibility.Api.Entities.Version1.User;

namespace Telligent.Evolution.Extensions.Lync
{
	public class LyncContext
    {
	    public static LyncContext Instance()
	    {
	        return new LyncContext();
	    }

	    public string Presence(string username)
	    {
            var user = PublicApi.Users.Get(new UsersGetOptions { Username = username });
	        return UserPresence(user).ToString();
	    }

        public string Presence(int userId)
        {
            var user = PublicApi.Users.Get(new UsersGetOptions { Id = userId });
            return UserPresence(user).ToString();
        }

        public string Presence(User user)
        {
            return UserPresence(user).ToString();
        }

        public UserStatus UserPresence(User user)
	    {
            var presence = user.Presence();
            if (string.IsNullOrEmpty(presence)) return GetUserPresence(user);

            Availability availability;
            return Enum.TryParse(presence, out availability) ? (UserStatus)availability : UserStatus.Offline;
	    }

        public void SetPresence(User user, PreferredUserStatus status)
	    {
            if (!LyncCollaboration.HasStarted) LyncCollaboration.Start();

            var state = PresenceState.UserAway;

            switch (status)
            {
                case PreferredUserStatus.BeRightBack:
                    state = PresenceState.UserBeRightBack;
                    break;
                case PreferredUserStatus.Busy:
                    state = PresenceState.UserBusy;
                    break;
                case PreferredUserStatus.DoNotDisturb:
                    state = PresenceState.UserDoNotDisturb;
                    break;
                case PreferredUserStatus.Offwork:
                    state = PresenceState.UserOffWork;
                    break;
                case PreferredUserStatus.Online:
                    state = PresenceState.UserAvailable;
                    break;
            }

            user.ClearPresence();

            var host = Plugin.LyncPlugin.Configuration.GetString("host");
            var appPort = Plugin.LyncPlugin.Configuration.GetInt("appPort");
            var sip = "sip:" + user.SipUri();
            var endpoint = new UserEndpoint(LyncCollaboration.Platform, new UserEndpointSettings(sip, host, appPort));
            
            endpoint.EndEstablish(endpoint.BeginEstablish(null, null));
            endpoint.LocalOwnerPresence.EndSubscribe(endpoint.LocalOwnerPresence.BeginSubscribe(null, null));
            endpoint.LocalOwnerPresence.EndPublishPresence(endpoint.LocalOwnerPresence.BeginPublishPresence(new PresenceCategory[] { state }, null, null));
            endpoint.EndTerminate(endpoint.BeginTerminate(null, null));
	    }

        private UserStatus GetUserPresence(User user)
	    {
	        if (!LyncCollaboration.HasStarted) LyncCollaboration.Start();

            var sipUri = "sip:" + user.SipUri();
            var endpoint = LyncCollaboration.UserEndpoint;
            if (endpoint == null) throw new Exception("Lync User Endpoint is null.");

            var presenceQuery = endpoint.PresenceServices.EndPresenceQuery(endpoint.PresenceServices.BeginPresenceQuery(new[] { sipUri }, new[] { "state" }, null, null, null));
            var presence = LyncCollaboration.ProcessPresenceNotification(presenceQuery, user);

            if (LyncCollaboration.RemotePresence != null)
            {
                var target = new RemotePresentitySubscriptionTarget(sipUri);
                LyncCollaboration.RemotePresence.StartSubscribingToPresentities(new[] { target });
            }

            Availability availability;
            return Enum.TryParse(presence, out availability) ? (UserStatus)availability : UserStatus.Offline;
	    }
    }
}
