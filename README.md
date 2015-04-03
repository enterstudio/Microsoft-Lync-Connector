# Microsoft-Lync-Connector

Version 3.0
Compatible with Telligent Evolution 8.5

#### Dependencies

The Microsoft Lync Connector requires Lync Server 2013. A Trusted Application needs to be setup to allow the connector to community with Lync.

**Lync 2013**
- Microsoft.Rtc.Collaboration.dll

**Zimbra Social**
- Zimbra Social (free or commercial) 8.5 or higher

#### Setup Trusted Application

```powershell

PS > Get-CsSite

Identity                  : Site:LyncLab
SiteId                    : 1
Services                  : {UserServer:daldevutil01.dev.telligent.com,
                            Registrar:daldevutil01.dev.telligent.com,
                            UserDatabase:daldevutil01.dev.telligent.com,
                            FileStore:daldevutil01.dev.telligent.com...}
Pools                     : {daldevutil01.dev.telligent.com}
FederationRoute           :
XmppFederationRoute       :
DefaultPersistentChatPool :
Description               : Lync Site
DisplayName               : LyncLab
SiteType                  : CentralSite
ParentSite                :

PS > New-CsTrustedApplicationPool -id daldevutil01.dev.telligent.com -Registrar Registrar:daldevutil01.dev.telligent.com -site Site:LyncLab
PS > New-CsTrustedApplication -ApplicationId community -TrustedApplicationPoolFqdn daldevutil01.dev.telligent.com  -Port 7489
PS > Enable-CsTopology

```

Get Thumbpring : Get-CsCertificate