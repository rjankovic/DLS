using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DB_Tools
{

    public class DiagramManager
    {
        private Repository _repository;
        private NetBridge _db;

        public DiagramManager(Repository repository)
        {
            _repository = repository;
            _db = new NetBridge(_repository.ConnectionString);
        }

        public enum DiagramTypeEnum { Logical, CompositeStructure };

        public EA.Diagram CreateDiagram(EA.IDualElement parent, string name, DiagramTypeEnum type)
        {
            EA.Diagram diagram = parent.Diagrams.AddNew(name, type.ToString());

            if (!diagram.Update())
            {
                throw new Exception(diagram.GetLastError());
            }

            if (type == DiagramTypeEnum.CompositeStructure)
            {
                if (!parent.SetCompositeDiagram(diagram.DiagramGUID))
                {
                    throw new Exception(parent.GetLastError());
                }
            }
            return diagram;
        }

        public void ClearDiagram(EA.Diagram diagram)
        {
            for (short i = 0; i < diagram.DiagramLinks.Count; i++)
            {
                diagram.DiagramLinks.Delete(i);
            }
            for (short i = 0; i < diagram.DiagramObjects.Count; i++)
            {
                diagram.DiagramObjects.Delete(i);
            }
            if (!diagram.Update())
            {
                throw new Exception(diagram.GetLastError());
            }
        }

        public void SetDiagramObjects(EA.Diagram diagram, EA.IDualElement objectsParent)
        {
            ClearDiagram(diagram);
            var elements = objectsParent.Elements;

            int offsetLeft = 100;
            int offsetTop = 100;
            int rectH = 100;
            int rectW = 200;
            int attrH = 20;
            int rectMarginT = 50;

            Lookup lookup = new Lookup(_repository);
            DataTable attributeCountTable;

            if (objectsParent is EA.Package)
            {
                attributeCountTable = lookup.ExecuteSelectStatement(@"
SELECT 
o.Object_ID
,o.ea_guid [Guid]
,a.c Attribute_Count 
FROM t_object o
OUTER APPLY (SELECT COUNT(*) FROM t_attribute a WHERE a.Object_ID = o.Object_ID) a(c)
WHERE o.Package_ID = @packageId", new Dictionary<string, object>() { { "packageId", ((EA.Package)objectsParent).PackageID } });
            }
            else if (objectsParent is EA.Element)
            {
                attributeCountTable = lookup.ExecuteSelectStatement(@"
SELECT
o.Object_ID
,o.ea_guid [Guid]
,a.c Attribute_Count 
FROM t_object o 
OUTER APPLY (SELECT COUNT(*) FROM t_attribute a WHERE a.Object_ID = o.Object_ID) a(c)
WHERE o.ParentID = @parentId", new Dictionary<string, object>() { { "parentId", ((EA.Element)objectsParent).ElementID } });
            }
            else
            {
                throw new Exception();
            }

            Dictionary<int, int> attributeCounts = attributeCountTable.AsEnumerable()
                .ToDictionary(x => (int)x["Object_ID"], x => (int)x["Atribute_Count"]);

            foreach (EA.Element elem in elements)
            {
                var elemAttrHeight = attrH * attributeCounts[elem.ElementID];
                var positioningTo = string.Format("l={0};r={1};t={2};b={3};", offsetLeft, offsetLeft + rectW, offsetTop, offsetTop + rectH + elemAttrHeight);
                offsetTop += rectH + elemAttrHeight + rectMarginT;
                EA.DiagramObject diagObj = diagram.DiagramObjects.AddNew(positioningTo, "");
                diagObj.ElementID = elem.ElementID;
                if (!diagObj.Update())
                {
                    throw new Exception(diagObj.GetLastError());
                }
            }

            if (!diagram.Update())
            {
                throw new Exception(diagram.GetLastError());
            }
        }
    }
}
