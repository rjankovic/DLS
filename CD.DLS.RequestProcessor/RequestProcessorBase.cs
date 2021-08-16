using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Receiver;
using CD.DLS.DAL.Security;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.RequestProcessor
{
    public class RequestProcessorBase
    {
        private StageManager _stageManager;
        private AnnotationManager _annotationManager;
        private GraphManager _graphManager;
        private InspectManager _inspectManager;
        private ProjectConfigManager _projectConfigManager;
        private RequestManager _requestManager;
        private SearchManager _searchManager;
        private SecurityManager _securityManager;
        private LearningManager _learningManger;
        private string _customerCode;
        private Guid _requestId;
        //private IReceiver _receiver;

        public ILogger Log { get { return ConfigManager.Log; } }

        public StageManager StageManager { get { return _stageManager; } }
        public AnnotationManager AnnotationManager { get { return _annotationManager; } }
        public GraphManager GraphManager { get { return _graphManager; } }
        public InspectManager InspectManager { get { return _inspectManager; } }
        public ProjectConfigManager ProjectConfigManager { get { return _projectConfigManager; } }
        public RequestManager RequestManager { get { return _requestManager; } }
        public SearchManager SearchManager { get { return _searchManager; } }
        public SecurityManager SecurityManager { get { return _securityManager; } }
        public LearningManager LearningManager { get { return _learningManger; } }
        public Guid RequestId { get { return _requestId; } }
        public string CustomerCode { get { return _customerCode; } }

        public RequestProcessorBase()
        {
        }

        public void Init(RequestMessage message)
        {
            _customerCode = message.CustomerCode;
            _requestId = message.RequestId;
            NetBridge nb;
            nb = new NetBridge(true);
            if (ConfigManager.DeploymentMode == DeploymentModeEnum.Azure || ConfigManager.ApplicationClass == ApplicationClassEnum.Service)
            {
                nb.SetConnectionString(ConfigManager.GetCustomerDatabaseConnectionString(_customerCode));
            }

            _stageManager = new StageManager(nb);
            _annotationManager = new AnnotationManager(nb);
            _graphManager = new GraphManager(nb);
            _inspectManager = new InspectManager(nb);
            _projectConfigManager = new ProjectConfigManager(nb);
            _requestManager = new RequestManager(nb);
            _searchManager = new SearchManager(nb);
            _searchManager = new SearchManager(nb);
            _securityManager = new SecurityManager(nb);
            _learningManger = new LearningManager(nb);
        }

        protected Dictionary<int, MssqlModelElement> DeserializeElementsForDisplay(Guid projectConfigId, List<int> elementIds)
        {
            Dictionary<int, MssqlModelElement> modelElementsById = new Dictionary<int, MssqlModelElement>();

            BIDocModelStored modelStored = new Serialization.BIDocModelStored(projectConfigId, elementIds, GraphManager);
            Log.Important("Deserializing elements");
            Serialization.FromBIDocModelConverter converterFrom = new Serialization.FromBIDocModelConverter(modelStored);
            IReflectionHelper reflection = new Serialization.JsonReflectionHelper(new Model.Mssql.ModelActivator());
            ModelConverter converterTo = new ModelConverter(reflection);
            List<MssqlModelElement> convertedElements = converterFrom.ConvertFlat(converterTo).ToList();

            // selectively deserialize the elements for which the entire element tree is needed (SSIS packages for visualisation, ...)
            int convertedCount = convertedElements.Count;
            for (int i = 0; i < convertedCount; i++)
            {
                var convertedElement = convertedElements[i];
                //convertedElement is 
                if (convertedElement is Model.Mssql.Ssis.PackageElement || convertedElement is Model.Mssql.Ssrs.ReportElement)
                {
                    Serialization.BIDocModelStored deepModelStored = new Serialization.BIDocModelStored(projectConfigId, convertedElement.Id, BIDocModelStored.LoadMethodEnum.SecondLevelAncestor, GraphManager);
                    Log.Important("Deserializing details of " + convertedElement.Caption);
                    Serialization.FromBIDocModelConverter deepConverterFrom = new Serialization.FromBIDocModelConverter(deepModelStored);
                    MssqlModelElement deepConverted = deepConverterFrom.Convert(converterTo, convertedElement.Id);
                    Log.Important("Done deserializing " + convertedElement.Caption);

                    convertedElements[i] = deepConverted;
                }

            }

            modelElementsById = convertedElements.ToDictionary(x => x.Id, x => x);

            return modelElementsById;
        }

        protected void LogException(Exception e)
        {
            ConfigManager.Log.Error(e.Message);
            if (e.InnerException != null)
            {
                ConfigManager.Log.Error(e.InnerException.Message);
            }
            ConfigManager.Log.Error(e.StackTrace);
            ConfigManager.Log.FlushMessages();
        }
    }
}
