namespace Takenet.MessagingHub.ServiceActivator
{
    partial class ProjectInstaller
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.activatorServiceProcessInstaller = new System.ServiceProcess.ServiceProcessInstaller();
            this.activatorServiceInstaller = new System.ServiceProcess.ServiceInstaller();
            // 
            // activatorServiceProcessInstaller
            // 
            this.activatorServiceProcessInstaller.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
            this.activatorServiceProcessInstaller.Password = null;
            this.activatorServiceProcessInstaller.Username = null;
            // 
            // activatorServiceInstaller
            // 
            this.activatorServiceInstaller.Description = "Dynamically activates Messaging Hub application packages";
            this.activatorServiceInstaller.DisplayName = "Messaging Hub Application Activator";
            this.activatorServiceInstaller.ServiceName = "MessagingHubActivatorService";
            this.activatorServiceInstaller.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
            // 
            // ProjectInstaller
            // 
            this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.activatorServiceProcessInstaller,
            this.activatorServiceInstaller});

        }

        #endregion

        private System.ServiceProcess.ServiceProcessInstaller activatorServiceProcessInstaller;
        private System.ServiceProcess.ServiceInstaller activatorServiceInstaller;
    }
}