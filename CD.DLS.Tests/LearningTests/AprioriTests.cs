using System;
using System.Collections.Generic;
using System.Linq;
using Accord.IO;
using Accord.MachineLearning.Rules;
using CD.DLS.DAL.Engine;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Objects.Learning;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CD.DLS.Tests.LearningTests
{
    [TestClass]
    public class AprioriTests
    {
        [TestMethod]
        public void AssocTest1()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            LearningManager lm = new LearningManager(nb);
            Guid projectConfigId = new Guid("55880448-DA67-4A80-BF5E-0AC42E797D15");

            lm.GetOlapQueryFields(projectConfigId);

            SortedSet<int>[] dataset =
{
    // Each row represents a set of items that have been bought 
    // together. Each number is a SKU identifier for a product.
    new SortedSet<int> { 1, 2, 3, 4 }, // bought 4 items
    new SortedSet<int> { 1, 2, 4 },    // bought 3 items
    new SortedSet<int> { 1, 2 },       // bought 2 items
    new SortedSet<int> { 2, 3, 4 },    // ...
    new SortedSet<int> { 2, 3 },
    new SortedSet<int> { 3, 4 },
    new SortedSet<int> { 2, 4 },
};

            // We will use Apriori to determine the frequent item sets of this database.
            // To do this, we will say that an item set is frequent if it appears in at 
            // least 3 transactions of the database: the value 3 is the support threshold.

            // Create a new a-priori learning algorithm with support 3
            Apriori apriori = new Apriori(threshold: 3, confidence: 0);

            // Use the algorithm to learn a set matcher
            AssociationRuleMatcher<int> classifier = apriori.Learn(dataset);

            // Use the classifier to find orders that are similar to 
            // orders where clients have bought items 1 and 2 together:
            int[][] matches = classifier.Decide(new[] { 1, 2 });

            // The result should be:
            // 
            //   new int[][]
            //   {
            //       new int[] { 4 },
            //       new int[] { 3 }
            //   };

            // Meaning the most likely product to go alongside the products
            // being bought is item 4, and the second most likely is item 3.

            // We can also obtain the association rules from frequent itemsets:

            
            AssociationRule<int>[] rules = classifier.Rules;
        }

        [TestMethod]
        public void AssocTest2()
        {
            NetBridge nb = new NetBridge(true, false);
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            LearningManager lm = new LearningManager(nb);
            Guid projectConfigId = new Guid("55880448-DA67-4A80-BF5E-0AC42E797D15");

            var queryFields = lm.GetOlapQueryFields(projectConfigId);

            var sets = queryFields.GroupBy(x => x.QueryElementId).Select(
                g => 
                new {
                    QueryElementId = g.Key,
                    Fields = new SortedSet<int>(g.Select(f => f.OlapFieldId))
                }
                );

            SortedSet<int>[] dataset = sets.Where(x => x.Fields.Count > 1).Select(x => x.Fields).ToArray();

            /*
{
// Each row represents a set of items that have been bought 
// together. Each number is a SKU identifier for a product.
new SortedSet<int> { 1, 2, 3, 4 }, // bought 4 items
new SortedSet<int> { 1, 2, 4 },    // bought 3 items
new SortedSet<int> { 1, 2 },       // bought 2 items
new SortedSet<int> { 2, 3, 4 },    // ...
new SortedSet<int> { 2, 3 },
new SortedSet<int> { 3, 4 },
new SortedSet<int> { 2, 4 },
};
*/
            // We will use Apriori to determine the frequent item sets of this database.
            // To do this, we will say that an item set is frequent if it appears in at 
            // least 3 transactions of the database: the value 3 is the support threshold.

            // Create a new a-priori learning algorithm with support 3

            // support 9%
            var threshold = dataset.Count() / 11;
            Apriori apriori = new Apriori(threshold: threshold, confidence: 0.8);

            // Use the algorithm to learn a set matcher
            AssociationRuleMatcher<int> classifier = apriori.Learn(dataset);

            //Serializer.Save<AssociationRuleMatcher<int>>(compression: SerializerCompression.None, obj: classifier);

            // Use the classifier to find orders that are similar to 
            // orders where clients have bought items 1 and 2 together:
            //int[][] matches = classifier.Decide(new[] { 1, 2 });

            // The result should be:
            // 
            //   new int[][]
            //   {
            //       new int[] { 4 },
            //       new int[] { 3 }
            //   };

            // Meaning the most likely product to go alongside the products
            // being bought is item 4, and the second most likely is item 3.

            // We can also obtain the association rules from frequent itemsets:
            AssociationRule<int>[] rules = classifier.Rules;
            
        }

        [TestMethod]
        public void AssocFullTest()
        {
            NetBridge nb = new NetBridge(true, false);
            //nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=samplecustdb;MultipleActiveResultSets=True");
            nb.SetConnectionString("Server=tcp:dls.database.windows.net;Database=dls;Uid=dls;Pwd=Lineage190;Encrypt=yes;Initial Catalog=cpicustdb;MultipleActiveResultSets=True");
            GraphManager graphManager = new GraphManager(nb);
            ProjectConfigManager pcm = new ProjectConfigManager(nb);
            LearningManager lm = new LearningManager(nb);
            Guid projectConfigId = new Guid("FD1312FC-1182-4B9C-82D9-2089F3468BFB");

            lm.CollectOlapFieldReferences(projectConfigId);
            
            var queryFields = lm.GetOlapQueryFields(projectConfigId);
            var olapFields = lm.ListOlapFields(projectConfigId);

            OlapRuleSet ruleSet = new OlapRuleSet();
            ruleSet.ProjectConfigId = projectConfigId;
            ruleSet.Fields = olapFields;

            Dictionary<int, OlapField> fieldsById = olapFields.ToDictionary(x => x.OlapFieldId, x => x);
            
            var sets = queryFields.GroupBy(x => x.QueryElementId).Select(
                g =>
                new {
                    QueryElementId = g.Key,
                    Fields = new SortedSet<int>(g.Select(f => f.OlapFieldId))
                }
                );

            var inputCount = sets.Count();
            SortedSet<int>[] dataset = sets.Where(x => x.Fields.Count > 1).Select(x => x.Fields).ToArray();
            
            var threshold = Math.Min(dataset.Count() / 10, 15);
            Apriori apriori = new Apriori(threshold: threshold, confidence: 0.8);
            
            // Use the algorithm to learn a set matcher
            AssociationRuleMatcher<int> classifier = apriori.Learn(dataset);
            
            ruleSet.Rules = new List<OlapRule>();
            int ruleCount = 0;
            var xDist = classifier.Rules.SelectMany(x => x.X).Distinct().ToList();
            var yDist = classifier.Rules.SelectMany(x => x.Y).Distinct().ToList();

            var limit = 1000;

            foreach (var aprioriRule in classifier.Rules.OrderByDescending(x => x.Support))
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

            var presavePremises = ruleSet.Rules.SelectMany(x => x.PremiseFields).Distinct().ToList();

            lm.SaveOlapRules(ruleSet);

            var loadedRules = lm.LoadOlapRules(projectConfigId);

            var postsavePremises = loadedRules.Rules.SelectMany(x => x.PremiseFields).Distinct().ToList();

            var postsaveConclusions = loadedRules.Rules.SelectMany(x => x.ConclusionFields).Distinct().ToList();
        }
    }
}
