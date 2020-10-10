using CD.DLS.Serialization;
using CD.DLS.Parse.Mssql;
using CD.DLS.Interfaces;
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
    class ExtractMetadataRequestProcessor : BIDocRequestProcessor<ExtractMetadataRequest>
    {
        private BIDocCore _core;

        public ExtractMetadataRequestProcessor(BIDocCore core) : base(core)
        {
            _core = core;
        }

        

        public override ProcessingResult ProcessRequest(ExtractMetadataRequest request, ProjectConfig projectConfig)
        {
            _core.IsBusy = true;
            try
            {
                MssqlModelElement model = MssqlModelExtractor.ParseAll(new ModelSettings() { Config = projectConfig, Log = _core.Log });
                /**/
                _core.Log.Important("Converting to DB format");
                //using (var dbContext = new CDFrameworkContext())
                //{
                BIDocModelBulk modelBulk = new BIDocModelBulk();
                ToBIDocModelConverter modelConverterTo = new ToBIDocModelConverter(modelBulk);
                ModelConverter modelConverterFrom = new ModelConverter(new JsonReflectionHelper(new ModelActivator()));
                modelConverterFrom.Convert(model, modelConverterTo);
                // !! per-component parser
                //modelConverterFrom.Convert(model, modelConverterTo, new Dictionary<MssqlModelElement, int>());

                _core.Log.Important("Persisting model");
                modelBulk.UpdateModel(projectConfig.ProjectConfigId);

                //dbContext.SaveChanges();
                //}
                /**/
                _core.IsBusy = false;
                return new ProcessingResult()
                {
                    Content = string.Empty,
                    Attachments = null
                };
            }
            catch
            {
                _core.IsBusy = false;
                throw;
            }
            
        }
    }
}
