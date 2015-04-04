using Telligent.Evolution.Extensibility.Api.Entities.Version1;

namespace Telligent.Evolution.Extensions.Lync
{
    public enum UserStatus 
	{
		Away,
		BeRightBack,
		Busy,
		DoNotDisturb,
		IdleBusy,
		IdleOnline,
		Online,
		Offline
	}

	public enum PreferredUserStatus 
	{
		Away,
		BeRightBack,
		Busy,
		DoNotDisturb,
		Offwork,
		Online
	}

	public interface ILyncCommunicationService
	{
		UserStatus GetUserStatus(User user);

		UserStatus GetCurrentUserStatus();

		void SetCurrentUserStatus(PreferredUserStatus status);

		void ReportActivity();
	}
}
