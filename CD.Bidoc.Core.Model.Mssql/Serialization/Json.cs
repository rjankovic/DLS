using CD.DLS.Model.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Serialization
{
    public class JsonReflectionHelper : IReflectionHelper
    {
        private readonly IModelActivator _modelActivator;

        public JsonReflectionHelper(IModelActivator ma)
        {
            this._modelActivator = ma;
        }

        public object CreateObject(string typeName, string refPath, string caption)
        {
            var activator = _modelActivator.GetActivatorFor(typeName);
            if (activator == null)
                throw new Exception(string.Format("Cannot find how to activate model element of type {0}", typeName));
            return activator.Activate(refPath, caption);
        }

        public object CreateObject(string typeName, string refPath, string definition, string caption)
        {
            var activator = _modelActivator.GetActivatorFor(typeName);
            if (activator == null)
                throw new Exception(string.Format("Cannot find how to activate model element of type {0}", typeName));
            return activator.Activate(refPath, caption, definition);
        }

        public string GetTypeName(object obj)
        {
            return obj.GetType().FullName;
        }

        public void PopulateExtendedProperties(object obj, string extendedProperties)
        {
            JsonConvert.PopulateObject(extendedProperties, obj);
        }

        public string ReflectExtendedProperties(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
