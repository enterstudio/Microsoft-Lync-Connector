using Telligent.Evolution.Extensibility.Api.Entities.Version1;
using Telligent.Evolution.Extensibility.Api.Version1;

namespace Telligent.Evolution.Extensions.Lync
{
	public class LyncCommunicationService : ILyncCommunicationService
	{
		public UserStatus GetUserStatus(User user)
		{
		    return LyncContext.Instance().UserPresence(user);
		}

		public UserStatus GetCurrentUserStatus()
		{
		    var user = PublicApi.Users.AccessingUser;
		    return GetUserStatus(user);
		}

		public void SetCurrentUserStatus(PreferredUserStatus status)
		{
            var user = PublicApi.Users.AccessingUser;
            LyncContext.Instance().SetPresence(user, status);
		}

		public void ReportActivity()
		{
            var user = PublicApi.Users.AccessingUser;
            LyncContext.Instance().UserPresence(user);
		}
	}
}
