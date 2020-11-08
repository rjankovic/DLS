using System;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CD.DLS.Tests.MessagingTests
{
    [TestClass]
    public class OnPremisesMessageTests
    {
        const string receiver1Id = "74144F5D-CF3D-4225-9320-413E762D2649";
        const string receiver2Id = "337F7CA0-7F59-48C7-BD63-4346DB7B1962";
        const string projectId = "8EB6935F-82B5-4E22-B42C-2E200B4821A2";
        private static DAL.Receiver.Receiver _r1;
        private static DAL.Receiver.Receiver _r2;
        private static TestContext _tc;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            ConfigManager.SetCustomConfigManager(new StandardConfigManager());
            ManualConfigManager mcm = new ManualConfigManager();
            ConfigManager.SetCustomConfigManager(mcm);
            mcm.DeploymentMode = DeploymentModeEnum.OnPremises;
            mcm.QueueMode = QueueModeEnum.ServiceBroker;
            mcm.CustomerDatabaseConnectionString = "Data Source=localhost;Initial Catalog=DLS;Integrated Security=True;Pooling=False";
            mcm.Log = new DbLogger();
            //mcm.CustomerCode = "DEFAULT";

            
            var cs = ConfigManager.CustomerDatabaseConnectionString;
            var nb = new NetBridge();
            nb.SetConnectionString(cs);
            
            _r1 = new DAL.Receiver.Receiver(new Guid(receiver1Id), "R1", null, null, new DAL.Managers.RequestManager(nb));
            _r2 = new DAL.Receiver.Receiver(new Guid(receiver2Id), "R2", null, null, new DAL.Managers.RequestManager(nb));
            _tc = tc;
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
        }

        [TestMethod]
        public void SendAndReceive()
        {
            var msg = DAL.Receiver.Helpers.CreateRequest(_r1, Guid.Empty);
            msg.MessageToObjectId = _r2.Id;

            var respTask = _r1.PostMessage(msg);
            var resp = DAL.Receiver.Helpers.CreateResponse(msg);
            _r2.PostMessageNoResponse(resp).Wait();

            var receivedResp = respTask.Result;
            Assert.IsTrue(resp.MessageId == receivedResp.MessageId);
        }


    }
}
