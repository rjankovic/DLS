using System;
using System.Threading.Tasks;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CD.DLS.Tests.AnnotationsTests
{
    [TestClass]
    public class BusinessDictionaryUnitTests
    {
        [TestMethod]
        public void LoadCPI_50Users()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=cpicustdb;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            AnnotationManager annotationManager = new AnnotationManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            Guid projectConfigId = new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB");
            var tasks = new Task[50];
            for (int i = 0; i < 50; i++)
            {
                tasks[i] = Task.Factory.StartNew(() =>
                {
                    var index = new DAL.Objects.BusinessDictionaryIndex.BusinessDictionaryIndex(annotationManager, projectConfigId);
                });
            }

            Task.WaitAll(tasks);
        }
    }
}
