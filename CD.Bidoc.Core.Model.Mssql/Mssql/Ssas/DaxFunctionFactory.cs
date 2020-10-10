using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql.Ssas
{
    public class DaxFunctionFactory
    {
        Dictionary<string, Type> _functionsByName = null;
        
        public DaxFunctionFactory()
        {
            _functionsByName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            var typesWithAttribute = GetTypesWithDaxFunctionNameAttribute(Assembly.GetAssembly(typeof(DaxOperationElement)));
            foreach (var type in typesWithAttribute)
            {
                var attribute = (DaxFunctionName)type.GetCustomAttributes(typeof(DaxFunctionName)).First();
                _functionsByName.Add(attribute.FunctionName, type);
            }

            var nameList = string.Join(Environment.NewLine, _functionsByName.Select(x => x.Value.FullName));
        }

        private RefPath GetFunctionUrn(SsasModelElement parent)
        {
            var ordinal = parent.Children.Count() + 1;
            return parent.RefPath.NamedChild("FunctionCall", "No_" + ordinal);
        }

        private IEnumerable<Type> GetTypesWithDaxFunctionNameAttribute(Assembly assembly)
        {
            foreach (Type type in assembly.GetTypes())
            {
                if (type.GetCustomAttributes(typeof(DaxFunctionName), true).Length > 0)
                {
                    yield return type;
                }
            }
        }

        public DaxScalarFunctionElement CreateScalarFunctionElement(string functionName, DaxElement parent)
        {
            if (_functionsByName.ContainsKey(functionName))
            {
                return CreateScalarFunctionElement(_functionsByName[functionName], functionName, parent);
            }
            else
            {
                return new GeneralDaxScalarFunctionElement(GetFunctionUrn(parent), functionName, functionName, parent);
            }
        }

        public DaxTableFunctionElement CreateTableFunctionElement(string functionName, DaxElement parent)
        {
            if (_functionsByName.ContainsKey(functionName))
            {
                return CreateTableFunctionElement(_functionsByName[functionName], functionName, parent);
            }
            else
            {
                return new UnknownDaxTableFunctionElement(GetFunctionUrn(parent), functionName, functionName, parent);
            }
        }

        public DaxExpressionEvaluationFunctionElement CreateExpressionFunctionElement(string functionName, DaxElement parent)
        {
            if (_functionsByName.ContainsKey(functionName))
            {
                return CreateExpressionFunctionElement(_functionsByName[functionName], functionName, parent);
            }
            else
            {
                throw new Exception();
            }
        }

        // DaxSummarizeColumnsFunctionElement(RefPath refPath, string caption, string definition, MssqlModelElement parent)

        private DaxTableFunctionElement CreateTableFunctionElement(Type functionType, string functionName, DaxElement parent)
        {
            var constructor = functionType.GetConstructor(new Type[] {
                typeof(RefPath),
                typeof(string),
                typeof(string),
                typeof(MssqlModelElement)
            });

            var functionUrn = GetFunctionUrn(parent);
            DaxTableFunctionElement functionElement = (DaxTableFunctionElement)constructor.Invoke(new object[] { functionUrn, functionName, functionName, parent });

            return functionElement;
        }

        private DaxExpressionEvaluationFunctionElement CreateExpressionFunctionElement(Type functionType, string functionName, DaxElement parent)
        {
            var constructor = functionType.GetConstructor(new Type[] {
                typeof(RefPath),
                typeof(string),
                typeof(string),
                typeof(MssqlModelElement)
            });

            var functionUrn = GetFunctionUrn(parent);
            DaxExpressionEvaluationFunctionElement functionElement = 
                (DaxExpressionEvaluationFunctionElement)constructor.Invoke(new object[] { functionUrn, functionName, functionName, parent });

            return functionElement;
        }

        private DaxScalarFunctionElement CreateScalarFunctionElement(Type functionType, string functionName, DaxElement parent)
        {
            var constructor = functionType.GetConstructor(new Type[] {
                typeof(RefPath),
                typeof(string),
                typeof(string),
                typeof(MssqlModelElement)
            });

            var functionUrn = GetFunctionUrn(parent);
            DaxScalarFunctionElement functionElement = (DaxScalarFunctionElement)constructor.Invoke(new object[] { functionUrn, functionName, functionName, parent });

            return functionElement;
        }

        public DaxOperationElement CreateFunctionElement(string functionName, DaxElement parent)
        {
            var refPath = GetFunctionUrn(parent);

            if (!_functionsByName.ContainsKey(functionName))
            {
                ConfigManager.Log.Warning(string.Format("DAX Parser: Unrecognized function {0} in {1}, defaulting to general scalar function", functionName, parent.RefPath.Path));
                return new GeneralDaxScalarFunctionElement(refPath, functionName, functionName, parent);
            }

            var functionType = _functionsByName[functionName];
            if (typeof(DaxTableOperationElement).IsAssignableFrom(functionType))
            {
                return CreateTableFunctionElement(functionName, parent);
            }
            else if (typeof(DaxScalarOperationElement).IsAssignableFrom(functionType))
            {
                return CreateScalarFunctionElement(functionName, parent);
            }
            else if (typeof(DaxExpressionEvaluationFunctionElement).IsAssignableFrom(functionType))
            {
                return CreateExpressionFunctionElement(functionName, parent);
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
