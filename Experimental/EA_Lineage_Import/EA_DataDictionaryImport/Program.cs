using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EA_DataDictionaryImport
{
    class Program
    {
        private const string FILE_PATH = @"C:\CDFramework\XLS_Input\NR_Detailni_datova_specifikace_v01.xlsx";
        private const string SHEET_NAME = "Atributy a ukazatele";
        private const string EA_REPO_NAME = "Enterprise_Architect_NOIS";
        private const string EA_CONNECTION_STRING = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=Enterprise_Architect_NOIS;Data Source=fsczprsa0010;";
        private const string EA_PKG_RROT = "Sandbox/Radovan Jankovic/XLS_Imports/Attribute_List";

        static EA_DB_Tools.Repository _repo;
        static EA_DB_Tools.ElementManager _elemMngr;
        static EA_DB_Tools.PackageManager _pkgMngr;
        static EA_DB_Tools.Lookup _lookup;


        static void Main(string[] args)
        {
            var tbl = ExcelTools.ExcelTools.ReadSheet(FILE_PATH, SHEET_NAME, true, 2, 0);

            Dictionary<string, Dictionary<string, EA.Element>> entitiesAndAreas = new Dictionary<string, Dictionary<string, EA.Element>>();

            for (int i = 0; i < tbl.Rows.Count; i++)
            {
                var entity = tbl.Rows[i]["Entity"].ToString().Trim();
                var area = tbl.Rows[i]["Entity Area"].ToString().Trim();
                if (area == string.Empty && entity == string.Empty)
                {
                    continue;
                }
                if (!entitiesAndAreas.ContainsKey(entity))
                {
                    entitiesAndAreas.Add(entity, new Dictionary<string, EA.Element>());
                }
                if (!entitiesAndAreas[entity].ContainsKey(area))
                {
                    entitiesAndAreas[entity].Add(area, null);
                }
            }

            var attributesGrouped = tbl.AsEnumerable().GroupBy(x => x["Entity"].ToString().Trim()).ToDictionary(x => x.Key, x => x.GroupBy(y => y["Entity Area"].ToString().Trim()).ToDictionary(z => z.Key, z => z));

            _repo = new EA_DB_Tools.Repository(EA_CONNECTION_STRING, EA_REPO_NAME);
            _elemMngr = new EA_DB_Tools.ElementManager(_repo);
            _pkgMngr = new EA_DB_Tools.PackageManager(_repo);
            _lookup = new EA_DB_Tools.Lookup(_repo);
            var pkgRoot = _lookup.GetPackage(EA_PKG_RROT);
            _pkgMngr.ClearPackage(pkgRoot);

            Dictionary<int, Tuple<EA.Element, EA_DB_Tools.ElementManager.AttributeSpecifiaction>> attributeIdMap =
                    new Dictionary<int, Tuple<EA.Element, EA_DB_Tools.ElementManager.AttributeSpecifiaction>>();
            Dictionary<int, List<int>> duplicateLists = new Dictionary<int, List<int>>();

            foreach (var entity in entitiesAndAreas.Keys)
            {
                
                var entityElement = _elemMngr.CreateClass(pkgRoot, entity);
                Dictionary<string, EA.Element> entityAreaElements = new Dictionary<string, EA.Element>();
                foreach (var entityArea in entitiesAndAreas[entity].Keys)
                {
                    var attributes = attributesGrouped[entity][entityArea];
                    List<EA_DB_Tools.ElementManager.AttributeSpecifiaction> attrSpecs = new List<EA_DB_Tools.ElementManager.AttributeSpecifiaction>();
                    foreach (var attribute in attributes)
                    {
                        var nameCZ = attribute["Návrh názvu atributu CD"].ToString();
                        var descriptionCZ = attribute["Popis/ Význam "].ToString();
                        var issues = attribute["ISSUES"].ToString();
                        var duplicate = attribute["duplicita"].ToString();
                        var approvalStatus = attribute["Status procesu schvalování"].ToString();
                        var lifecycle = attribute["Životní cyklus atributu"].ToString();
                        var descriptionEN = attribute["Description"].ToString();
                        var nameEN = attribute["Návrh názvu atributu Anglický CD"].ToString();
                        var attrId = attribute["ID Atributu"].ToString();
                        var attrIdInt = int.Parse(attrId);

                        if (string.IsNullOrEmpty(nameEN))
                        {
                            continue;
                        }

                        duplicateLists.Add(attrIdInt, new List<int>());
                        var duplicates = duplicate.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var dplStr in duplicates)
                        {
                            if (string.IsNullOrWhiteSpace(dplStr))
                            {
                                continue;
                            }
                            int duplId;
                            var succ = int.TryParse(dplStr.Trim(), out duplId);
                            if (succ)
                            {
                                duplicateLists[attrIdInt].Add(duplId);
                            }
                        }
                        
                        var spec = new EA_DB_Tools.ElementManager.AttributeSpecifiaction();
                        spec.Name = nameEN;
                        spec.Type = EA_DB_Tools.ElementManager.AttributeSpecifiaction.AttributeTypeEnum.Char;
                        spec.Value = string.Empty;
                        spec.Notes = descriptionCZ;
                        spec.TaggedValues = new Dictionary<string, string>();
                        spec.TaggedValues.Add("VWFS::Issues", issues);
                        spec.TaggedValues.Add("VWFS::AttributeId", attrId);
                        spec.TaggedValues.Add("VWFS::ApprovalStatus", approvalStatus);
                        spec.TaggedValues.Add("VWFS::AttributeLifeCycle", lifecycle);
                        spec.TaggedValues.Add("VWFS::NameCZ", nameCZ);
                        spec.TaggedValues.Add("VWFS::DescriptionEN", descriptionEN);
                        attrSpecs.Add(spec);
                    }
                    
                    var entityAreaElement = _elemMngr.CreateClass(entityElement, entityArea, attrSpecs);
                    foreach (var attribute in attrSpecs)
                    {
                        attributeIdMap.Add(int.Parse(attribute.TaggedValues["VWFS::AttributeId"]), new Tuple<EA.Element, EA_DB_Tools.ElementManager.AttributeSpecifiaction>(entityAreaElement, attribute));
                    }
                    entityAreaElements[entityArea] = entityAreaElement;
                }
                foreach (var area in entityAreaElements.Keys)
                {
                    entitiesAndAreas[entity][area] = entityAreaElements[area];
                }
            }

            foreach (var duplicateAttr in duplicateLists.Keys)
            {
                var duplSource = attributeIdMap[duplicateAttr];
                foreach (var duplicateDst in duplicateLists[duplicateAttr])
                {
                    if (!attributeIdMap.ContainsKey(duplicateDst))
                    {
                        continue;
                    }
                    var duplDest = attributeIdMap[duplicateDst];
                    var srcId = duplSource.Item1.ElementID;
                    var dstId = duplDest.Item1.ElementID;
                    

                    _elemMngr.CreateConnector(new EA_DB_Tools.ConnectorProperteis()
                    {
                        ConnectorType = EA_DB_Tools.ConnectorTypeEnum.DirectedAssociation,
                        Name = "DuplicateOf",
                        SourceAttributeGUID = duplSource.Item2.AttributeGUID,
                        TargetAttributeGUID = duplDest.Item2.AttributeGUID,
                        SourceElement = duplSource.Item1,
                        TargetElement = duplDest.Item1
                    });
                }
            }



            //for (int i = 0; i < tbl.Rows.Count; i++)
            //{
            //    var entity = tbl.Rows[i]["Entity"].ToString().Trim();
            //    var area = tbl.Rows[i]["Entity Area"].ToString().Trim();

            //    var areaElement = entitiesAndAreas[entity][area];
                
            //}

            _repo.Dispose();

            
        }
    }
}
