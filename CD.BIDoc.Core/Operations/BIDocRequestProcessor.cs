using CD.DLS.Model.Mssql;
using CD.DLS.API;
using CD.DLS.Common.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Operations
{
    abstract class BIDocRequestProcessor
    {
        protected readonly BIDocCore _core;

        protected BIDocRequestProcessor(BIDocCore core)
        {
            _core = core;
        }

        public abstract bool CanProcess(DLSApiMessage request);
        public abstract ProcessingResult ProcessRequest(DLSApiMessage request, ProjectConfig projectConfig);


        protected Dictionary<int, MssqlModelElement> DeserializeElementsForDisplay(Guid projectConfigId, List<int> elementIds)
        {
            Dictionary<int, MssqlModelElement> modelElementsById = new Dictionary<int, MssqlModelElement>();

            Serialization.BIDocModelStored modelStored = new Serialization.BIDocModelStored(projectConfigId, elementIds);
            _core.Log.Important("Deserializing elements");
            Serialization.FromBIDocModelConverter converterFrom = new Serialization.FromBIDocModelConverter(modelStored);
            Interfaces.IReflectionHelper reflection = new Serialization.JsonReflectionHelper(new Model.Mssql.ModelActivator());
            ModelConverter converterTo = new ModelConverter(reflection);
            List<MssqlModelElement> convertedElements = converterFrom.ConvertFlat(converterTo).ToList();

            // selectively deserialize the elements for which the entire element tree is needed (SSIS packages for visualisation, ...)
            int convertedCount = convertedElements.Count;
            for (int i = 0; i < convertedCount; i++)
            {
                var convertedElement = convertedElements[i];
                if (convertedElement is Model.Mssql.Ssis.PackageElement || convertedElement is Model.Mssql.Ssrs.ReportElement)
                {
                    Serialization.BIDocModelStored deepModelStored = new Serialization.BIDocModelStored(projectConfigId, convertedElement.RefPath.Path);
                    _core.Log.Important("Deserializing details of " + convertedElement.Caption);
                    Serialization.FromBIDocModelConverter deepConverterFrom = new Serialization.FromBIDocModelConverter(deepModelStored);
                    MssqlModelElement deepConverted = deepConverterFrom.Convert(converterTo, convertedElement.Id);
                    convertedElements[i] = deepConverted;
                }

            }

            modelElementsById = convertedElements.ToDictionary(x => x.Id, x => x);

            return modelElementsById;

        }
    }

    abstract class BIDocRequestProcessor<TRequest> : BIDocRequestProcessor
        where TRequest : DLSApiMessage
    {
        public BIDocRequestProcessor(BIDocCore core) :
            base(core)
        { }

        public override bool CanProcess(DLSApiMessage request)
        {
            return request is TRequest;
        }

        public override ProcessingResult ProcessRequest(DLSApiMessage request, ProjectConfig projectConfig)
        {
            return ProcessRequest((TRequest)request, projectConfig);
        }

        public abstract ProcessingResult ProcessRequest(TRequest request, ProjectConfig projectConfig);
    }
    class NullRequestProcessor : BIDocRequestProcessor
    {
        public NullRequestProcessor(BIDocCore core) : base(core)
        {
        }

        public override bool CanProcess(DLSApiMessage request)
        {
            return request == null;
        }
        public override ProcessingResult ProcessRequest(DLSApiMessage request, ProjectConfig projectConfig)
        {
            var attachments = new List<Attachment>();
            return new ProcessingResult() { Content = string.Empty, Attachments = attachments };
        }
        

    }
}
