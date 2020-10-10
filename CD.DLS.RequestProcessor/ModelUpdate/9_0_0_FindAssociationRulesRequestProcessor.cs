using CD.DLS.API;
using CD.DLS.API.ModelUpdate;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using CD.DLS.Model.Mssql;
using CD.DLS.Model.Serialization;
using CD.DLS.Parse.Mssql.Ssas;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Ssrs;
using CD.DLS.Parse.Mssql.Db;
using System;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model;
using CD.DLS.Model.Business.Excel;
using CD.DLS.Model.Business.Organization;
using CD.DLS.DAL.Objects.Learning;
using Accord.MachineLearning.Rules;

namespace CD.DLS.RequestProcessor.ModelUpdate
{
    public class FindAssociationRulesRequestProcessor : RequestProcessorBase, IRequestProcessor<FindAssociationRulesRequest, DLSApiProgressResponse>
    {
        public DLSApiProgressResponse Process(FindAssociationRulesRequest request, ProjectConfig projectConfig)
        {
            try
            {
                /*
                LearningManager.CollectOlapFieldReferences(projectConfig.ProjectConfigId);

                var queryFields = LearningManager.GetOlapQueryFields(projectConfig.ProjectConfigId);
                var olapFields = LearningManager.ListOlapFields(projectConfig.ProjectConfigId);

                OlapRuleSet ruleSet = new OlapRuleSet();
                ruleSet.ProjectConfigId = projectConfig.ProjectConfigId;
                ruleSet.Fields = olapFields;

                Dictionary<int, OlapField> fieldsById = olapFields.ToDictionary(x => x.OlapFieldId, x => x);

                // rules can be established at once for all the cubes - the field sets are independent
                var sets = queryFields.GroupBy(x => x.QueryElementId).Select(
                g =>
                    new {
                        QueryElementId = g.Key,
                        Fields = new SortedSet<int>(g.Select(f => f.OlapFieldId))
                    }
                    );

                
                var inputCount = sets.Count();
                SortedSet<int>[] dataset = sets.Where(x => x.Fields.Count > 1).Select(x => x.Fields).ToArray();

                var threshold = Math.Min(dataset.Count() / 18, 15);
                Apriori apriori = new Apriori(threshold: threshold, confidence: 0.8);

                // Use the algorithm to learn a set matcher
                AssociationRuleMatcher<int> classifier = apriori.Learn(dataset);

                ruleSet.Rules = new List<OlapRule>();
                int ruleCount = 0;

                var limit = 1000;

                foreach (var aprioriRule in classifier.Rules)
                {
                    ruleCount++;
                    if (ruleCount > limit)
                    {
                        break;
                    }
                    OlapRule olapRule = new OlapRule();
                    olapRule.Confidence = aprioriRule.Confidence;
                    olapRule.Support = (double)aprioriRule.Support / inputCount;
                    olapRule.RuleCode = ruleCount.ToString();

                    olapRule.PremiseFields = aprioriRule.X.Select(x => fieldsById[x]).ToList();
                    olapRule.ConclusionFields = aprioriRule.Y.Select(x => fieldsById[x]).ToList();

                    olapRule.ServerName = fieldsById[aprioriRule.Y.First()].ServerName;
                    olapRule.DbName = fieldsById[aprioriRule.Y.First()].DbName;
                    olapRule.CubeName = fieldsById[aprioriRule.Y.First()].CubeName;

                    ruleSet.Rules.Add(olapRule);
                }

                LearningManager.SaveOlapRules(ruleSet);
                */
                return new DLSApiProgressResponse()
                {
                    ContinueWith = new SetModelAvailableRequest() //BuildAggregationsRequest()
                    {
                    }
                };
            }
            catch (Exception ex)
            {
                LogException(ex);
                throw ex;
            }
        }
    }
}
