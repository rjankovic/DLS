using CD.DLS.DAL.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CD.DLS.DAL.Objects.Extract;

namespace CD.DLS.DAL.Managers
{
    public class StageManager
    {
        
        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public StageManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public StageManager()
        {
            _netBridge = new NetBridge();
        }

        public void SaveExtractItem(ExtractObject extractItem, Guid extractId, int componentId)
        {
            var content = extractItem.Serialize();

            /*
    CREATE PROCEDURE [Stg].[sp_SaveExtractItem]
	@ExtractId	UNIQUEIDENTIFIER,
	@ComponentId INT,
	@ObjectType NVARCHAR(MAX),
	@ObjectName NVARCHAR(MAX),
	@Content NVARCHAR(MAX)
AS
             */
            NetBridge.ExecuteProcedure("[Stg].[sp_SaveExtractItem]", new Dictionary<string, object>()
            {
                { "ExtractId", extractId },
                { "ComponentId", componentId },
                { "ObjectType", extractItem.ExtractType.ToString() },
                { "ObjectName", extractItem.Name},
                { "Content", content }
            });
        }

        public ExtractObject GetExtractItem(int extractItemId)
        {
            var dt = NetBridge.ExecuteTableFunction("[Stg].[f_GetExtractItem]", new Dictionary<string, object>()
                {
                    { "ExtractItemId", extractItemId }
                });

            List<ExtractObject> res = new List<ExtractObject>();
            foreach (var obj in dt.AsEnumerable())
            {
                var deser = ExtractObject.Deserialize(obj["Content"] as string);
                deser.ExtractItemId = (int)obj["ExtractItemId"];
                res.Add(deser);
            }

            if (res.Count != 1)
            {
                throw new Exception();
            }
            return res.First();
        }

        public List<ExtractObject> GetExtractItems(Guid extractId, int componentId,
    ExtractTypeEnum extractType/*, bool includeDefinitions = true*/)
        {
            /*
CREATE FUNCTION [Stg].[f_GetExtractItems]
(
	@ExtractId	UNIQUEIDENTIFIER,
	@ComponentId INT,
	@ObjectType NVARCHAR(200)
)
             */

            var spName = "[Stg].[f_GetExtractItems]";
            //if (!includeDefinitions)
            //{
            //    spName = "[Stg].[f_GetExtractItemsNoDef]";
            //}

            var dt = NetBridge.ExecuteTableFunction(spName, new Dictionary<string, object>()
                {
                    { "ExtractId", extractId },
                    { "ComponentId", componentId },
                    { "ObjectType", extractType.ToString() }
                });

            List<ExtractObject> res = new List<ExtractObject>();
            foreach (var obj in dt.AsEnumerable())
            {
                var deser = ExtractObject.Deserialize(obj["Content"] as string);
                deser.ExtractItemId = (int)obj["ExtractItemId"];
                res.Add(deser);
            }

            return res;
        }

        public void CreateNewExtract(Manifest manifest)
        {
            /*
             CREATE PROCEDURE [Stg].[sp_SaveNewExtract]
	@extractId UNIQUEIDENTIFIER,
	@projectConfigId UNIQUEIDENTIFIER,
	@extractedBy NVARCHAR(MAX),
	@extractStartTime DATETIME
             */

            NetBridge.ExecuteProcedure("[Stg].[sp_SaveNewExtract]", new Dictionary<string, object>()
                {
                    { "extractId", manifest.ExtractId },
                    { "projectConfigId", manifest.ProjectConfig.ProjectConfigId },
                    { "extractedBy", manifest.ExecutedBy },
                    { "extractStartTime", manifest.ExtractStart}
                });

        }
        

    }
}
