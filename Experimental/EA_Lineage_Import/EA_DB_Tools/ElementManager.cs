using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Tools
{

    public class ElementManager
    {
        private Repository _repository;
        private NetBridge _db;

        public ElementManager(Repository repository)
        {
            _repository = repository;
            _db = new NetBridge(_repository.ConnectionString);
        }

        public class AttributeSpecifiaction
        {
            public enum AttributeTypeEnum { Int, Char }
            public string Name;
            public AttributeTypeEnum Type;
            public string Value;
            public string Notes;
            public Dictionary<string, string> TaggedValues;
            public string AttributeGUID;
        }

        private void AddAttriubtesToElement(EA.Element e, List<AttributeSpecifiaction> attributes)
        {
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    EA.Attribute a = e.Attributes.AddNew(attr.Name, attr.Type.ToString().ToLower());
                    if (a.Notes != null)
                    {
                        a.Notes = attr.Notes;
                    }
                    
                    a.Default = attr.Value;
                    if (!a.Update())
                    {
                        throw new Exception(a.GetLastError());
                    }
                    attr.AttributeGUID = a.AttributeGUID;

                    if (attr.TaggedValues != null)
                    {
                        foreach (var tv in attr.TaggedValues)
                        {
                            var tvObj = a.TaggedValues.AddNew(tv.Key, string.Empty);
                            tvObj.Value = tv.Value.Substring(0, Math.Min(255, tv.Value.Length));
                            if (!tvObj.Update())
                            {
                                throw new Exception(tvObj.GetLastError());
                            }
                        }
                    }
                }
            }
        }

        private void ClearAttributes(EA.Element element)
        {
            for (short i = 0; i < element.Attributes.Count; i++)
            {
                element.Attributes.Delete(i);
            }
            element.Attributes.Refresh();
        }

        public void DeleteElement(EA.Element element)
        {
            var package = _repository.GetPackageById(element.PackageID);
            short index = 0;
            while (((EA.IDualElement)package.Elements.GetAt(index)).ElementID != element.ElementID)
            {
                index++;
            }
            package.Elements.DeleteAt(index, true);
        }

        public EA.Connector CreateConnector(ConnectorProperteis connectorProperties)
        {
            var connectorTypeString = connectorProperties.ConnectorType.ToString();
            if (connectorProperties.ConnectorType == ConnectorTypeEnum.DirectedAssociation)
            {
                connectorTypeString = "Directed Association";
            }
            EA.Connector connector = connectorProperties.SourceElement.Connectors.AddNew(connectorProperties.Name, connectorTypeString);
            connector.SupplierID = connectorProperties.TargetElement.ElementID;
            string styleEx = null;
            if (string.IsNullOrEmpty(connectorProperties.SourceAttributeGUID) && !string.IsNullOrEmpty(connectorProperties.TargetAttributeGUID))
            {
                styleEx = string.Format("LFEP={0}R;", connectorProperties.TargetAttributeGUID);
            }
            else if (!string.IsNullOrEmpty(connectorProperties.SourceAttributeGUID) && string.IsNullOrEmpty(connectorProperties.TargetAttributeGUID))
            {
                styleEx = string.Format("LFSP={0}R;", connectorProperties.SourceAttributeGUID);
            }
            else if (!string.IsNullOrEmpty(connectorProperties.SourceAttributeGUID) && !string.IsNullOrEmpty(connectorProperties.TargetAttributeGUID))
            {
                styleEx = string.Format("LFEP={1}L;LFSP={0}L;", connectorProperties.SourceAttributeGUID, connectorProperties.TargetAttributeGUID);
            }
            if (styleEx != null)
            {
                connector.StyleEx = styleEx;
            }
            
            if (!connector.Update())
            {
                throw new Exception(connector.GetLastError());
            }
            connectorProperties.SourceElement.Connectors.Refresh();
            connectorProperties.TargetElement.Connectors.Refresh();

            return connector;
        }

        public EA.Element CreateClass(EA.IDualElement parent, string name, List<AttributeSpecifiaction> attributes = null)
        {
            EA.Element e = parent.Elements.AddNew(name, "Class");
            if (!e.Update())
            {
                throw new Exception(e.GetLastError());
            }
            AddAttriubtesToElement(e, attributes);
            return e;
        }

        public EA.Element CreateClass(EA.IDualPackage parent, string name, List<AttributeSpecifiaction> attributes = null)
        {
            EA.Element e = parent.Elements.AddNew(name, "Class");
            if (!e.Update())
            {
                throw new Exception(e.GetLastError());
            }
            AddAttriubtesToElement(e, attributes);
            return e;
        }

        public EA.Element UpdateElement(EA.Element e, string name, List<AttributeSpecifiaction> attributes = null)
        {
            e.Name = name;
            if (!e.Update())
            {
                throw new Exception(e.GetLastError());
            }
            ClearAttributes(e);
            AddAttriubtesToElement(e, attributes);
            return e;
        }
    }
}
