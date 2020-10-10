using CD.DLS.API;
using CD.DLS.API.Query;
using CD.DLS.Clients.Web.Models;
using CD.DLS.Clients.Web.Models.TechView;
using CD.DLS.DAL.Managers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CD.DLS.Clients.Web.Controllers
{
    public class ElementViewController : BaseController
    {
        // GET: ElementView
        public ActionResult Index(string argument1)
        {
            var nodeId = int.Parse(argument1);
            ElementView elementView = CreateElementView(nodeId);
            return PartialView("Index", elementView);
        }

        public ActionResult JsonFormat(string argument1)
        {
            var nodeId = int.Parse(argument1);
            ElementView elementView = CreateElementView(nodeId);

            //convert Enums to Strings (instead of Integer)
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });

            return Json(elementView, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TechView(string argument1)
        {
            var elementId = int.Parse(argument1);

            var techViewRequest = new ElementTechViewRequest() { ElementId = elementId };
            var response = AfService.PostRequest(techViewRequest).Result;

            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new StringEnumConverter { CamelCaseText = true });
                return settings;
            });
            
            if (response.NodeDescription is VisualPartNodeDescription)
            {
                var renderer = new ElementTechViewRenderer();
                var rendered = renderer.Render(response.NodeDescription as VisualPartNodeDescription, response.VisualAncestor);
                return Json(rendered, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var rendered = new ElementTechViewVisual() { VisualType = VisualTypeEnum.None.ToString() };
                return Json(rendered, JsonRequestBehavior.AllowGet);
            }
        }

        public void SaveBusinessDictionary(string argument1)
        {
            var data = JsonConvert.DeserializeObject<ElementDictionaryEntryCollection>(argument1);
            var annotationManager = new AnnotationManager(NetBridge);
            var linksFrom = annotationManager.ListLinksFrom(ProjectConfig.ProjectConfigId, data.ElementId);
            List<AnnotationViewFieldValue> fieldValues = data.FieldValues
                .Select(x => new AnnotationViewFieldValue()
                { FieldId = x.FieldId, ModelElementId = data.ElementId, Value = x.FieldValue })
                .ToList();
            annotationManager.UpdateElementFields(fieldValues, ProjectConfig.ProjectConfigId, UserData.UserId, linksFrom, new List<int>() { data.ElementId });
            return;
        }

        private ElementView CreateElementView(int nodeId)
        {
            var graphManager = new GraphManager(NetBridge);
            var annotationManager = new AnnotationManager(NetBridge);
            var element = graphManager.GetModelElementByNodeId(nodeId);

            var views = annotationManager.ListProjectViews(ProjectConfig.ProjectConfigId);
            var viewId = views.First(x => x.ViewName == "Default").AnnotationViewId;
            var typeView = views.FirstOrDefault(x => x.ViewName == "Type_" + element.Type);
            if (typeView != null)
            {
                viewId = typeView.AnnotationViewId;
            }

            var businessViewFields = annotationManager.ListViewFields(viewId).OrderBy(x => x.FieldOrder).ToList();
            var businessFieldValues = annotationManager.GetViewFieldValues(viewId, element.Id);

            string refPathSplit = element.RefPath.ToString();
            refPathSplit = refPathSplit.Replace("[@Name='", ": ");
            refPathSplit = refPathSplit.Replace("']/", "" + System.Environment.NewLine);
            refPathSplit = refPathSplit.Replace("']", "");
            var refPathArray = refPathSplit
                .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            
            var res = new ElementView
            {
                RefPath = element.RefPath,
                TechViewType = ElementTechViewTypeEnum.None,
                ElementId = element.Id,
                ElementName = element.Caption,
                ElementViewId = Guid.NewGuid().ToString(),
                BusinessDictionary = new List<ElementBusinessDictionaryEntry>(),
                RefPathParts = refPathArray
            };

            foreach (var field in businessViewFields)
            {
                string value = null;
                if (businessFieldValues.Any(x => x.FieldId == field.FieldId))
                {
                    value = businessFieldValues.First(x => x.FieldId == field.FieldId).Value;
                }
                res.BusinessDictionary.Add(new ElementBusinessDictionaryEntry()
                {
                    FieldId = field.FieldId,
                    FieldName = field.FieldName,
                    FieldValue = value
                });
            }

            return res;
        }
        
    }

}