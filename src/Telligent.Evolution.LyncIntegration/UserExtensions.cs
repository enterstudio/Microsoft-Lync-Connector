using System;
using Telligent.Evolution.Extensibility.Api.Entities.Version1;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Caching.Version1;

namespace Telligent.Evolution.Extensions.Lync
{
	public static class UserExtensions
	{
        public static string LyncAttribute = "lync-presence";

		public static string SipUri(this User user)
		{
		    var sip = user.ProfileFields["Sip"];
			return sip != null && !string.IsNullOrEmpty(sip.Value) ? sip.Value : user.PrivateEmail;
		}

        public static void SipUri(this User user, string sip)
        {
            user.ProfileFields.Get("Sip").Value = sip;

            PublicApi.Users.Update(new UsersUpdateOptions
            {
                Id = user.Id,
                ProfileFields = user.ProfileFields
            });
        }

	    public static string Presence(this User user)
	    {
            var presence = CacheService.Get(string.Concat("LYNC-PRES-", user.Id.GetValueOrDefault()), CacheScope.Context | CacheScope.Process) as string;
	        return presence ?? string.Empty;
	    }

        public static void ClearPresence(this User user)
        {
            CacheService.Remove(string.Concat("LYNC-PRES-", user.Id.GetValueOrDefault()), CacheScope.Context | CacheScope.Process);
        }

        public static void Presence(this User user, string presence)
        {
            CacheService.Put(string.Concat("LYNC-PRES-", user.Id.GetValueOrDefault()), presence, CacheScope.Context | CacheScope.Process, TimeSpan.FromSeconds(30));
        }

		public static string Domain(this User user)
		{ 
			var sipUri = user.SipUri();
			return string.IsNullOrEmpty(sipUri) ? string.Empty : new System.Net.Mail.MailAddress(sipUri).Host;
		}
	}
}
