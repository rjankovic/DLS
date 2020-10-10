using CD.DLS.Model.Mssql.Ssis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Parse.Mssql.Ssis
{
    public class IOPerComponent
    {
        public Dictionary<string, ComponentAllIO> Dictionary = new Dictionary<string, ComponentAllIO>();
        public ComponentAllIO this[string identificationString]
        {
            get
            {
                return Dictionary[identificationString];
            }
            set
            {
                Dictionary[identificationString] = value;
            }
        }
    }

    public class ComponentAllIO
    {
        private Dictionary<string, ComponentInput> _inputs = new Dictionary<string, ComponentInput>();
        private Dictionary<string, ComponentOutput> _outputs = new Dictionary<string, ComponentOutput>();

        public DfComponentElement ModelElement { get; set; }

        internal Dictionary<string, ComponentInput> Inputs
        {
            get
            {
                return _inputs;
            }

            set
            {
                _inputs = value;
            }
        }

        internal Dictionary<string, ComponentOutput> Outputs
        {
            get
            {
                return _outputs;
            }

            set
            {
                _outputs = value;
            }
        }
    }

    public class ComponentInput
    {
        public Dictionary<string, DfColumnElement> Dictionary = new Dictionary<string, DfColumnElement>(StringComparer.OrdinalIgnoreCase);
        public DfColumnElement this[string identificationString]
        {
            get
            {
                return Dictionary[identificationString];
            }
            set
            {
                Dictionary[identificationString] = value;
            }
        }

        public DfInputElement ModelElement { get; set; }
    }

    public class ComponentOutput
    {
        public Dictionary<string, DfColumnElement> Dictionary = new Dictionary<string, DfColumnElement>(StringComparer.OrdinalIgnoreCase);
        public DfColumnElement this[string identificationString]
        {
            get
            {
                return Dictionary[identificationString];
            }
            set
            {
                Dictionary[identificationString] = value;
            }
        }

        public DfOutputElement ModelElement { get; set; }
    }

}
