using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql
{
    /// <summary>
    /// Converts Mssql model to or from another model type.
    /// </summary>
    public class ModelConverter : IModelConverter<MssqlModelElement>
    {
        /// <summary>
        /// Provides reflection functionality.
        /// </summary>
        private IReflectionHelper _reflection;

        public ModelConverter(IReflectionHelper reflection)
        {
            this._reflection = reflection;
        }

        /// <summary>
        /// Creates a model element.
        /// </summary>
        public MssqlModelElement Factory(int id, string type, string refPath, string caption, string definition, string extendedProperties)
        {
            MssqlModelElement obj = (MssqlModelElement)_reflection.CreateObject(type, refPath, definition, caption);
            obj.Id = id;
            _reflection.PopulateExtendedProperties(obj, extendedProperties);
            return obj;
        }

        public MssqlModelElement Factory(int id, string type, string refPath, string caption, string extendedProperties)
        {
            MssqlModelElement obj = (MssqlModelElement)_reflection.CreateObject(type, refPath, caption);
            obj.Id = id;
            _reflection.PopulateExtendedProperties(obj, extendedProperties);
            return obj;
        }


        /// <summary>
        /// Links converted model elements.
        /// </summary>
        public void Link(MssqlModelElement from, MssqlModelElement to, string type, string extendedProperties)
        {
            if (type == "parent")
            {
                // The parent element is saved as a link of type "parent"
                to.AddChild(from);
                from.Parent = to;
            }
            else
            {
                from.AddLink(to, type, _reflection);
            }
        }

        private void ConvertModelNode<TTarget>(ModelConversion<MssqlModelElement, TTarget> conversion, MssqlModelElement modelElement)
        {
            string extendedProperties = _reflection.ReflectExtendedProperties(modelElement);
            //if (modelElement.Caption == "ReceivedOrderTypeCode" && modelElement is Db.ColumnElement)
            //{

            //}
            TTarget converted = conversion.Convert(modelElement, modelElement.Id, _reflection.GetTypeName(modelElement), modelElement.RefPath.Path, modelElement.Caption, modelElement.Definition, extendedProperties);

            if (modelElement.Parent != null)
            {
                conversion.Link(modelElement, modelElement.Parent, "parent");
            }

            foreach (var child in modelElement.Children)
            {
                ConvertModelNode(conversion, child);
            }
        }

        /// <summary>
        /// Converts mssql model to another model
        /// </summary>
        public void Convert<TTarget>(MssqlModelElement root, IModelConverter<TTarget> targetConverter)
        {
            ModelConversion<MssqlModelElement, TTarget> conversion = new ModelConversion<MssqlModelElement, TTarget>(targetConverter);

            ConvertModelNode(conversion, root);

            foreach (var m in conversion.MappedObjects)
            {
                m.SaveLinks(conversion, _reflection);
            }
        }

        /// <summary>
        /// Converts mssql model to another model, appending it to previously converted parts
        /// </summary>
        public Dictionary<MssqlModelElement, TTarget> Convert<TTarget>(MssqlModelElement root, IModelConverter<TTarget> targetConverter, Dictionary<MssqlModelElement, TTarget> convertedEnvironment)
        {
            ModelConversion<MssqlModelElement, TTarget> conversion = new ModelConversion<MssqlModelElement, TTarget>(targetConverter, convertedEnvironment);

            ConvertModelNode(conversion, root);

            ConvertModelNodeLinks(conversion, root);

            /*
            foreach (var m in conversion.MappedObjects)
            {
                if (m.RefPath.Path.StartsWith(root.RefPath.Path)) //(!convertedEnvironment.ContainsKey(m))
                {
                    m.SaveLinks(conversion, _reflection);
                }
            }*/

            var conversionAdditions = conversion.ConversionMap.Where(x => !convertedEnvironment.ContainsKey(x.Key)).ToDictionary(x => x.Key, y => y.Value);
            return conversionAdditions;
        }

        private void ConvertModelNodeLinks<TTarget>(ModelConversion<MssqlModelElement, TTarget> conversion, MssqlModelElement modelElement)
        {
            modelElement.SaveLinks(conversion, _reflection);

            foreach (var child in modelElement.Children)
            {
                ConvertModelNodeLinks(conversion, child);
            }
        }

        public void LinkBatch(MssqlModelElement from, IEnumerable<MssqlModelElement> to, string type, string extendedProperties)
        {
            foreach (var t in to)
            {
                Link(from, t, type, extendedProperties);
            }
        }
    }
}
