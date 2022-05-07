using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Receiver;
using CD.DLS.RequestProcessor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Service
{
    public partial class DLS : InstallableServiceBase // ServiceBase
    {
        private Receiver _receiver;
        private MessageProcessor _processor;

        public DLS()
        {
            InitializeComponent();
            this.ServiceName = "DLS";
        }

        new internal void InstallService()
        {
            base.InstallService();
        }
        new internal void UninstallService()
        {
            base.UninstallService();
        }

        protected override void OnStart(string[] args)
        {
            ConfigManager.ApplicationClass = ApplicationClassEnum.Service;
            ConfigManager.Log.Important("Starting service");
            ConfigManager.Log.Important("Deployment mode: " + ConfigManager.DeploymentMode.ToString());
            var receiverId = ConfigManager.ServiceReceiverId;
            _receiver = new Receiver(receiverId, "DLS Service");
            _receiver.MessageReceived += this.Receiver_MessageReceived;
            _processor = new MessageProcessor(_receiver);
            base.OnStart(args);
        }

        protected override void OnStop()
        {
            ConfigManager.Log.Important("Stopping service");
            base.OnStop();
        }

        public void StartConsole()
        {
            OnStart(new string[] { });
            ConfigManager.Log.Important("Service Started, receiver " + _receiver.Id);
            Console.WriteLine("Press Esc to exit");
            bool quit = false;
            while (!quit)
            {
                var k = Console.ReadKey();
                quit = k.Key == ConsoleKey.Escape;
            }
            OnStop();
        }

        public void StopConsole()
        {
            OnStop();
        }

        private async void Receiver_MessageReceived(RequestMessage message)
        {
            await _processor.ProcessAsync(message);
        }
    }

    /*
     private Receiver _receiver;
        private MessageProcessor _processor;
        public CDFramework()
        {
            InitializeComponent();
        }

        public void StartConsole()
        {
            OnStart(new string[] { });
            ConfigManager.Log.Important("Service Started, receiver " + _receiver.Id);
            Console.WriteLine("Press Esc to exit");
            bool quit = false;
            while (!quit)
            {
                var k = Console.ReadKey();
                quit = k.Key == ConsoleKey.Escape;
            }
            OnStop();
        }
        public void StopConsole()
        {
            OnStop();
        }

        protected override void OnStart(string[] args)
        {
            ConfigManager.ApplicationClass = ApplicationClassEnum.Service;
            ConfigManager.Log.Important("Starting service");
            ConfigManager.Log.Important("Deployment mode: " + ConfigManager.DeploymentMode.ToString());
            var receiverId = ConfigManager.ServiceReceiverId;
            _receiver = new Receiver(receiverId, "CDFramework Service");
            _receiver.MessageReceived += this.Receiver_MessageReceived;
            _processor = new MessageProcessor(_receiver);
            base.OnStart(args);
        }

        private async void Receiver_MessageReceived(RequestMessage message)
        {

            await _processor.ProcessAsync(message);
        }
        

        protected override void OnStop()
        {
            ConfigManager.Log.Important("Stopping service");
            base.OnStop();
        }
     */
}
