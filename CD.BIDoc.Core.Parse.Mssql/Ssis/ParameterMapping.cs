using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Ssis
{
    internal class ParameterMappings
    {
        public enum ParameterDirectionEnum { Input, Output };

        public class ParameterMapping
        {
            private string _parameterName;
            private ParameterDirectionEnum _direction;
            private string _variableId;

            public string ParameterName
            {
                get { return _parameterName; }
            }

            public ParameterDirectionEnum Directíon
            {
                get { return _direction; }
            }

            public string VariableId
            {
                get { return _variableId; }
            }

            public ParameterMapping(string mapString)
            {
                var kv = mapString.Split(',');
                var k = kv[0].Trim('\"');
                _variableId = kv[1];
                var p = k.Split(':');
                _parameterName = p[0];
                if (p.Length == 1)
                {
                    _direction = ParameterDirectionEnum.Input;
                }
                else
                {
                    var kDir = p[1];
                    _direction = (ParameterDirectionEnum)(Enum.Parse(typeof(ParameterDirectionEnum), kDir));
                }
            }
        }

        private List<ParameterMapping> _mappings = new List<ParameterMapping>();

        public IEnumerable<ParameterMapping> Mappings { get { return _mappings; } }

        public ParameterMappings(string mapString)
        {
            var mappingDefs = mapString.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var mappingDef in mappingDefs)
            {
                _mappings.Add(new ParameterMapping(mappingDef));
            }

        }
    }
}
