using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql
{
    /// <summary>
    /// Activates Mssql model elements using reflection.
    /// Used to recreate the model that was stored in a database.
    /// </summary>
    public class ModelActivator : IModelActivator
    {
        /// <summary>
        /// Uses 2-parameter constructor
        /// </summary>
        private class Ctor2Activator : IModelElementActivator
        {
            private readonly ConstructorInfo _twoParamConstructor;

            public Ctor2Activator(ConstructorInfo twoParamConstructor)
            {
                this._twoParamConstructor = twoParamConstructor;
            }

            public IModelElement Activate(string refPath, string caption)
            {
                return (IModelElement)_twoParamConstructor.Invoke(new object[] { new RefPath(refPath), caption });
            }

            public IModelElement Activate(string refPath, string caption, string definition)
            {
                var elem = (IModelElement)_twoParamConstructor.Invoke(new object[] { new RefPath(refPath), caption });
                elem.Definition = definition;
                return elem;
            }
        }
        /// <summary>
        /// Uses 4-parameter constructor
        /// </summary>
        private class Ctor4Activator : IModelElementActivator
        {
            private readonly ConstructorInfo _fourParamConstructor;

            public Ctor4Activator(ConstructorInfo fourParamConstructor)
            {
                this._fourParamConstructor = fourParamConstructor;
            }

            public IModelElement Activate(string refPath, string caption)
            {
                return (IModelElement)_fourParamConstructor.Invoke(new object[] { new RefPath(refPath), caption, null, null });
            }

            public IModelElement Activate(string refPath, string caption, string definition)
            {
                return (IModelElement)_fourParamConstructor.Invoke(new object[] { new RefPath(refPath), caption, definition, null });
            }
        }


        private readonly Type[] _ctor2type = new Type[] { typeof(RefPath), typeof(string) };
        
        private bool IsCtor4Par(ParameterInfo[] par)
        {
            if (par.Length != 4)
                return false;
            if (par[0].ParameterType != typeof(RefPath))
                return false;
            if (par[1].ParameterType != typeof(string))
                return false;
            return true;
        }

        public IModelElementActivator GetActivatorFor(string typeName)
        {

            if (typeName.StartsWith("CD.DLS.Model", StringComparison.Ordinal))
            {
                //if (typeName.StartsWith("CD.DLS.Model.Mssql", StringComparison.Ordinal)) {
                Type type = Assembly.GetExecutingAssembly().GetType(typeName);
                if (type != null)
                {
                    ConstructorInfo ctor2 = type.GetConstructor(_ctor2type);
                    if (ctor2 != null)
                    {
                        return new Ctor2Activator(ctor2);
                    }

                    foreach (ConstructorInfo ctor4 in type.GetConstructors())
                    {
                        var par = ctor4.GetParameters();
                        if (IsCtor4Par(par))
                        {
                            return new Ctor4Activator(ctor4);
                        }
                    }
                    ConfigManager.Log.Error(string.Format("Could not find a suitable constructor for {0}", type.FullName));
                }
                else
                {
                    ConfigManager.Log.Error(string.Format("The type {0} could not be found in {1}", typeName, Assembly.GetExecutingAssembly().FullName));
                }
            }
            else
            {
                ConfigManager.Log.Error(string.Format("The type {0} does not have the required prefix", typeName));
            }
            //Cannot activate object
            return null;
        }
    }
}
