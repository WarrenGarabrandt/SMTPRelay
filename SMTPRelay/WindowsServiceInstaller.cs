﻿/*
using System;
using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace SMTPRelay
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        /// <summary>
        /// Public Constructor for WindowsServiceInstaller.
        /// - Put all of your Initialization code here.
        /// </summary>
        public WindowsServiceInstaller()
        {
            ServiceProcessInstaller serviceProcessInstaller =
                               new ServiceProcessInstaller();
            ServiceInstaller serviceInstaller = new ServiceInstaller();

            //# Service Account Information
            serviceProcessInstaller.Account = ServiceAccount.LocalSystem;
            serviceProcessInstaller.Username = null;
            serviceProcessInstaller.Password = null;

            //# Service Information
            serviceInstaller.DisplayName = "SMTP Relay Agent";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.Description = "SMTP Relay Agent.";

            //# This must be identical to the WindowsService.ServiceBase name
            //# set in the constructor of WindowsService.cs
            serviceInstaller.ServiceName = "SMTPRelay";

            this.Installers.Add(serviceProcessInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}

*/