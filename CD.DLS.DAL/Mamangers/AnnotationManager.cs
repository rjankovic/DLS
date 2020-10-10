using CD.DLS.DAL.Engine;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.DAL.Objects.Inspect;
using CD.DLS.DAL.Objects.BusinessDictionaryIndex;

namespace CD.DLS.DAL.Managers
{
    public class AddLinkEventArgs : EventArgs
    {
        public bool IsFrom { get; set; } //to = false, from = true
    }
    public delegate void AddLinkEventHandler(object sender, AddLinkEventArgs e);

    public class AnnotationViewField
    {
        public int AnnotationViewId { get; set; }
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public int FieldOrder { get; set; }
        public string AssociatedTypeName { get; set; }
        public string AssociatedTypeDescription { get; set; }
    }

    // SELECT vals.FieldValueId, vals.AnnotationElementId, vals.FieldId, e.ModelElementId, vals.Value
    public class AnnotationViewFieldValue
    {
        public int FieldValueId { get; set; }
        public int AnnotationElementId { get; set; }
        public int FieldId { get; set; }
        public int? ModelElementId { get; set; }
        public string Value { get; set; }
    }

    public class AnnotationField
    {
        public int FieldId { get; set; }
        public string FieldName { get; set; }
        public bool Deleted { get; set; }
        public bool UsedInViews { get; set; }
    }

    public class AnnotationLinkType
    {
        public int LinkTypeId { get; set; }
        public string LinkTypeName { get; set; }
        public bool Deleted { get; set; }
        public bool UsedInLinks { get; set; }
    }

    public class AnnotationLinkFromTo
    {
        public int LinkId { get; set; }
        public string LinkTypeName { get; set; }
        public int LinkTypeId { get; set; }
        public string ElementFromToCaption { get; set; }
        public string ElementFromToDescriptivePath { get; set; }
        public int ModelElementFromId { get; set; }
        public int ModelElementToId { get; set; }
        public int UpdatedVersion { get; set; }
        public string Delete { get; set; }
    }

    public class AnnotationManager
    {

        private NetBridge _netBridge;
        public NetBridge NetBridge { get { return _netBridge; } }
        public AnnotationManager(NetBridge netBridge)
        {
            _netBridge = netBridge;
        }

        public AnnotationManager()
        {
            _netBridge = new NetBridge();
        }

        /*
         * CREATE FUNCTION Annotate.f_ListProjectViews
(@projectConfigId UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT AnnotationViewId, ViewName 
FROM Annotate.AnnotationViews WHERE ProjectConfigId = @projectConfigId
         */

        // SELECT AnnotationViewId, ViewName, dsc.ElementType, dsc.TypeDescription

        public class AnnotationView
        {
            public int AnnotationViewId { get; set; }
            public string ViewName { get; set; }
            public string ElementType { get; set; }
            public string TypeDescription { get; set; }
        }

        public List<AnnotationView> ListProjectViews(Guid projectConfigId)
        {
            var results = NetBridge.ExecuteTableFunction("Annotate.f_ListProjectViews",
                new Dictionary<string, object>()
                {
                    { "projectConfigid", projectConfigId }
                });

            var res = new List<AnnotationView>();
            foreach (DataRow dr in results.Rows)
            {
                res.Add(new AnnotationView()
                {
                    AnnotationViewId = (int)dr["AnnotationViewId"],
                    ViewName = (string)dr["ViewName"],
                    ElementType = dr["ElementType"] == DBNull.Value ? (string)"Default" : (string)dr["ElementType"],
                    TypeDescription = dr["TypeDescription"] == DBNull.Value ? (string)"Default" : (string)dr["TypeDescription"]
                });
            }

            return res;
        }

        public class OrderedListItem
        {
            public int Id { get; set; }
            public int Order { get; set; }
        }

        public static DataTable FillOrderedList(List<OrderedListItem> items)
        {
            /*
             * CREATE TYPE [BIDoc].[UDTT_OrderedIdList] AS TABLE(
	[Id] [int] NOT NULL,
	[Order] [int] NOT NULL
)
            */

            DataTable res = new DataTable();
            res.TableName = "BIDoc.UDTT_OrderedIdList";

            res.Columns.Add("Id");
            res.Columns.Add("Order");
            foreach (var item in items)
            {
                var dr = res.NewRow();
                dr[0] = item.Id;
                dr[1] = item.Order;
                res.Rows.Add(dr);
            }

            return res;
        }

        public void UpdateViewFields(int annotationViewId, List<OrderedListItem> fields)
        {
            var ordList = FillOrderedList(fields);
            NetBridge.ExecuteProcedure("[Annotate].[sp_UpdateViewFields]", new Dictionary<string, object>()
                {
                    { "annotationViewId", annotationViewId },
                    { "fields", ordList}
                }
            );
        }

        /*
       CREATE FUNCTION Annotate.f_ListViewFields
(@viewId INT)
RETURNS TABLE
AS RETURN
SELECT vf.AnnotationViewId, f.FieldId, f.FieldName, vf.FieldOrder 
FROM Annotate.Fields f 
INNER JOIN Annotate.AnnotationViewFields vf ON f.FieldId = vf.FieldId
WHERE vf.AnnotationViewId = @viewId

        */

        public List<AnnotationViewField> ListViewFields(int viewId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_ListViewFields", new Dictionary<string, object>()
             {
                 { "viewId", viewId}
             });
            var list = ReadViewFields(dt);
            return list;
        }

        public List<AnnotationViewField> ReadViewFields(DataTable dt)
        {
            List<AnnotationViewField> res = new List<AnnotationViewField>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new AnnotationViewField()
                {
                    FieldId = (int)r["FieldId"],
                    FieldName = (string)r["FieldName"],
                    AnnotationViewId = (int)r["AnnotationViewId"],
                    FieldOrder = (int)r["FieldOrder"]
                });
            }
            return res;
        }
        /*
         CREATE FUNCTION Annotate.f_GetViewFieldValues
        (@viewId INT)
        RETURNS TABLE
        AS RETURN
        SELECT vals.FieldValueId, vals.AnnotationElementId, vals.FieldId, e.ModelElementId, vals.Value

         */

        public List<AnnotationViewFieldValue> GetViewFieldValues(int viewId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_GetViewFieldValues", new Dictionary<string, object>()
             {
                 { "viewId", viewId}
             });
            var res = ReadViewFieldValues(dt);
            return res;
        }


        public List<AnnotationViewFieldValue> GetViewFieldValues(int viewId, string refPathPrefix, string elementType)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_GetViewFieldValuesUnderPath", new Dictionary<string, object>()
             {
                 { "viewId", viewId},
                 { "path", refPathPrefix},
                 { "type", elementType}
             });
            var res = ReadViewFieldValues(dt);
            return res;
        }


        public List<AnnotationViewFieldValue> GetViewFieldValues(int viewId, int modelElementId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_GetViewFieldValues", new Dictionary<string, object>()
             {
                 { "viewId", viewId},
                { "modelElementId", modelElementId }
             });
            var res = ReadViewFieldValues(dt);
            return res;
        }

        private List<AnnotationViewFieldValue> ReadViewFieldValues(DataTable dt)
        {
            List<AnnotationViewFieldValue> res = new List<AnnotationViewFieldValue>();
            foreach (DataRow r in dt.Rows)
            {
                res.Add(new AnnotationViewFieldValue()
                {
                    FieldValueId = (int)r["FieldValueId"],
                    AnnotationElementId = (int)r["AnnotationElementId"],
                    FieldId = (int)r["Fieldid"],
                    ModelElementId = (r["ModelElementId"] == DBNull.Value ? (int?)null : (int)r["ModelElementId"]),
                    Value = (string)r["Value"]
                });
            }
            return res;
        }

        public DataTable ElementLinksToTable(List<AnnotationLinkFromTo> annotationLink)
        {
            /*
CREATE TYPE [Annotate].[UDTT_ElementLinks] AS TABLE
(
	ModelElementFromId INT, 
	ModelElementToId INT, 
	LinkTypeId INT,
	Value NVARCHAR(MAX)
)

             */
            DataTable dt = new DataTable();
            dt.TableName = "Annotate.UDTT_ElementLinks";
            dt.Columns.Add("ModelElementFromId", typeof(int));
            dt.Columns.Add("ModelElementToId", typeof(int));
            dt.Columns.Add("LinkTypeId", typeof(int));
            foreach (var item in annotationLink)
            {
                var nr = dt.NewRow();
                nr[0] = item.ModelElementFromId;
                nr[1] = item.ModelElementToId;
                nr[2] = item.LinkTypeId;
                dt.Rows.Add(nr);
            }

            return dt;
        }

        public DataTable ViewFieldValuesToTable(List<AnnotationViewFieldValue> values)
        {
            /*
             * 	ModelElementId INT, 
	FieldId INT,
	Value NVARCHAR(MAX)
             */
            DataTable dt = new DataTable();
            dt.TableName = "Annotate.UDTT_FieldValues";
            dt.Columns.Add("ModelElementId", typeof(int));
            dt.Columns.Add("FieldId", typeof(int));
            dt.Columns.Add("Value", typeof(string));
            foreach (var item in values)
            {
                var nr = dt.NewRow();
                nr[0] = item.ModelElementId;
                nr[1] = item.FieldId;
                nr[2] = item.Value;
                dt.Rows.Add(nr);
            }
            return dt;
        }

        public void UpdateElementFields(List<AnnotationViewFieldValue> values, Guid projectConfigId, int userId, List<AnnotationLinkFromTo>annotationLinks, List<int> modifiedElementIds)
        {
            /*
             CREATE PROCEDURE [Annotate].[sp_UpdateElementFields]
	@projectConfigId UNIQUEIDENTIFIER,
	@fieldValues  [Annotate].[UDTT_FieldValues] READONLY

            */
            var dt = ViewFieldValuesToTable(values);
            var linksTable = ElementLinksToTable(annotationLinks);
            var modifiedElementsTable = GraphManager.GetIdListTable(modifiedElementIds);
            NetBridge.ExecuteProcedure("[Annotate].[sp_UpdateElementFields]", new Dictionary<string, object>()
             {
                 { "projectConfigId", projectConfigId},
                 { "fieldValues", dt},
                 { "links", linksTable},
                 { "modifiedModelElementIds", modifiedElementsTable},
                 { "userId", userId}
             });
        }

        public void CreateLinksAnnotationsAndModelElements(Guid projectConfigId)
        {
            NetBridge.ExecuteProcedure("Annotate.sp_UpdateModelElememntReferences", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });
        }

        public DataTable GetHistoryTable(Guid projectConfigId, int modelElementId)
        {
            var dt = NetBridge.ExecuteProcedureTable("[Annotate].[f_GetHistory]", new Dictionary<string, object>()
            {
                { "modelElementId", modelElementId },
                { "projectConfigId", projectConfigId }
            });
            return dt;
        }

        public List<AnnotationField> ListFields(Guid projectConfigId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_ListFields", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId }
            });

            return ReadFields(dt);
        }

        public List<AnnotationLinkType>ListLinkTypes(Guid projectConfigId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_ListLinkTypes", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId }
            });

            return ReadLinkTypes(dt);
        }

        public List<AnnotationLinkFromTo>ListLinksFrom(Guid projectConfigId, int elementId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_GetListLinksFrom", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId},
                { "modelElementId", elementId}
            });

            return ReadLinkFrom(dt);
        }

        public List<AnnotationLinkFromTo>ListLinksTo(Guid projectConfigId, int elementId)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_GetListLinksTo", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId},
                { "modelElementId", elementId}
            });

            return ReadLinkTo(dt);
        }

        public List<AnnotationLinkFromTo>GetLinksFrom(Guid projectConfigId, string elementType, string refPath)
        {
            var dt = NetBridge.ExecuteTableFunction("Annotate.f_GetListLinks", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId},
                { "elementType", elementType},
                { "refPath", refPath}
            });

            return ReadLinkFrom(dt);
        }

        private List<AnnotationField> ReadFields(DataTable dt)
        {
            List<AnnotationField> res = new List<AnnotationField>();

            foreach (DataRow dr in dt.Rows)
            {
                AnnotationField af = new AnnotationField()
                {
                    Deleted = (bool)dr["Deleted"],
                    FieldId = (int)dr["FieldId"],
                    FieldName = (string)dr["FieldName"],
                    UsedInViews = (int)dr["UsedInViews"] > 0
                };
                res.Add(af);
            }

            return res;
        }

        private List<AnnotationLinkType> ReadLinkTypes(DataTable dt)
        {
            List<AnnotationLinkType> res = new List<AnnotationLinkType>();

            foreach (DataRow dr in dt.Rows)
            {
                AnnotationLinkType af = new AnnotationLinkType()
                {
                    Deleted = (bool)dr["Deleted"],
                    LinkTypeId = (int)dr["LinkTypeId"],
                    LinkTypeName = (string)dr["LinkTypeName"],
                    UsedInLinks = (int)dr["UsedInLinks"] > 0
                };
                res.Add(af);
            }

            return res;
        }

        private List<AnnotationLinkFromTo>ReadLinkTo(DataTable dt)
        {
            List<AnnotationLinkFromTo> res = new List<AnnotationLinkFromTo>();

            foreach (DataRow dr in dt.Rows)
            {
                AnnotationLinkFromTo al = new AnnotationLinkFromTo()
                {
                    LinkId = (int)dr["ElementLinkId"],
                    LinkTypeName = (string)dr["LinkTypeName"],
                    ElementFromToCaption = (string)dr["ElementFromCaption"],
                    ElementFromToDescriptivePath = (string)dr["ElementFromDescriptivePath"],
                    UpdatedVersion = (int)dr["UpdatedVersion"],
                    Delete = "Delete",
                    LinkTypeId = (int)dr["LinkTypeId"],
                    ModelElementFromId= (int)dr["ModelElementFromId"],
                    ModelElementToId = (int)dr["ModelElementToId"],
                };
                res.Add(al);
            }

            return res;
        }

        private List<AnnotationLinkFromTo>ReadLinkFrom(DataTable dt)
        {
            List<AnnotationLinkFromTo> res = new List<AnnotationLinkFromTo>();

            foreach (DataRow dr in dt.Rows)
            {
                AnnotationLinkFromTo al = new AnnotationLinkFromTo()
                {
                    LinkId = (int)dr["ElementLinkId"],
                    LinkTypeName = (string)dr["LinkTypeName"],
                    ElementFromToCaption = (string)dr["ElementToCaption"],
                    ElementFromToDescriptivePath = (string)dr["ElementToDescriptivePath"],
                    UpdatedVersion = (int)dr["UpdatedVersion"],
                    Delete = "Delete",
                    LinkTypeId = (int)dr["LinkTypeId"],
                    ModelElementFromId = (int)dr["ModelElementFromId"],
                    ModelElementToId = (int)dr["ModelElementToId"],                   
                };
                res.Add(al);
            }
            return res;
        }

        public void AddField(Guid projectConfigId, string fieldName)
        {
            NetBridge.ExecuteProcedure("[Annotate].[sp_CreateField]", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId },
                { "fieldName", fieldName }
            });
        }

        public void AddType(Guid projectConfigId, string linkTypeName)
        {
            NetBridge.ExecuteProcedure("[Annotate].[sp_CreateLinkType]", new Dictionary<string, object>()
            {
                { "projectConfigId", projectConfigId },
                { "linkTypeName", linkTypeName }
            });
        }

        public void DeleteField(int fieldId)
        {
            NetBridge.ExecuteProcedure("[Annotate].[sp_DeleteField]", new Dictionary<string, object>()
            {
                { "fieldId", fieldId}
            });
        }

        public void DeleteLinkType(int linkTypeId)
        {
            NetBridge.ExecuteProcedure("[Annotate].[sp_DeleteLinkType]", new Dictionary<string, object>()
            {
                { "linkTypeId", linkTypeId}
            });
        }

        public void DeleteLink(int linkId)
        {
            NetBridge.ExecuteProcedure("[Annotate].[sp_DeleteLink]", new Dictionary<string, object>()
            {
                { "linkId", linkId}
            });
        }

        public List<ProjectDictionaryFieldMappingItem> ProjectDictionaryFieldsMapping(Guid projectConfigId)
        {
            var table = NetBridge.ExecuteTableFunction("Annotate.f_ProjectDictionaryFieldsMapping", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId }
            });

            List<ProjectDictionaryFieldMappingItem> res = new List<ProjectDictionaryFieldMappingItem>();
            foreach (DataRow r in table.Rows)
            {
                res.Add(new ProjectDictionaryFieldMappingItem()
                {
                    AnnotationViewId = (int)r["AnnotationViewId"],
                    ElementType = (string)r["ElementType"],
                    FieldId = (int)r["FieldId"],
                    FieldName = (string)r["FieldName"],
                    FieldOrder = (int)r["FieldOrder"]
                });
            }

            return res;
        }
        
        public List<AnnotationViewFieldValue> ProjectDictionaryValues(Guid projectConfigId)
        {
            var table = NetBridge.ExecuteTableFunction("Annotate.f_ProjectDictionaryValues", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });

            List<AnnotationViewFieldValue> res = new List<AnnotationViewFieldValue>();
            foreach (DataRow r in table.Rows)
            {
                res.Add(new AnnotationViewFieldValue()
                {
                    AnnotationElementId = (int)r["AnnotationElementId"],
                    FieldId = (int)r["FieldId"],
                    FieldValueId = (int)r["FieldValueId"], // = (string)r["FieldName"],
                    ModelElementId = (int)r["ModelElementId"],
                    Value = (string)r["Value"]
                });
            }

            return res;
        }

        public List<ProjectOlapAttributesLookupItem> ProjectOlapAttributesLookupTable(Guid projectConfigId)
        {
            var table = NetBridge.ExecuteTableFunction("Annotate.f_ProjectOlapAttributesLookupTable", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });

            List<ProjectOlapAttributesLookupItem> res = new List<ProjectOlapAttributesLookupItem>();
            foreach (DataRow r in table.Rows)
            {
                res.Add(new ProjectOlapAttributesLookupItem()
                {
                    AttributeName = (string)r["AttributeName"],
                    CubeAttributeElementId = (int)r["CubeAttributeElementId"],
                    CubeDimensionName = (string)r["CubeDimensionName"],
                    DatabaseDimensionName = (string)r["DatabaseDimensionName"],
                    DimensionAttributeElementId = (int)r["DimensionAttributeElementId"],
                    HierarchyLevelName = r["HierarchyLevelName"] == DBNull.Value ? string.Empty : (string)r["HierarchyLevelName"],
                    HierarchyName = r["HierarchyName"] == DBNull.Value ? string.Empty : (string)r["HierarchyName"], 
                    RefPath = (string)r["RefPath"],
                    ElementType = (string)r["ElementType"]
                });
            }

            return res;
        }


        public List<ProjectOlapMeasuresLookupItem> ProjectOlapMeasuresLookupTable(Guid projectConfigId)
        {
            var table = NetBridge.ExecuteTableFunction("Annotate.f_ProjectOlapMeasuresLookupTable", new Dictionary<string, object>()
            {
                { "projectconfigid", projectConfigId}
            });

            List<ProjectOlapMeasuresLookupItem> res = new List<ProjectOlapMeasuresLookupItem>();
            foreach (DataRow r in table.Rows)
            {
                res.Add(new ProjectOlapMeasuresLookupItem()
                {
                    MeasureElementId = (int)r["MeasureElementId"],
                    MeasureName = (string)r["MeasureName"],
                    RefPath = (string)r["RefPath"],
                    ElementType = (string)r["ElementType"]
                });
            }

            return res;
        }
        
    }

}
