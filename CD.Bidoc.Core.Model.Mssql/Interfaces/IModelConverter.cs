using CD.DLS.DAL.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Interfaces
{
    public interface IModelConverter<TModelElement>
    {
        TModelElement Factory(int id, string type, string refPath, string caption, string extendedProperties);

        TModelElement Factory(int id, string type, string refPath, string caption, string definition, string extendedProperties);


        void Link(TModelElement from, TModelElement to, string type, string extendedProperties);

        /// <summary>
        /// The same as Link, but allows fewer calls when there are mulitple links with the same type and the same extended properties.
        /// If this is not useful, implement this method by calling Link for each element of to.
        /// </summary>
        void LinkBatch(TModelElement from, IEnumerable<TModelElement> to, string type, string extendedProperties);
    }

    public interface IModelElementActivator
    {
        IModelElement Activate(string refPath, string caption, string definition);

        IModelElement Activate(string refPath, string caption);
    }
    public interface IModelActivator
    {
        IModelElementActivator GetActivatorFor(string typeName);
    }
    public interface IReflectionHelper
    {
        object CreateObject(string type, string refPath, string caption);
        object CreateObject(string type, string refPath, string definition, string caption);
        string GetTypeName(object obj);
        string ReflectExtendedProperties(object obj);
        void PopulateExtendedProperties(object obj, string extendedProperties);
    }

    public class ModelConversion<TSource, TTarget>
    {
        private IModelConverter<TTarget> _targetConverter;
        private Dictionary<TSource, TTarget> _conversionMap = new Dictionary<TSource, TTarget>();
        public Dictionary<TSource, TTarget> ConversionMap
        {
            get { return _conversionMap; }
        }

        public ModelConversion(IModelConverter<TTarget> targetConverter)
        {
            this._targetConverter = targetConverter;
        }

        public ModelConversion(IModelConverter<TTarget> targetConverter, Dictionary<TSource, TTarget> preconvertedParts)
        {
            this._targetConverter = targetConverter;
            foreach (var kv in preconvertedParts)
            {
                _conversionMap.Add(kv.Key, kv.Value);
            }
        }

        public TTarget Convert(TSource source, int id, string type, string refPath, string caption, string extendedProperties)
        {
            TTarget target = _targetConverter.Factory(id, type, refPath, caption, extendedProperties);

            // for update-mode conversion
            if (!_conversionMap.ContainsKey(source))
            {
                _conversionMap.Add(source, target);
            }
            return target;
        }

        public TTarget Convert(TSource source, int id, string type, string refPath, string caption, string definition, string extendedProperties)
        {
            TTarget target = _targetConverter.Factory(id, type, refPath, caption, definition, extendedProperties);

            // for update-mode conversion
            if (!_conversionMap.ContainsKey(source))
            {
                _conversionMap.Add(source, target);
            }
            return target;
        }

        public void Link(TSource from, TSource to, string type, string extendedProperties = "")
        {
            //when links are created in parallel, there can be "dirty" links whose elements were not loaded to the conversion map
            if (_conversionMap.ContainsKey(from))
            {
                try
                {
                    _targetConverter.Link(_conversionMap[from], _conversionMap[to], type, extendedProperties);
                }
                catch (Exception ex)
                {
                    ConfigManager.Log.Error(string.Format("Could not deserialize link from {0} to {1}", from.ToString(), to.ToString()));
                    throw;
                }
            }
        }
        public void LinkBatch(TSource from, IEnumerable<TSource> to, string type, string extendedProperties = "")
        {
            if (_conversionMap.ContainsKey(from))
            {
                _targetConverter.LinkBatch(_conversionMap[from], to.Select(e => _conversionMap[e]), type, extendedProperties);
            }
        }

        public void Add(TSource m, TTarget n)
        {
            _conversionMap.Add(m, n);
        }
        public IEnumerable<TSource> MappedObjects
        {
            get
            {
                return _conversionMap.Keys;
            }
        }
        public IEnumerable<TTarget> TargetObjects
        {
            get
            {
                return _conversionMap.Values;
            }
        }
        public TTarget this[TSource obj] { get { return _conversionMap[obj]; } }
    }
}
