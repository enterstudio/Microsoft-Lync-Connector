# Microsoft Lync Connector

Version 3.0 compatible with Telligent Community 8.5

#### Dependencies

The Microsoft Lync Connector requires Lync Server 2013. A Trusted Application needs to be set up to allow the connector to communicate with Lync.

**Unified Communications Management API (UCMA) 4.0 SDK**
- [Microsoft.Rtc.Collaboration.dll](http://www.microsoft.com/en-us/download/details.aspx?id=35463)

**Telligent Community**
- Telligent Community (free or commercial) version 8.5 or higher
- Telligent.DynamicConfiguration.dll
- Telligent.Evolution.Api.dll
- Telligent.Evolution.Components.dll
- Telligent.Evolution.Core.dll
- Telligent.Evolution.Rest.dll

**Lync Connector**
- Telligent.Evolution.Extensions.Lync.dll

#### How to set up Trusted Application

```powershell

PS > Get-CsSite

Identity                  : Site:LyncLab
SiteId                    : 1
Services                  : {UserServer:lync.server.telligent.com,
                            Registrar:lync.server.telligent.com,
                            UserDatabase:lync.server.telligent.com,
                            FileStore:lync.server.telligent.com...}
Pools                     : {lync.server.telligent.com}
FederationRoute           :
XmppFederationRoute       :
DefaultPersistentChatPool :
Description               : Lync Site
DisplayName               : LyncLab
SiteType                  : CentralSite
ParentSite                :

PS > New-CsTrustedApplicationPool -id lync.server.telligent.com -Registrar Registrar:lync.server.telligent.com -site Site:LyncLab
PS > New-CsTrustedApplication -ApplicationId community -TrustedApplicationPoolFqdn lync.server.telligent.com  -Port 7489
PS > Enable-CsTopology

```

#### How to install the Microsoft Lync Connector

To install the Microsoft Lync Connector for Telligent Community, copy the following files to the Telligent Community server \bin folder:

- Telligent.Evolution.Extensions.Lync.dll
- Microsoft.Rtc.Collaboration.dll

The UCMA SDK library file is usually located in C:\Program Files\Microsoft UCMA 4.0\SDK\Core\Bin.

Once the files are installed in the Telligent Community \bin directory, the Lync Connector will be available as a plug-in within the Telligent Community Control Panel.

#### How to configure the Microsoft Lync Connector

In the Telligent Community Control Panel, navigate to the **Lync Integration** plugin and click Configure. Enter the required fields and click Save.

**Lync Host**

The fully qualified domain name (FQDN) of the server where Microsoft Lync Server 2013 is deployed.

**GRUU**

The trusted [Globally Routable User-Agent URI](http://blog.greenl.ee/2011/11/16/gruu/) (GRUU) for the application.

**Trusted Port**

Port given to the trusted application. This is 7489 from the above example.

**Application Port**

Application default port. ex: 5061

**Account SIP**

SIP for the service account.

**Certificate Thumbprint**

To get the Thumbprint, run the following from the Lync Powershell:

`PS > Get-CsCertificate` 

The OAuthTokenIssuer certificate needs to be exported from the Lync Server and imported into the Zimbra Social server(s).

Once imported the Application Pool account needs permission to the Lync Certificate.

- Click Start and type mmc
- Select Add/Remove Snap-in from the File menu
- Select Certificates
- Select Computer Account
- Navigate to the Personal certificates
- Right click the imported Lync certificate
- Select All Tasks > Manage Private Keys...
- Add the IIS Application Pool account running the community
- Click Ok

####Where is the documentation?
Please refer to the [wiki section](https://github.com/Telligent/Microsoft-Lync-Connector/wiki/) of this repository.

####How do I report a bug?
You can use the [issues section](https://github.com/Telligent/Microsoft-Lync-Connector/issues/) of this repository to report any issues.

####Where can I ask questions?
Please visit our [developer community](http://community.telligent.com/community/f/554) to ask questions, get answers, collaborate and connect with other developers. Plus, give us feedback there so we can continue to improve these tools for you.

####Can I contribute?
Yes, we will have more details soon on how you can contribute.
