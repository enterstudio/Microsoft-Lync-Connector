using System;
using System.Collections.Generic;
using System.Linq;
using Telligent.DynamicConfiguration.Components;
using Telligent.Evolution.Extensibility.Sockets.Version1;
using Telligent.Evolution.Extensibility.Version1;
using Telligent.Evolution.Extensions.Lync.RestApi;

namespace Telligent.Evolution.Extensions.Lync.Plugins
{
    public class Plugin : IPluginGroup, ICategorizedPlugin, ISocket, IConfigurablePlugin
    {
        public ISocketController SocketController;

		public string Name
		{
			get { return "Lync Integration"; }
		}

		public string Description
		{
			get { return "Contains all plugins defining the Lync Integration functionality."; }
		}

		public IEnumerable<Type> Plugins
		{
			get 
			{
				return new[] {
                    typeof(LyncRoutePlugin),
					typeof(UserStatusPlugin)
				};
			}
		}

		public void Initialize() { }

        public string[] Categories
        {
            get { return new[] { "Lync 2013" }; }
        }

        public void SetController(ISocketController controller)
        {
            SocketController = controller;
            SocketController.Clients.Received += (sender, e) =>
            {
                
            };
        }
        
        public string SocketName
        {
            get { return "lync"; }
        }

        public static Plugin LyncPlugin
        {
            get
            {
                return PluginManager.Get<Plugin>().FirstOrDefault();
            }
        }

        #region IConfigurablePlugin

        public IPluginConfiguration Configuration { get; private set; }

        public void Update(IPluginConfiguration configuration)
        {
            Configuration = configuration;
        }

        public PropertyGroup[] ConfigurationOptions
        {
            get
            {
                PropertyGroup[] groups = { new PropertyGroup("options", "Options", 0) };

                groups[0].Properties.Add(new Property("host", "Lync Host", PropertyType.String, 0, "") { DescriptionText = "The FQDN of the machine where Lync is deployed." });
                groups[0].Properties.Add(new Property("gruu", "GRUU", PropertyType.String, 0, "") { DescriptionText = "The trusted GRUU for the application." });
                groups[0].Properties.Add(new Property("trustedPort", "Trusted Port", PropertyType.Int, 0, "") { DescriptionText = "Port given to the trusted application." });
                groups[0].Properties.Add(new Property("appPort", "Application Port", PropertyType.Int, 0, "") { DescriptionText = "Application default port. ex: 5061" });
                groups[0].Properties.Add(new Property("accountSip", "Account SIP", PropertyType.String, 0, "") { DescriptionText = "SIP for the service account." });
                groups[0].Properties.Add(new Property("thumbprint", "Certificate Thumbprint", PropertyType.String, 0, ""));

                return groups;
            }
        }

        #endregion
    }
}
