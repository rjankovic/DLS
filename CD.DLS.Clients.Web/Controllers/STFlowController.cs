using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.Clients.Web.Models;
using CD.DLS.Clients.Web.Models.Diagram;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using CD.DLS.DAL.Receiver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class STFlowController : BaseController
    {
        // var mapSpec = { sourceRootId: sourceRootId, targetRootId: targetRootId, sourceElementType: sourceElementType, targetElementType: targetElementType };

        public class LineageMapSpec
        {
            public int sourceRootId { get; set; }
            public int targetRootId { get; set; }
            public string sourceElementType { get; set; }
            public string targetElementType { get; set; }
        }

        // GET: STFlow
        public ActionResult Default()
        {
            var guid = Guid.NewGuid().ToString();
            return PartialView("Default", guid);
        }

        public ActionResult GetSolutionTree()
        {
            /*
             [
       { "id" : "ajson1", "parent" : "#", "text" : "Simple root node" },
       { "id" : "ajson2", "parent" : "#", "text" : "Root node 2" },
       { "id" : "ajson3", "parent" : "ajson2", "text" : "Child 1" },
       { "id" : "ajson4", "parent" : "ajson2", "text" : "Child 2" },
    ]
             */
            var inspectManager = new InspectManager(NetBridge);
            var highLevelTree = inspectManager.GetHighLevelSolutionTree(ProjectConfig.ProjectConfigId);
            var res = highLevelTree.Select(x => new
            {
                id = x.ModelElementId.ToString(),
                parent = x.ParentElementId > 0 ? x.ParentElementId.ToString() : "#",
                text = "[" + x.TypeDescription + "] " + x.Caption
            }).ToArray();

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Argument 1 = source model element ID, Argument 2 = target model element ID
        /// </summary>
        /// <param name="argument1"></param>
        /// <param name="argument2"></param>
        public ActionResult TypeSelection(string argument1, string argument2)
        {
            var sourceElementId = int.Parse(argument1);
            var targetElementId = int.Parse(argument2);

            var inspectManager = new InspectManager(NetBridge);
            var graphManager = new GraphManager(NetBridge);

            var sourceTypes = inspectManager.GetHighLevelTypesUnderElement(ProjectConfig.ProjectConfigId, sourceElementId);
            var targetTypes = inspectManager.GetHighLevelTypesUnderElement(ProjectConfig.ProjectConfigId, targetElementId);

            var sourceNodeDescriptivePath = graphManager.GetModelElementDescriptivePath(sourceElementId);
            var targetNodeDescriptivePath = graphManager.GetModelElementDescriptivePath(targetElementId);

            var model = new SourceTargetFlowTypes()
            {
                SourceTypes = sourceTypes,
                TargetTypes = targetTypes,
                SourceDescriptivePath = sourceNodeDescriptivePath,
                TargetDescriptivePath = targetNodeDescriptivePath
            };

            return PartialView("TypeSelection", model);
        }

        public ActionResult LineageMap(string argument1)
        {
            var spec = JsonConvert.DeserializeObject<LineageMapSpec>(argument1);
            InspectManager inspectManager = new InspectManager(NetBridge);
            GraphManager graphManager = new GraphManager(NetBridge);

            var sourceRoot = graphManager.GetModelElementById(spec.sourceRootId);
            var targetRoot = graphManager.GetModelElementById(spec.targetRootId);
            var sourceNodeType = spec.sourceElementType.Substring(spec.sourceElementType.LastIndexOf('.') + 1);
            var targetNodeType = spec.targetElementType.Substring(spec.targetElementType.LastIndexOf('.') + 1);

            var dfMap = inspectManager.GetDataFlowBetweenGroupsFlat(ProjectConfig.ProjectConfigId,
            sourceRoot.RefPath, targetRoot.RefPath, sourceNodeType, targetNodeType).ToArray();

            var sourceDescPath = graphManager.GetModelElementDescriptivePath(spec.sourceRootId);
            var targetDescPath = graphManager.GetModelElementDescriptivePath(spec.targetRootId);
            var lineageMap = new LineageMap()
            {
                DataFlow = dfMap,
                SourceDescriptivePath = sourceDescPath,
                TargetDescriptivePath = targetDescPath
            };


            //var serializer = new JavaScriptSerializer();

            // For simplicity just use Int32's max value.
            // You could always read the value from the config section mentioned above.
            //serializer.MaxJsonLength = Int32.MaxValue;

            var json = JsonConvert.SerializeObject(lineageMap);
            var result = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return result;

            //return Json(lineageMap, JsonRequestBehavior.AllowGet);
        }

        public ActionResult FlowDetail(string argument1)
        {
            var flowDetailSpec = JsonConvert.DeserializeObject<SourceTargetFlowDetailSpec>(argument1);
            LineageDetailRequest request = new LineageDetailRequest { SourceRefPath = flowDetailSpec.SourceElementRefPath, TargetRefPath = flowDetailSpec.TargetElementRefPath, DetailLevel = flowDetailSpec.LineageDetailLevel };
            var response = AfService.PostRequest(request).Result;

            DiagramLoader loader = new DiagramLoader(NetBridge);
            var diagramFormed = loader.LoadDiagram(request, response);

            var json = JsonConvert.SerializeObject(diagramFormed);
            
            var result = new ContentResult
            {
                Content = json,
                ContentType = "application/json"
            };
            return result;
        }
    }
}