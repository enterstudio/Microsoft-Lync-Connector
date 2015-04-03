# Microsoft Lync Connector

Version 3.0
Compatible with Zimbra Social 8.5

#### Dependencies

The Microsoft Lync Connector requires Lync Server 2013. A Trusted Application needs to be set up to allow the connector to communicate with Lync.

**Lync 2013**
- Microsoft.Rtc.Collaboration.dll

**Zimbra Social**
- Zimbra Social (free or commercial) 8.5 or higher

**Lync Connector**
- Telligent.Evolution.LyncIntegration.dll

#### Set up Trusted Application

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

#### Setup Lync Plugin

To install the plugin, copy the Telligent.Evolution.LyncIntegration.dll file to the Zimbra Social server bin folder.

In the control panel, navigate to the Lync Integration plugin and click Configure. Enter the required fields and save.

**Lync Host**

The FQDN of the machine where Lync is deployed.

**GRUU**

The trusted GRUU for the application.

**Trusted Port**

Port given to the trusted application. This is 7489 from the above example.

**Application Port**

Application default port. ex: 5061

**Account SIP**

SIP for the service account.

**Certificate Thumbprint**

To get the Thumbprint, run the following from the Lync Powershell:

`PS > Get-CsCertificate`. 

The certificate also needs to be exported from the Lync Server and imported into the Zimbra Social server(s).