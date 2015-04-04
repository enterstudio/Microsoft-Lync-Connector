using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Telligent.Evolution.Components;
using Telligent.Evolution.Extensibility.Api.Version1;
using Telligent.Evolution.Extensibility.Sockets.Version1;
using Telligent.Evolution.Extensibility.UI.Version1;
using Telligent.Evolution.Extensibility.Version1;

namespace Telligent.Evolution.Extensions.Lync.Plugins
{
    public class UserStatusPlugin : IUserActionLinkPlugin, ITranslatablePlugin, ICategorizedPlugin, IHtmlHeaderExtension, ISocket
	{
		private const string LyncSocketName = "lyncstatus";

        ITranslatablePluginController _translation;
		ISocketController _controller;
		ILyncCommunicationService _lyncCommunicationService;

		public string Name
		{
            get { return "Lync Status Plugin"; }
		}

		public string Description
		{
            get { return "Adds support for displaying/changing user status and starting a conversation."; }
		}

		public void Initialize()
		{
			_lyncCommunicationService = new LyncCommunicationService();
		}

		#region IUserActionLinkPlugin

		public IEnumerable<IUserActionLink> GetActionLinks(int userId, int accessingUserId)
		{
            var links = new List<IUserActionLink>();
            var user = PublicApi.Users.Get(new UsersGetOptions { Id = userId });
            var accessingUser = userId == accessingUserId ? user : PublicApi.Users.Get(new UsersGetOptions() { Id = accessingUserId });

            if (user != null && 
                !string.Equals(user.Username, PublicApi.Users.AnonymousUserName, StringComparison.InvariantCulture) && 
                accessingUser != null && 
                !string.Equals(accessingUser.Username, PublicApi.Users.AnonymousUserName, StringComparison.InvariantCulture))
            {
                links.Add(new UserActionLink
                {
                    Html = string.Concat(
                        "<a href=\"javascript:void(0)\" class=\"internal-link lync-status ui-lyncstatus disabled\" data-userid=\"",
                        userId,
                        "\" data-accessinguserid=\"",
                        accessingUserId,
						"\" data-usersipuri=\"",
						user.SipUri(),
                        "\">",
						_translation.GetLanguageResourceValue("recieving"),
                        "</a>"),
                    LinkTypeName = "LyncStatus"
                });
            }

            return links;
		}

		public bool HasActionLinks(int userId, int accessingUserId)
		{
            return GetActionLinks(userId, accessingUserId).Any();
		}

		#endregion

        #region IHtmlHeaderExtension Members

        public string GetHeader(RenderTarget target)
        {

            var script = new StringBuilder();

			script.Append("<style type = \"text/css\">");
			script.Append(@".site-banner .banner.site .lync-indicator {  
								right: -2px;
								top: 17px;
								position: absolute;
								background-color: #e4e4e4;
								display: block;
								width: 10px;
								height: 10px;
								z-index: 100;
								-webkit-border-radius: 500px;
								-moz-border-radius: 500px;
								border-radius: 500px;
							}
							.site-banner .banner.site .lync-indicator.Online,
							.site-banner .banner.site .lync-indicator.IdleOnline { background-color: #82d34b; }
							.site-banner .banner.site .lync-indicator.Away,
							.site-banner .banner.site .lync-indicator.Offwork,
							.site-banner .banner.site .lync-indicator.BeRightBack{ background-color: #ffeb1e; }
							.site-banner .banner.site .lync-indicator.Busy,
							.site-banner .banner.site .lync-indicator.IdleBusy,
							.site-banner .banner.site .lync-indicator.DoNotDisturb{ background-color: #f40707; }
							");

			script.Append("</style>");

            script.Append("<script type=\"text/javascript\">");

            script.Append(@"jQuery(function($) {

				var popup = null;
				var statuses = {").AppendFormat("'{0}':'{1}',", UserStatus.Away, JavaScript.Encode(_translation.GetLanguageResourceValue("away")))
								  .AppendFormat("'{0}':'{1}',", UserStatus.BeRightBack, JavaScript.Encode(_translation.GetLanguageResourceValue("be_right_back")))
								  .AppendFormat("'{0}':'{1}',", UserStatus.Busy, JavaScript.Encode(_translation.GetLanguageResourceValue("busy")))
								  .AppendFormat("'{0}':'{1}',", UserStatus.DoNotDisturb, JavaScript.Encode(_translation.GetLanguageResourceValue("do_not_disturb")))
								  .AppendFormat("'{0}':'{1}',", UserStatus.IdleBusy, JavaScript.Encode(_translation.GetLanguageResourceValue("idle_busy")))
								  .AppendFormat("'{0}':'{1}',", UserStatus.IdleOnline, JavaScript.Encode(_translation.GetLanguageResourceValue("idle_online")))
								  .AppendFormat("'{0}':'{1}',", UserStatus.Online, JavaScript.Encode(_translation.GetLanguageResourceValue("online")))
								  .AppendFormat("'{0}':'{1}',", PreferredUserStatus.Offwork, JavaScript.Encode(_translation.GetLanguageResourceValue("offwork")))
								  .AppendFormat("'{0}':'{1}' ", UserStatus.Offline, JavaScript.Encode(_translation.GetLanguageResourceValue("offline"))).Append(@"};

				var preferred = [").AppendFormat("'{0}',", PreferredUserStatus.Away)
								   .AppendFormat("'{0}',", PreferredUserStatus.BeRightBack)
								   .AppendFormat("'{0}',", PreferredUserStatus.Busy)
								   .AppendFormat("'{0}',", PreferredUserStatus.DoNotDisturb)
								   .AppendFormat("'{0}',", PreferredUserStatus.Offwork)
								   .AppendFormat("'{0}' ", PreferredUserStatus.Online).Append(@"];
				
				var classes = [];
				for(var status in statuses) { classes.push(status) };
				classes = classes.join(' ');

				jQuery.telligent.evolution.ui.components.lyncstatus = {
					setup: function() {
						var html = '';
						for(var status, i=0; i < preferred.length; i++) {
							status = preferred[i]
							html += '<li><a href=""javascript:void(0)"" class=""internal-link"" data-status=""' + status + '"">' + statuses[status] + '</a></li>';
						}
						html = '<ul>' + html + '</ul>';
						popup = $('<div></div>')
							.glowPopUpPanel({
								cssClass: 'links-popup-panel lync-statuses-panel',
								zIndex: 1500,
								hideOnDocumentClick: false
							})
							.glowPopUpPanel('html', html)
							.on('glowPopUpPanelShown', function() {
								$.telligent.evolution.messaging.publish('ui.links.show'); 
								setTimeout(function(){
									if(!popup.data('over'))
										popup.glowPopUpPanel('hide'); 
								}, 1000);
							})
							.on('glowPopUpPanelHidden', function() {
								$.telligent.evolution.messaging.publish('ui.links.hide'); 
								popup.data('over', false);
							});

						$(document).on('mouseenter', '.lync-statuses-panel', function(){
								popup.data('over', true);
							})
							.on('mouseleave', '.lync-statuses-panel', function(){
								popup.data('over', false);
							})
							.on('glowDelayedMouseLeave', '.lync-statuses-panel', 500, function(el) {
								el.stopPropagation();
								popup.glowPopUpPanel('hide'); 
							})
							.on('click', '.lync-statuses-panel .internal-link', function(el) {
								var statusLink = $('.lync-status');
								var old = statusLink.text();
								statusLink.text('").Append(JavaScript.Encode(_translation.GetLanguageResourceValue("processing"))).Append(@"');
								$.telligent.evolution.post({
									url: $.telligent.evolution.site.getBaseUrl() + 'api.ashx/v2/lync/userstatus.json',
									data: { UserStatus: $(this).data('status') },
									dataType: 'json',
									success: function(response) { 
									    $.telligent.evolution.messaging.publish('lyncstatus.updated.' + response.UserStatus.userid, response.UserStatus.status); 
									},
									error: function(response) { statusLink.text(old); }
								});

								popup.glowPopUpPanel('hide'); 
							});
					},
					add: function(e, options) {
						
						var sId = $.telligent.evolution.messaging.subscribe('lyncstatus.updated.' + options.userid, function(status) {	
							try{
							    
							    $( "".lync-indicator[data-userid*='"" + options.userid + ""']"" )
								    .attr('data-tip', statuses[status])
								    .removeClass(classes)
								    .addClass(status);

							    if (e.hasClass('lync-status')){
								    e.text(statuses[status]).removeClass('disabled'); 
							    }
							}catch (err){
							    $.telligent.evolution.messaging.unsubscribe(sId);
								return;
							}
						});

						$.telligent.evolution.sockets.lync.on('presence', function(data) {  
				            $.telligent.evolution.messaging.publish('lyncstatus.updated.' + data.userid, data.presence);
						});

						$.telligent.evolution.get({
				            url: $.telligent.evolution.site.getBaseUrl() + (options.userid == options.accessinguserid ? 'api.ashx/v2/lync/userstatus.json' : 'api.ashx/v2/lync/userstatus/{UserId}.json'),
				            data: { UserId: options.userid },
				            dataType: 'json',
				            success: function(response) { 

								$.telligent.evolution.messaging.publish('lyncstatus.updated.' + response.UserStatus.userid, response.UserStatus.status);

								if (options.userid != options.accessinguserid) {
									e.attr('href', 'sip:' + options.usersipuri);
								}
								else {
									e.on('click', function() {
										popup.glowPopUpPanel('show', e); 
									});
								}
							},
				            error: function(response){ 
								$.telligent.evolution.messaging.publish('lyncstatus.updated.' + options.userid, 'offline'); 
				            }
			            });
					}
				};

				$.telligent.evolution.messaging.subscribe('socket.connected', function() {					
					//report user activity every 45 seconds
					setInterval(function() {
						$.telligent.evolution.sockets.").Append(LyncSocketName).Append(@".send('reportActivity');
					}, 45000);
				});");

			script.AppendFormat(@"$('.site-banner .user-links a.user:not(.with-icon)').wrap('<span style=""position:relative""></span>').after('<a href=""javascript:void(0)"" class=""ui-lyncstatus ui-tip lync-indicator disabled"" data-userid=""{0}"" data-accessinguserid=""{0}""/>')", PublicApi.Users.AccessingUser.Id);

			script.Append(@"});");

            script.Append("</script>");

            return script.ToString();
        }

        public bool IsCacheable
        {
            get { return true; }
        }

        public bool VaryCacheByUser
        {
            get { return false; }
        }

        #endregion

		#region ISocket

		public void SetController(ISocketController controller)
		{
			_controller = controller;
			_controller.Clients.Received += Clients_Received;
		}

		void Clients_Received(object sender, MessageReceivedEventArgs e)
		{
			if (e.MessageName == "reportActivity") 
			{
				_lyncCommunicationService.ReportActivity();
			}
		}

		public string SocketName
		{
			get { return LyncSocketName; }
		}

		#endregion
        
		#region ITranslatablePlugin Members

		public Translation[] DefaultTranslations
		{
			get
			{
				var t = new Translation("en-us");
				t.Set("processing", "...");
				t.Set("recieving", "Recieving...");
				t.Set("away", "Away");
				t.Set("be_right_back", "Be Right Back");
				t.Set("busy", "Busy");
				t.Set("do_not_disturb", "Do Not Disturb");
				t.Set("idle_busy", "Idle Busy");
				t.Set("idle_online", "Idle Online");
				t.Set("offline", "Offline");
				t.Set("online", "Online");
				t.Set("offwork", "Off Work");

				return new[] { t };
			}
		}

		public void SetController(ITranslatablePluginController controller)
		{
			_translation = controller;
		}

		#endregion

		#region ICategorizedPlugin Members

		public string[] Categories
		{
			get { return new[] { "Translatable" }; }
		}

		#endregion

	}
}
