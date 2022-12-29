# Introduction
This is a self-hosted SMTP Relay agent. 
Typical usage is to install it onto a server, set up users and devices, set up mail gateways, and then point your internal services/servers to this agent for mail relay. It will also allow receiving of messages without trying to relay them.

# Getting Started
Compile the code using the latest version of Visual Studio. This will create the following files:
* Configuration.exe: this allows editing users, devices, mail gateways, etc. You can run this while the service is running, but not everything will take effect until the service is restarted.
* Database.dll: Contains the logic for the SQLite Database. 
* Model.dll: Contains model definitions for the service, configuration, and database components.
* WinService.exe: Windows service (and console application) that hosts the SMTP listener and sender components.
* System.Data.SQLite.dll: SQLite database engine.
* x64 and x86 folders: these contain the interop SQLite dlls.

Copy all these files to where you want to install the SMTP Relay agent. I suggest a location such as: c:\Program Files\SMTPRelay\

You can open WinService.exe and it will run as a console program. If you don't run it as admin the first time, you'll get a notice that it can't set up Event Log Source. This will prevent most logging from working correctly. Run as an admin to fix this automatically. 

Opening WinService.exe will create "C:\ProgramData\SMTPRelay\config.db" if it does not already exist. This is the config database that will also contain your mail items. 

You can then close WinService.exe (control + C, or close with the X in the top right corner), and open Configuration.exe
Send Queue tab:
  This is planned to show a list of mail items that are waiting to be sent, but there was a lot of overhead loading this so I didn't implement it yet.

Endpoints tab:
  This is all the local endpoints that SMTP Relay will listen on. You can create multiple ones to have TLS/SSL/Unencrypted variants, or to use for different hostnames and SSL certificates. 

Users tab:
  This is where you create the users that will be able to send or receive mail. You'll want to create the Mail Gateways first if your users need to be able to relay mail.

Devices tab:
  This is where you create devices that can send mail without needing to specify a username in their SMTP config, since they are "authenticated" using only their source IP/Hostname. You'll want to create the Mail Gateways first if your devices need to be able to relay mail.

Mail Gateways tab:
  This is where you specify the outbound SMTP server settings that mail will be relayed to. If you specify a sender override, it will replace the sender address with the specified sender, no matter which device or user actually originated the mail item. Useful if you want to aggregate mail from multiple users and have them all appear from a single source.
  

1.	Installation process
You can use either SC CREATE to add the service, or installutil.exe. Both requires an elevated command prompt.

For SC CREATE:

SC CREATE "SMTPRelayAgent" binpath="Path to WinService.exe"

For installutil:

Navigate to the path where installutil.exe is, for example "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\"

installutil.exe "c:\Program Files\SMTP Relay\WinService.exe"

2.	Software dependencies
Requires .NET framework 4.8

You can run WinService.exe either as a service or as command line program, whichever you prefer, but don't run it as both at the same time.

I Recommend you get SQLite Studio (https://github.com/pawelsalawa/sqlitestudio) and open the config.db file. Look in the System table and there are some parameters you can tweak. These will eventually be added to the configuration program, but that's still a TODO for now.

# Planned Features
1. Add system configuration options to a tab page in the configuration tool. (For now, these are configured directly in the database with SQLite Studio.)
2. Storing mailbox items in separate mailbox databases, and allowing to configure where to store those.
3. Add a simple mail client to allow viewing maildrop items in a mailbox.
4. Add ability to look up and connect to mail servers for mail relay without a gateway configured for a user/device. 
5. Allow POP/IMAP access to mailboxes.
6. Implement auth xoauth2 for SMTP connections (for example Microsoft 365).
