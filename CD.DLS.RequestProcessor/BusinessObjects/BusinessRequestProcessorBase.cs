using CD.DLS.API.Structures;
using CD.DLS.Model.Business.Excel;
using CD.DLS.Model.Business.Organization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.Model.Mssql;

namespace CD.DLS.RequestProcessor.BusinessObjects
{
    public abstract class BusinessRequestProcessorBase : RequestProcessorBase
    {
        //protected PivotTableStructure PivotTableModelToStructure(PivotTableTemplateElement model)
        //{
        //    PivotTableStructure structure = new PivotTableStructure();

        //    structure.ConnectionString = model.ConnectionString;
        //    structure.CubeName = model.CubeName;
        //    structure.ValuesOrientation = model.ValuesOrientation;

        //    structure.VisibleFields = new List<PivotTableField>();

        //    foreach (var vf in model.VisibleFields)
        //    {
        //        PivotTableField sf = new PivotTableField();
        //        sf.Attribute = vf.Attribute;
        //        sf.Dimension = vf.Dimension;
        //        sf.Hierarchy = vf.Hierarchy;
        //        sf.Orientation = vf.Orientation;
        //        sf.Position = vf.Position;
        //        sf.SourceName = vf.SourceName;
        //        sf.VisibleItems = new List<API.Structures.PivotFieldItem>();
        //        sf.Filters = new List<PivotTableFilter>();

        //        if (vf.VisibleItems != null)
        //        {
        //            foreach (var visibleItem in vf.VisibleItems)
        //            {
        //                sf.VisibleItems.Add(new API.Structures.PivotFieldItem()
        //                {
        //                    ItemName = visibleItem.ItemName
        //                });
        //            }
        //        }

        //        if (vf.Filters != null)
        //        {
        //            foreach (var filter in vf.Filters)
        //            {
        //                var sFilter = new PivotTableFilter();
        //                sFilter.MeasureName = filter.MeasureName;
        //                sFilter.Type = filter.Type;
        //                sFilter.Value1 = filter.Value1;
        //                sFilter.Value2 = filter.Value2;
        //                sf.Filters.Add(sFilter);
        //            }
        //        }

        //        structure.VisibleFields.Add(sf);
        //    }

        //    return structure;
        //}

        /// <summary>
        /// If a table with the same name already exists in the folder, it will be overwritten
        /// </summary>
        /// <param name="structure"></param>
        /// <param name="parentFolder"></param>
        /// <param name="elementName"></param>
        /// <returns></returns>
        protected PivotTableTemplateElement PivotTableStructureToModel(PivotTableStructure structure, BusinessFolderElement parentFolder, string elementName)
        {
            var tableRefPath = parentFolder.RefPath.NamedChild("PivotTableTemplate", elementName);

            PivotTableTemplateElement templateElement = new PivotTableTemplateElement(tableRefPath, elementName, null, parentFolder);
            var existentChild = parentFolder.Children.FirstOrDefault(x => x is PivotTableTemplateElement && x.Caption == elementName);
            if (existentChild != null)
            {
                // overwrite
                existentChild = templateElement;
            }
            else
            {
                parentFolder.AddChild(templateElement);
            }

            templateElement.ConnectionString = structure.ConnectionString;
            templateElement.CubeName = structure.CubeName;
            templateElement.ValuesOrientation = structure.ValuesOrientation;
            templateElement.PivotTableStructure = structure;

            int fieldCounter = 1;
            foreach (var field in structure.VisibleFields)
            {
                var fieldRefPath = tableRefPath.NamedChild("Field", string.Format("Field_{0}", fieldCounter));
                var modelField = new PivotTableFieldElement(fieldRefPath, string.Format("Field_{0}", fieldCounter++), null, templateElement);
                templateElement.AddChild(modelField);

                modelField.Attribute = field.Attribute;
                modelField.Dimension = field.Dimension;
                modelField.Hierarchy = field.Hierarchy;
                modelField.Orientation = field.Orientation;
                modelField.Position = field.Position;
                modelField.SourceName = field.SourceName;
                modelField.VisibleItems = new List<Model.Business.Excel.PivotFieldItem>();

                if (field.VisibleItems != null)
                {
                    foreach (var vi in field.VisibleItems)
                    {
                        modelField.VisibleItems.Add(new Model.Business.Excel.PivotFieldItem()
                        {
                            ItemName = vi.ItemName
                        });
                    }
                }

                if (field.Filters != null)
                {
                    int filterCounter = 1;
                    foreach (var filter in field.Filters)
                    {
                        var filterRefPath = fieldRefPath.NamedChild("ValueFilter", string.Format("Filter_{0}", filterCounter));
                        var modelFilter = new PivotTableValuesFilterElement(filterRefPath, string.Format("Filter_{0}", filterCounter++), null, modelField);
                        modelField.AddChild(modelFilter);

                        modelFilter.MeasureName = filter.MeasureName;
                        modelFilter.Type = filter.Type;
                        modelFilter.Value1 = filter.Value1;
                        modelFilter.Value2 = filter.Value2;
                    }
                }
            }

            return templateElement;
        }
    }
}
