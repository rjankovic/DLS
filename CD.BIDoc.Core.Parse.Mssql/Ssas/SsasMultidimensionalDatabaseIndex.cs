using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssas;
using System.Text.RegularExpressions;
using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Serialization;
using CD.DLS.Common.Structures;
using CD.DLS.DAL.Managers;
using CD.DLS.Model.Mssql;
using CD.DLS.Model;

namespace CD.DLS.Parse.Mssql.Ssas
{

    public class MdxStatementIndex
    {
        protected List<Dictionary<string, SsasModelElement>> _localReferrables = new List<Dictionary<string, SsasModelElement>>();
        private Regex _measuresRegex = new Regex("^\\[(?i)measures].");
        
        public CubeIndex CubeIndex { get; set; }

        public MdxStatementIndex()
        {
            CubeIndex = new CubeIndex();
        }

        public SsasModelElement TryResolveIdentifier(int axis, string identifier, bool globalPriority, out int realLength)
        {
            if (globalPriority)
            {
                var globalResolution = CubeIndex.TryResolveIdentifier(identifier, out realLength);
                if (globalResolution != null)
                {
                    return globalResolution;
                }
            }
            
            if (_localReferrables.Count <= axis)
            {
                if (!globalPriority)
                {
                    var globalResolution = CubeIndex.TryResolveIdentifier(identifier, out realLength);
                    if (globalResolution != null)
                    {
                        return globalResolution;
                    }
                }

                realLength = 0;
                return null;
            }
            
            List<string> incrementalPrefixes;
            var wrappedIdentifier = MdxHelper.WrapIdentifier(identifier, out incrementalPrefixes);
            incrementalPrefixes.Add(wrappedIdentifier);
            for (int i = incrementalPrefixes.Count - 1; i >= 0; i--)
            {
                var prefix = incrementalPrefixes[i];
                if (_localReferrables[axis].ContainsKey(prefix))
                {
                    realLength = prefix.Length;
                    return _localReferrables[axis][prefix];
                }
            }
            var localRefWoWrap = _localReferrables[axis].FirstOrDefault(x => "[measures].[" + x.Key.ToLower() + "]" == wrappedIdentifier.ToLower());
            if (localRefWoWrap.Value != null)
            {
                realLength = localRefWoWrap.Key.Length;
                return localRefWoWrap.Value;
            }

            if (incrementalPrefixes.Count == 1)
            {
                return TryResolveIdentifier(axis, "[Measures]." + identifier, false, out realLength);
            }

            realLength = 0;

            if (!globalPriority)
            {
                var globalResolution = CubeIndex.TryResolveIdentifier(identifier, out realLength);
                if (globalResolution != null)
                {
                    return globalResolution;
                }
            }

            return null;
        }
        
        public void AddElementOnAxis(MdxElement element, int axis)
        {
            while (_localReferrables.Count <= axis)
            {
                _localReferrables.Add(new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase));
            }
            if (_localReferrables[axis].ContainsKey(element.Caption))
            {
                ConfigManager.Log.Warning("Duplicate axis member: {0}", element.Caption);
            }
            else
            {
                _localReferrables[axis].Add(element.Caption, element);
            }
        }

        public void AddMeasureOnAxis(MdxElement element, int axis)
        {
            while (_localReferrables.Count <= axis)
            {
                _localReferrables.Add(new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase));
            }
            _localReferrables[axis].Add(string.Format("[Measures].[{0}]",  element.Caption), element);
        }

        //public Dictionary<string, SsasModelElement> Columns { get { return _localReferrables[0]; } }
    }

    public class CubeIndex
    {
        private Dictionary<string, SsasModelElement> _referrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, SsasModelElement> _localReferrablesDictionary = new Dictionary<string, SsasModelElement>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, MdxScriptElement> _scriptsDictionary = new Dictionary<string, MdxScriptElement>(StringComparer.OrdinalIgnoreCase);
        private Regex _measuresRegex = new Regex("^\\[(?i)measures].");
        private ProjectConfig _projectConfig;
        private GraphManager _graphManager;
        private Dictionary<MssqlModelElement, int> _premappedElements = new Dictionary<MssqlModelElement, int>();
        public Dictionary<MssqlModelElement, int> PremappedElements { get { return _premappedElements; } }

        private void AddMappedElement(MssqlModelElement e)
        {
            if (!_premappedElements.ContainsKey(e))
            {
                _premappedElements.Add(e, e.Id);
            }
        }

        public CubeIndex()
        {
        }

        public CubeIndex(CubeElement cubeElement, ProjectConfig projectConfig, GraphManager graphManager)
        {
            _graphManager = graphManager;
            _projectConfig = projectConfig;
            CubeElement cube = cubeElement;
            if (!cube.Children.Any())
            {
                SerializationHelper sh = new SerializationHelper(_projectConfig, _graphManager);
                var db = (SsasMultidimensionalDatabaseElement)sh.LoadElementModel(cubeElement.Parent.RefPath.Path);
                cube = db.Cubes.First(x => x.Caption == cubeElement.Caption); // (CubeElement)sh.LoadElementModel(cubeElement.RefPath.Path);
            }

            foreach (var mg in cube.MeasureGroups)
            {
                foreach (var measure in mg.Measures)
                {
                    AddMeasure(measure);
                }
            }

            foreach (var calcMeasure in cube.CalculatedMeasures)
            {
                AddMeasure(calcMeasure);
            }

            foreach (var cubeDimension in cube.Dimensions)
            {
                AddCubeDimension(cubeDimension);
            }
        }

        
        public virtual SsasModelElement TryResolveIdentifier(string identifier, out int realLength)
        {
            List<string> incrementalPrefixes;
            var wrappedIdentifier = MdxHelper.WrapIdentifier(identifier, out incrementalPrefixes);
            incrementalPrefixes.Add(wrappedIdentifier);
            for (int i = incrementalPrefixes.Count - 1; i >= 0; i--)
            {
                var prefix = incrementalPrefixes[i];
                if (_localReferrablesDictionary.ContainsKey(prefix))
                {
                    realLength = prefix.Length;
                    return _localReferrablesDictionary[prefix];
                }
                if (_referrablesDictionary.ContainsKey(prefix))
                {
                    realLength = prefix.Length;
                    return _referrablesDictionary[prefix];
                }
            }
            if (incrementalPrefixes.Count == 1)
            {
                return TryResolveIdentifier("[Measures]." + identifier, out realLength);
            }

            realLength = 0;
            return null;
        }
        
        private void AddMeasure(MeasureElement measure)
        {
            _referrablesDictionary.Add(string.Format("[Measures].[{0}]", measure.Caption), measure);
            AddMappedElement(measure);
        }

        // public - MDX script extractor needs this for SSRS
        public void AddMeasure(CalculatedMeasureElement measure)
        {
            _referrablesDictionary.Add(string.Format("[Measures].[{0}]", measure.Caption), measure);
            AddMappedElement(measure);
        }

        public void AddLocalMeasure(CalculatedMeasureElement measure)
        {
            _localReferrablesDictionary.Add(string.Format("[Measures].[{0}]", measure.Caption), measure);
            //AddMappedElement(measure);
        }
        
        public void ClearLocalIndexes()
        {
            _localReferrablesDictionary = new Dictionary<string, SsasModelElement>();
            _scriptsDictionary = new Dictionary<string, MdxScriptElement>();
        }


        private void AddCubeDimension(CubeDimensionElement cubeDimension)
        {
            var dimensionPrefix = MdxHelper.WrapIdentifier(cubeDimension.Caption);

            _referrablesDictionary.Add(dimensionPrefix, cubeDimension);
            AddMappedElement(cubeDimension);
            foreach (CubeDimensionHierarchyElement hierarchy in cubeDimension.Hierarchies)
            {
                var hierarchyIdentifier = string.Format("{0}.{1}", dimensionPrefix, MdxHelper.WrapIdentifier(hierarchy.Caption));
                _referrablesDictionary.Add(hierarchyIdentifier, hierarchy);
                AddMappedElement(hierarchy);
                foreach (var level in hierarchy.Levles)
                {
                    var levelIdentifier = string.Format("{0}.{1}", hierarchyIdentifier, MdxHelper.WrapIdentifier(level.Caption));
                    _referrablesDictionary.Add(levelIdentifier, level);
                    AddMappedElement(level);
                }
            }

            foreach (var attribute in cubeDimension.Attributes)
            {
                if (attribute.DatabaseDimensionAttribute.AttributeHierarchyEnabled)
                {
                    var attrHierarchyIdentifier = string.Format("{0}.{1}", dimensionPrefix, MdxHelper.WrapIdentifier(attribute.Caption));
                    _referrablesDictionary.Add(attrHierarchyIdentifier, attribute);
                    //if (dbDimension.Caption == "Building")
                    //{
                    //    ConfigManager.Log.Info("Adding mapped attribute " + attribute.RefPath.Path);
                    //}
                    AddMappedElement(attribute);
                }

                // include related attributes in CUBE dim attributes?
                foreach (var relatedAttribute in attribute.DatabaseDimensionAttribute.RelatedAttributes)
                {
                    var memberPropertyIdentifier = string.Format("{0}.{1}.{2}", dimensionPrefix, MdxHelper.WrapIdentifier(attribute.Caption), MdxHelper.WrapIdentifier(relatedAttribute.RelatedAttribute.Caption));
                    _referrablesDictionary.Add(memberPropertyIdentifier, relatedAttribute.RelatedAttribute);
                    //if (dbDimension.Caption == "Building")
                    //{
                    //    ConfigManager.Log.Info("Adding mapped attribute " + relatedAttribute.RefPath.Path);
                    //}
                    AddMappedElement(relatedAttribute.RelatedAttribute);
                }
            }
        }
        
        public void AddScript(MdxScriptElement scriptElement)
        {
            _scriptsDictionary.Add(scriptElement.Caption, scriptElement);
        }

        public MdxScriptElement FindScript(string name)
        {
            return _scriptsDictionary[name];
        }
    }
    
    /// <summary>
    /// Resolves references to SSAS DB objects, used for both local (script-wide) and global elements
    /// </summary>
    public class SsasMultidimensionalDatabaseIndex : SsasDatabaseIndex
    {

        private string _cubeInUse;
        public string CubeInUse {
            get { return _cubeInUse; }
            set { _cubeInUse = value; LoadCubeIndex(CubeInUse); }
        }
        private Dictionary<string, CubeElement> _cubeElementDictionary = new Dictionary<string, CubeElement>();
        private Dictionary<string, CubeIndex> _cubeIndexDictionary = new Dictionary<string, CubeIndex>();
        private ProjectConfig _projectConfig;
        private GraphManager _graphManager;

        public override Dictionary<MssqlModelElement, int> GetPremappedIds()
        {
            Dictionary<MssqlModelElement, int> res = new Dictionary<MssqlModelElement, int>();
            foreach (var cubeIdx in _cubeIndexDictionary.Values)
            {
                foreach (var kv in cubeIdx.PremappedElements)
                {
                    res.Add(kv.Key, kv.Value);
                }
            }

            return res;
        }

        public SsasModelElement TryResolveIdentifier(string identifier)
        {
            var translatedIdentifier = identifier;
            if (QueryMode == SsasQueryMode.DAX)
            {
                translatedIdentifier = TranslateDaxIdentifier(identifier);
            }
            int dummy;
            return _cubeIndexDictionary[CubeInUse].TryResolveIdentifier(translatedIdentifier, out dummy);
        }
        
        public SsasModelElement TryResolveIdentifier(string identifier, out int realLength)
        {
            var translatedIdentifier = identifier;
            if (QueryMode == SsasQueryMode.DAX)
            {
                translatedIdentifier = TranslateDaxIdentifier(identifier);
            }
            return _cubeIndexDictionary[CubeInUse].TryResolveIdentifier(translatedIdentifier, out realLength);
        }

        public void AddLocalMeasure(CalculatedMeasureElement measure)
        {
            _cubeIndexDictionary[CubeInUse].AddLocalMeasure(measure);
            //_cubeIndexDictionary[CubeInUse].AddMeasure(measure);
        }

        public void AddMeasure(CalculatedMeasureElement measure)
        {
            _cubeIndexDictionary[CubeInUse].AddMeasure(measure);
        }

        public void AddScript(MdxScriptElement scriptElement)
        {
            _cubeIndexDictionary[CubeInUse].AddScript(scriptElement);
        }

        public MdxScriptElement FindScript(string name)
        {
            return _cubeIndexDictionary[CubeInUse].FindScript(name);
        }

        public override void ClearLocalIndexes()
        {
            foreach (var cube in _cubeIndexDictionary.Values)
            {
                cube.ClearLocalIndexes();
            }
        }

        public CubeIndex GetCubeIndex(string cubeName)
        {
            LoadCubeIndex(cubeName);
            return _cubeIndexDictionary[cubeName];
        }
        
        //private void Cleanup()
        //{
        //    _cubeIndexDictionary = new Dictionary<string, CubeIndex>(StringComparer.OrdinalIgnoreCase);
        //}


        public SsasMultidimensionalDatabaseIndex(CubeElement cubeElement, GraphManager graphManager, ProjectConfig projectConfig)
        {
            QueryMode = SsasQueryMode.MDX;
            _ssasType = SsasTypeEnum.Multidimensional;

            _projectConfig = projectConfig;
            _graphManager = graphManager;
            _cubeIndexDictionary = new Dictionary<string, CubeIndex>();
            _cubeElementDictionary.Add(cubeElement.Caption, cubeElement);
            //AddCube(cubeElement);
            CubeInUse = cubeElement.Caption;
        }

        public static string TranslateDaxIdentifier(string daxId)
        {
            var trim = daxId.Trim();

            if (trim.StartsWith("["))
            {
                return string.Format("[Measures].{0}", trim);
            }
            else if (trim.StartsWith("'"))
            {
                var tableEnd = trim.IndexOf('\'', 1);
                if (tableEnd == -1)
                {
                    return trim;
                }

                var tableName = trim.Substring(1, tableEnd - 1);

                if (trim.EndsWith("]"))
                {
                    var columnStart = trim.LastIndexOf("[");
                    if (columnStart == -1)
                    {
                        return trim;
                    }

                    var columnName = trim.Substring(columnStart + 1).TrimEnd(']');

                    return string.Format("[{0}].[{1}].[{1}]", tableName, columnName);
                }
                else
                {
                    return string.Format("[{0}].[{1}]", tableName, tableName);
                }
            }
            else
            {
                return string.Format("[Measures].[{0}]", trim);
            }
        }

        private void LoadCubeIndex(string cubeName)
        {
            if (_cubeIndexDictionary.ContainsKey(cubeName))
            {
                return;
            }

            if (!_cubeElementDictionary.ContainsKey(cubeName))
            {
                _cubeIndexDictionary.Add(cubeName, new CubeIndex());
            }
            else
            {
                _cubeIndexDictionary.Add(cubeName, 
                    new CubeIndex(_cubeElementDictionary[cubeName], 
                    _projectConfig, _graphManager));
            }
        }

        public SsasMultidimensionalDatabaseIndex(SsasMultidimensionalDatabaseElement dbElement, GraphManager graphManager, ProjectConfig projectConfig)
        {
            //foreach (var cube in dbElement.Cubes)
            //{
            //    AddCube(cube);
            //}
            QueryMode = SsasQueryMode.MDX;
            _ssasType = SsasTypeEnum.Multidimensional;
            
            _projectConfig = projectConfig;
            _graphManager = graphManager;
            LoadCubeElementDictionary(dbElement);
        }

        private void LoadCubeElementDictionary(SsasMultidimensionalDatabaseElement dbElement)
        {
            SerializationHelper sh = new SerializationHelper(_projectConfig, _graphManager);
            var dbWithCubes = (SsasMultidimensionalDatabaseElement)sh.LoadElementModelToChildrenOfType(dbElement.RefPath.Path, typeof(CubeElement));
            _cubeElementDictionary = new Dictionary<string, CubeElement>();
            foreach (CubeElement cube in dbWithCubes.Cubes)
            {
                _cubeElementDictionary.Add(cube.Caption, cube);
            }
        }
        
    }
    

}
