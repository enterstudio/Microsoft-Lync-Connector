using System;
using System.Collections.Generic;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Rest.Infrastructure.Version1;
using Telligent.Evolution.Extensibility.Rest.Version2;
using Telligent.Evolution.Rest.Infrastructure.Version2;
using HttpMethod = Telligent.Evolution.Extensibility.Rest.Version2.HttpMethod;

namespace Telligent.Evolution.Extensions.Lync.RestApi
{
	public class LyncRoutePlugin : IRestEndpoints
	{
		ILyncCommunicationService _lyncCommunicationService;

		public string Name
		{
			get { return "Lync Integration REST API"; }
		}

		public string Description
		{
			get { return "REST routes for Lync Integration Plugin"; }
		}
		
		public void Initialize()
		{
			_lyncCommunicationService = new LyncCommunicationService();
		}

		public void Register(IRestEndpointController restRoutes)
		{
			// Retrieving user status by userId
			restRoutes.Add(2, "lync/userstatus/{userId}", HttpMethod.Get,
				req =>
				{
					var response = new DefaultRestResponse { Name = "UserStatus" };

					try
					{
						int userId;
						if (!Int32.TryParse(req.PathParameters["userId"] as string, out userId)) throw new ArgumentNullException("userId", "UserId is required");

						var user = PublicApi.Users.Get(new UsersGetOptions { Id = userId });
						if (user == null) throw new ArgumentNullException("user", "User not found");

                        response.Data = new { status = _lyncCommunicationService.GetUserStatus(user).ToString(), userid = userId };
						return response;
					}
					catch (Exception ex)
					{
						response.Errors = new[] { ex.Message };
						return response;
					}
				},
				new RestEndpointDocumentation
				{
					EndpointDocumentation = new RestEndpointDocumentationAttribute
					{
						Resource = "UserStatus",
						Action = "Get User Status"
					},
					RequestDocumentation = new List<RestRequestDocumentationAttribute>
					{
						new RestRequestDocumentationAttribute
						{
							Name = "UserId",
							Type = typeof(int),
							Description = "Id of the user",
							Required = RestRequired.Required
						}
					},
					ResponseDocumentation = new RestResponseDocumentationAttribute
					{
						Name = "UserStatus",
						Description = "User Status. Can be one of the list - 'Away', 'BeRightBack', 'Busy', 'DoNotDisturb', 'IdleBusy', 'IdleOnline', 'Offwork', 'Online' or 'Offline'",
						Type = typeof(UserStatus)
					}
				});

			// Retrieving current user status
			restRoutes.Add(2, "lync/userstatus", HttpMethod.Get,
				req =>
				{
					var response = new DefaultRestResponse { Name = "UserStatus" };
                    var user = PublicApi.Users.AccessingUser;

					try
					{
						response.Data = new { status = _lyncCommunicationService.GetCurrentUserStatus().ToString(), userid = user.Id.GetValueOrDefault()};
						return response;
					}
					catch (Exception ex)
					{
						response.Errors = new[] { ex.Message };
						return response;
					}
				},
				new RestEndpointDocumentation
				{
					EndpointDocumentation = new RestEndpointDocumentationAttribute
					{
						Resource = "UserStatus",
						Action = "Get Current User Status"
					},
					RequestDocumentation = new List<RestRequestDocumentationAttribute>(),
					ResponseDocumentation = new RestResponseDocumentationAttribute
					{
						Name = "UserStatus",
						Description = "User Status. Can be one of the list - 'Away', 'BeRightBack', 'Busy', 'DoNotDisturb', 'IdleBusy', 'IdleOnline', 'Offwork', 'Online' or 'Offline'",
						Type = typeof(UserStatus)
					}
				});

			// Updating current user status
			restRoutes.Add(2, "lync/userstatus", HttpMethod.Post,
				req =>
				{
					var response = new DefaultRestResponse { Name = "UserStatus" };
                    var user = PublicApi.Users.AccessingUser;

					try
					{
						PreferredUserStatus userStatus;
						if (!Enum.TryParse(req.Form["userStatus"], out userStatus))
							throw new ArgumentNullException("userStatus", "UserStatus not specified or has an invalid value. Allowed values - 'Away', 'BeRightBack', 'Busy', 'DoNotDisturb', 'Offwork', 'Online'");

						_lyncCommunicationService.SetCurrentUserStatus(userStatus);

                        response.Data = new { status = _lyncCommunicationService.GetCurrentUserStatus().ToString(), userid = user.Id.GetValueOrDefault() };

						return response;
					}
					catch (Exception ex)
					{
						response.Errors = new[] { ex.Message };
						return response;
					}
				},
				new RestEndpointDocumentation
				{
					EndpointDocumentation = new RestEndpointDocumentationAttribute
					{
						Resource = "UserStatus",
						Action = "Set Current User Status"
					},
					RequestDocumentation = new List<RestRequestDocumentationAttribute>
					{
						new RestRequestDocumentationAttribute
						{
							Name = "UserStatus",
							Type = typeof(PreferredUserStatus),
							Description = "New status of the user. Allowed values - 'Away', 'BeRightBack', 'Busy', 'DoNotDisturb', 'Offwork', 'Online'",
							Required = RestRequired.Required
						}
					},
					ResponseDocumentation = new RestResponseDocumentationAttribute
					{
						Name = "UserStatus",
						Description = "User Status. Can be one of the list - 'Away', 'BeRightBack', 'Busy', 'DoNotDisturb', 'IdleBusy', 'IdleOnline', 'Offwork', 'Online' or 'Offline'",
						Type = typeof(UserStatus)
					}
				});
		}
	}

}
