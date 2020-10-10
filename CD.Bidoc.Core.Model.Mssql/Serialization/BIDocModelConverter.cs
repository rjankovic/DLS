using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CD.DLS.DAL.Objects.BIDoc;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Serialization
{
    public class ModelNotAvailableException : Exception
    {
        public ModelNotAvailableException():
            base("Model not available. Use ExtractMetadataRequest to create the model.")
        {
        }
    }

    public class FromBIDocModelConverter 
    {
        private readonly BIDocModelStored _model;

        public FromBIDocModelConverter(BIDocModelStored model)
        {
            _model = model;
        }


        public TTarget Convert<TTarget>(IModelConverter<TTarget> target, int rootId = 1)
        {
            ModelConversion<int, TTarget> conversion = new ModelConversion<int, TTarget>(target);

            foreach (BIDocModelElement element in _model.Elements)
            {
                var cvt = conversion.Convert(element.Id, element.Id, element.Type, element.RefPath, element.Caption, element.Definition, element.ExtendedProperties);
            }
            foreach (BIDocModelLink link in _model.Links)
            {
                conversion.Link(link.ElementFromId, link.ElementToId, link.Type, link.ExtendedProperties);
            }

            if (_model.Elements.Count == 0)
            {
                return default(TTarget);
            }

            var rootEntity = _model.Elements.Where(t => t.Id == rootId).SingleOrDefault();
            
            if (rootEntity == null)
                throw new ModelNotAvailableException();

            return conversion[rootEntity.Id];
        }

        public IEnumerable<TTarget> ConvertFlat<TTarget>(IModelConverter<TTarget> target)
        {
            ModelConversion<int, TTarget> conversion = new ModelConversion<int, TTarget>(target);

            foreach (BIDocModelElement element in _model.Elements)
            {
                var cvt = conversion.Convert(element.Id, element.Id, element.Type, element.RefPath, element.Caption, element.Definition, element.ExtendedProperties);
            }
            foreach (BIDocModelLink link in _model.Links)
            {
                conversion.Link(link.ElementFromId, link.ElementToId, link.Type, link.ExtendedProperties);
            }
            
            return conversion.TargetObjects;
        }
    }

    public class ToBIDocModelConverter : IModelConverter<int>
    {
        private readonly BIDocModelBulk _model;

        public ToBIDocModelConverter(BIDocModelBulk model)
        {
            _model = model;
        }


        public int Factory(int id, string type, string refPath, string caption, string extendedProperties)
        {
            var modelElement = new BIDocModelElement
            {
                Type = type,
                RefPath = refPath,
                Caption = caption,
                ExtendedProperties = extendedProperties,
            };
            _model.AddElement(modelElement);
            return modelElement.Id;
            
        }

        public int Factory(int id, string type, string refPath, string caption, string definition, string extendedProperties)
        {
            var modelElement = new BIDocModelElement
            {
                Type = type,
                RefPath = refPath,
                Caption = caption,
                Definition = definition,// == null ? null : definition.Substring(0, 100000),
                ExtendedProperties = extendedProperties,
            };
            _model.AddElement(modelElement);
            return modelElement.Id;

        }

        public void Link(int from, int to, string type, string extendedProperties)
        {
            var bml = new BIDocModelLink
            {
                ElementFromId = from,
                ElementToId = to,
                Type = type,
                ExtendedProperties = extendedProperties
            };

            _model.AddLink(bml);
        }

        public void LinkBatch(int from, IEnumerable<int> to, string type, string extendedProperties)
        {
            foreach (var t in to)
                Link(from, t, type, extendedProperties);
        }
    }
}
