using CD.DLS.Common.Interfaces;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects;
using CD.DLS.DAL.Objects.Learning;

namespace CD.DLS.DAL.Managers
{
    public class OlapCubeRuleFilter
    {
        public string ServerName { get; set; }
        public string DbName { get; set; }
        public string CubeName { get; set; }

        public List<OlapField> FilterFields(List<OlapField> allFields)
        {
            return allFields.Where(x =>
            x.ServerName.ToLower() == ServerName.ToLower()
            && x.DbName.ToLower() == DbName.ToLower()
            && x.CubeName.ToLower() == CubeName.ToLower()
            ).ToList();
        }

        public List<OlapRule> FilterRules(List<OlapRule> allRules)
        {
            return allRules.Where(x =>
            x.ServerName.ToLower() == ServerName.ToLower()
            && x.DbName.ToLower() == DbName.ToLower()
            && x.CubeName.ToLower() == CubeName.ToLower()
            ).ToList();
        }

        public bool FiltersEqual(OlapCubeRuleFilter other)
        {
            if (other == null)
            {
                return false;
            }

            return other.ServerName.ToLower() == this.ServerName.ToLower()
                && other.DbName.ToLower() == this.DbName.ToLower()
                && other.CubeName.ToLower() == this.CubeName.ToLower();
        }
    }

    public class LearningManager
    {

        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }

        public LearningManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public LearningManager()
        {
            _netBridge = new NetBridge();
        }

        public void CollectOlapFieldReferences(Guid projectId)
        {
            NetBridge.ExecuteProcedure("[Learning].[sp_CollectOlapFieldReferences]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });
        }

        public List<OlapQueryField> GetOlapQueryFields(Guid projectId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Learning].[sp_GetOlapQueryFields]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectId }
            });

            List<OlapQueryField> fields = ReadOlapQueryFields(dt);
            return fields;
        }

        private List<OlapQueryField> ReadOlapQueryFields(DataTable dt)
        {
            List<OlapQueryField> fields = new List<OlapQueryField>();

            foreach (DataRow dr in dt.Rows)
            {
                fields.Add(new OlapQueryField()
                {
                    OlapQueryFieldId = (int)dr["OlapQueryFieldId"],
                    QueryElementId = (int)dr["QueryElementId"],
                    OlapFieldId = (int)dr["OlapFieldId"]
                });
            }

            return fields;
        }

        public void SaveOlapRules(OlapRuleSet ruleSet)
        {
            /*
             CREATE PROCEDURE [Learning].[sp_SetOlapRules]
	@projectconfigid UNIQUEIDENTIFIER,
	@rules [Learning].[UDTT_OlapRules] READONLY
             */

            Guid projectConfigId = ruleSet.ProjectConfigId;
            DataTable rulesDt = new DataTable();

            rulesDt.Columns.Add("RuleCode");
            rulesDt.Columns.Add("Support");
            rulesDt.Columns.Add("Confidence");
            rulesDt.Columns.Add("Premises");
            rulesDt.Columns.Add("Conclusions");
            rulesDt.Columns.Add("ServerName");
            rulesDt.Columns.Add("DbName");
            rulesDt.Columns.Add("CubeName");
            
            rulesDt.TableName = "Learning.UDTT_OlapRules";

            foreach (var rule in ruleSet.Rules)
            {
                var nr = rulesDt.NewRow();

                nr["RuleCode"] = rule.RuleCode;
                nr["Support"] = rule.Support;
                nr["Confidence"] = rule.Confidence;
                nr["Premises"] = string.Join(";", rule.PremiseFields.Select(x => x.OlapFieldId));
                nr["Conclusions"] = string.Join(";", rule.ConclusionFields.Select(x => x.OlapFieldId));
                nr["ServerName"] = rule.ServerName;
                nr["DbName"] = rule.DbName;
                nr["CubeName"] = rule.CubeName;

                rulesDt.Rows.Add(nr);
            }

            NetBridge.ExecuteProcedure("[Learning].[sp_SetOlapRules]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId },
                { "rules", rulesDt }
            });
        }

        public List<OlapField> ListOlapFields(Guid projectConfigId)
        {
            List<OlapField> fieldList = new List<OlapField>();

            var fieldsDt = NetBridge.ExecuteProcedureTable("[Learning].[sp_ListOlapFields]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });


            foreach (DataRow fr in fieldsDt.Rows)
            {
                OlapField field = new OlapField()
                {
                    FieldElementId = (int)fr["FieldElementId"],
                    FieldName = (string)fr["FieldName"],
                    FieldReference = (string)fr["FieldReference"],
                    FieldType = (OlapFieldType)Enum.Parse(typeof(OlapFieldType), (string)fr["FieldType"]),
                    OlapFieldId = (int)fr["OlapFieldId"],
                    ServerName = (string)fr["ServerName"],
                    DbName = (string)fr["DbName"],
                    CubeName = (string)fr["CubeName"]
                };

                fieldList.Add(field);
            }

            return fieldList;
        }
        
        public OlapRuleSet LoadOlapRules(Guid projectConfigId, OlapCubeRuleFilter cubeFilter = null)
        {
            OlapRuleSet ruleSet = new OlapRuleSet();
            ruleSet.ProjectConfigId = projectConfigId;

            Dictionary<int, OlapField> fieldsById = new Dictionary<int, OlapField>();
            List<OlapField> fieldList = new List<OlapField>();

            fieldList = ListOlapFields(projectConfigId);
            if (cubeFilter != null)
            {
                fieldList = cubeFilter.FilterFields(fieldList);
            }
            fieldsById = fieldList.ToDictionary(x => x.OlapFieldId, x => x);

            //var fieldsDt = NetBridge.ExecuteProcedureTable("[Learning].[sp_ListOlapFields]", new Dictionary<string, object>()
            //{
            //    { "projectconfigid", projectConfigId}
            //});


            //foreach (DataRow fr in fieldsDt.Rows)
            //{
            //    OlapField field = new OlapField()
            //    {
            //        FieldElementId = (int)fr["FieldElementId"],
            //        FieldName = (string)fr["FieldName"],
            //        FieldReference = (string)fr["FieldReference"],
            //        FieldType = (OlapFieldType)Enum.Parse(typeof(OlapFieldType), (string)fr["FieldType"]),
            //        OlapFieldId = (int)fr["OlapFieldId"]
            //    };

            //    fieldList.Add(field);
            //    fieldsById[field.OlapFieldId] = field;
            //}

            ruleSet.Fields = fieldList;

            Dictionary<int, OlapRule> rulesById = new Dictionary<int, OlapRule>();
            ruleSet.Rules = new List<OlapRule>();

            var rulesDt = NetBridge.ExecuteProcedureTable("[Learning].[sp_GetOlapRules]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });

            List<OlapRule> ruleList = new List<OlapRule>();
            foreach (DataRow rr in rulesDt.Rows)
            {
                OlapRule rule = new OlapRule()
                {
                    ConclusionFields = new List<OlapField>(),
                    Confidence = (float)rr["Confidence"],
                    PremiseFields = new List<OlapField>(),
                    RuleCode = (string)rr["RuleCode"],
                    Support = (float)rr["Support"],
                    OlapRuleId = (int)rr["OlapRuleId"],
                    ServerName = (string)rr["ServerName"],
                    DbName = (string)rr["DbName"],
                    CubeName = (string)rr["CubeName"]
                };

                ruleList.Add(rule);
            }

            if (cubeFilter != null)
            {
                ruleList = cubeFilter.FilterRules(ruleList);
            }


            rulesById = ruleList.ToDictionary(x => x.OlapRuleId, x => x);

            var premisesDt = NetBridge.ExecuteProcedureTable("[Learning].[sp_GetOlapRulePremises]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });

            foreach (DataRow pr in premisesDt.Rows)
            {
                var ruleId = (int)pr["OlapRuleId"];
                if (!rulesById.ContainsKey(ruleId))
                {
                    continue;
                }
                var rule = rulesById[ruleId];
                var fieldId = (int)pr["OlapFieldId"];
                rule.PremiseFields.Add(fieldsById[fieldId]);
            }

            var conclusionsDt = NetBridge.ExecuteProcedureTable("[Learning].[sp_GetOlapRuleConclusions]", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });

            foreach (DataRow cn in conclusionsDt.Rows)
            {
                var ruleId = (int)cn["OlapRuleId"];
                if (!rulesById.ContainsKey(ruleId))
                {
                    continue;
                }
                var rule = rulesById[ruleId];
                var fieldId = (int)cn["OlapFieldId"];
                rule.ConclusionFields.Add(fieldsById[fieldId]);
            }

            ruleSet.Rules = rulesById.Values.ToList();
            return ruleSet;
        }

    }
}