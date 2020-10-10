using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.BusinessDictionaryIndex
{
    public class BusinessDictionaryIndex
    {
        private AnnotationManager _annotationManager;
        private Guid _projectConfigId;
        private OlapFieldLookup _olapFieldLookup;
        private Dictionary<string, List<ProjectDictionaryFieldMappingItem>> _fieldsByElementType;
        private Dictionary<int, List<AnnotationViewFieldValue>> _fieldValues;

        public BusinessDictionaryIndex(AnnotationManager annotationManager, Guid projectConfigId)
        {
            _annotationManager = annotationManager;
            _projectConfigId = projectConfigId;

            Refresh();
        }

        public void Refresh()
        {
            _olapFieldLookup = new OlapFieldLookup(_annotationManager, _projectConfigId);
            var projectDictionaryFieldMapping = _annotationManager.ProjectDictionaryFieldsMapping(_projectConfigId);
            _fieldsByElementType = projectDictionaryFieldMapping.GroupBy(x => x.ElementType).ToDictionary(x => x.Key, x => x.OrderBy(y => y.FieldOrder).ToList());
            var fieldValues = _annotationManager.ProjectDictionaryValues(_projectConfigId);
            _fieldValues = fieldValues.GroupBy(x => x.ModelElementId.Value).ToDictionary(x => x.Key, x => x.ToList());
        }

        public OlapFieldLookup OlapFieldLookup { get { return _olapFieldLookup; } }
        //public Dictionary<string, List<ProjectDictionaryFieldMappingItem>> FieldsByElementType { get { return _fieldsByElementType; } }
        //public Dictionary<int, List<AnnotationViewFieldValue>> FieldValues { get { return _fieldValues; } }

        public List<ProjectDictionaryFieldMappingItem> FindFieldsForElementType(string type)
        {
            if (_fieldsByElementType.ContainsKey(type))
            {
                return _fieldsByElementType[type];
            }
            else
            {
                return _fieldsByElementType["Default"];
            }
        }

        public List<AnnotationViewFieldValue> GetFieldValues(int modelElementId)
        {
            if (_fieldValues.ContainsKey(modelElementId))
            {
                return _fieldValues[modelElementId];
            }
            else
            {
                return new List<AnnotationViewFieldValue>();
            }
        }
    }

    public class OlapFieldLookup
    {
        private AnnotationManager _annotationManager;
        private Guid _projectConfigId;
        private List<ProjectOlapAttributesLookupItem> _attributes;
        private List<ProjectOlapMeasuresLookupItem> _measures;
        private Dictionary<string, OlapCubeFieldLookup> _cubesByRefPath = new Dictionary<string, OlapCubeFieldLookup>(StringComparer.OrdinalIgnoreCase);

        public OlapFieldLookup(AnnotationManager annotationManager, Guid projectConfigId)
        {
            _annotationManager = annotationManager;
            _projectConfigId = projectConfigId;

            _attributes = _annotationManager.ProjectOlapAttributesLookupTable(_projectConfigId);
            _measures = _annotationManager.ProjectOlapMeasuresLookupTable(_projectConfigId);

            var attributesByCube = _attributes.GroupBy(x => GetOlapCubeRefPath(x.RefPath));
            var measuresByCube = _measures.GroupBy(x => GetOlapCubeRefPath(x.RefPath));

            foreach (var cubeRefPath in attributesByCube.Select(x => x.Key))
            {
                _cubesByRefPath.Add(cubeRefPath, new OlapCubeFieldLookup(attributesByCube.First(x => x.Key == cubeRefPath).ToList(), measuresByCube.First(x => x.Key == cubeRefPath).ToList()));
            }
        }

        private string GetOlapCubeRefPath(string fieldRefPath)
        {
            var postCubeIndex = Math.Max(
                Math.Max(fieldRefPath.IndexOf("/CubeDimension["), fieldRefPath.IndexOf("/MdxScript")),
                    fieldRefPath.IndexOf("/MeasureGroup")
                );

            return fieldRefPath.Substring(0, postCubeIndex);
        }

        public OlapFieldLookupResult FindOlapField(string cubeRefPath, string fieldName)
        {
            fieldName = fieldName.Trim();

            if (_cubesByRefPath.ContainsKey(cubeRefPath))
            {
                return _cubesByRefPath[cubeRefPath].FindField(fieldName);
            }
            else
            {
                return null;
            }
        }
    }

    public class OlapCubeFieldLookup
    {
        private List<ProjectOlapAttributesLookupItem> _attributes;
        private List<ProjectOlapMeasuresLookupItem> _measures;
        private Dictionary<string, ProjectOlapMeasuresLookupItem> _measuresByName;
        private Dictionary<Tuple<string, string>, ProjectOlapAttributesLookupItem> _dimensionAttributesDictionary;
        private Dictionary<Tuple<string, string, string>, ProjectOlapAttributesLookupItem> _dimensionHierarchiesDictionary = new Dictionary<Tuple<string, string, string>, ProjectOlapAttributesLookupItem>();

        public OlapCubeFieldLookup(List<ProjectOlapAttributesLookupItem> attributes, List<ProjectOlapMeasuresLookupItem> measures)
        {
            this._attributes = attributes;
            this._measures = measures;
            _measuresByName = measures.ToDictionary(x => x.MeasureName, x => x, StringComparer.OrdinalIgnoreCase);
            _dimensionAttributesDictionary = new Dictionary<Tuple<string, string>, ProjectOlapAttributesLookupItem>();
            foreach (var attribute in attributes.Where(x => x.HierarchyLevelName == string.Empty))
            {
                var tuple = new Tuple<string, string>(attribute.CubeDimensionName, attribute.AttributeName);
                _dimensionAttributesDictionary.Add(tuple, attribute);
            }

            _dimensionHierarchiesDictionary = new Dictionary<Tuple<string, string, string>, ProjectOlapAttributesLookupItem>();
            foreach (var attribute in attributes.Where(x => x.HierarchyLevelName != string.Empty))
            {
                var tuple = new Tuple<string, string, string>(attribute.CubeDimensionName, attribute.HierarchyName, attribute.HierarchyLevelName);
                _dimensionHierarchiesDictionary.Add(tuple, attribute);
            }
        }

        public OlapFieldLookupResult FindField(string fieldName)
        {
            try
            {
                if (fieldName.StartsWith("[Measures]"))
                {
                    var measureNameWrap = fieldName.Replace("[Measures].", "");
                    var measureNameInner = measureNameWrap.TrimStart('[').TrimEnd(']');
                    if (_measuresByName.ContainsKey(measureNameInner))
                    {
                        return new OlapFieldLookupResult()
                        {
                            ModelElementId = _measuresByName[measureNameInner].MeasureElementId,
                            ElementType = _measuresByName[measureNameInner].ElementType
                        };
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    var split = fieldName.Split(new string[] { "].[" }, StringSplitOptions.None).Select(x => x.TrimStart('[').TrimEnd(']')).ToList();
                    // just [dimension].[hierarchy = attribute]
                    if (split.Count == 2)
                    {
                        split.Add(split[1]);
                    }
                    // hierarchy == level => direct dimension attribute
                    if (split[1] == split[2])
                    {
                        var tuple1 = new Tuple<string, string>(split[0], split[2]);
                        if (_dimensionAttributesDictionary.ContainsKey(tuple1))
                        {
                            return new OlapFieldLookupResult()
                            {
                                ModelElementId = _dimensionAttributesDictionary[tuple1].DimensionAttributeElementId,
                                ElementType = _dimensionAttributesDictionary[tuple1].ElementType
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        var tuple2 = new Tuple<string, string, string>(split[0], split[1], split[2]);
                        if (_dimensionHierarchiesDictionary.ContainsKey(tuple2))
                        {
                            return new OlapFieldLookupResult()
                            {
                                ModelElementId = _dimensionHierarchiesDictionary[tuple2].DimensionAttributeElementId,
                                ElementType = _dimensionHierarchiesDictionary[tuple2].ElementType
                            };
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConfigManager.Log.Error("Failed to find OLAP field " + fieldName);
                return null;
            }
        }

    }

    public class OlapFieldLookupResult
    {
        public int ModelElementId { get; set; }
        public string ElementType { get; set; }
    }


    /*
     	DECLARE @CubePathEscaped NVARCHAR(MAX) 
	IF LEFT(@fieldName, LEN(N'[Measures]')) = N'[Measures]'
	BEGIN
		DECLARE @FieldNameTrimmed NVARCHAR(MAX) = SUBSTRING(@FieldName, LEN('[Measures].[') + 1, 1000)
		SET @FieldNameTrimmed = REPLACE(LEFT(@FieldNameTrimmed, LEN(@FieldNameTrimmed) - 1), '_', '\_')

		SELECT e.RefPath, e.ProjectConfigId, e.ModelElementId 
		FROM BIDoc.ModelElements e 
		WHERE 
		LEFT(e.RefPath, LEN(@cubePath)) = @cubePath 
		AND e.Type 
		IN (N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement', N'CD.DLS.Model.Mssql.Ssas.CalculatedMeasureElement') 
		AND e.Caption = @FieldNameTrimmed

	END
	ELSE
	BEGIN
		DECLARE @dimName NVARCHAR(MAX) = (SELECT item FROM adm.f_SplitString(@fieldName, N'.') x WHERE rwn = 1)
		DECLARE @hierName NVARCHAR(MAX) = (SELECT item FROM adm.f_SplitString(@fieldName, N'.') x WHERE rwn = 2)
		DECLARE @levelName NVARCHAR(MAX) = (SELECT item FROM adm.f_SplitString(@fieldName, N'.') x WHERE rwn = 3)
	
	
		 IF LEFT(@dimName, 1) = N'['
			SET @dimName = RIGHT(LEFT(@dimName, LEN(@dimName)-1), LEN(@dimName)-2)
		IF LEFT(@hierName, 1) = N'['
			SET @hierName = RIGHT(LEFT(@hierName, LEN(@hierName)-1), LEN(@hierName)-2)
		IF LEFT(@levelName, 1) = N'['
			SET @levelName = RIGHT(LEFT(@levelName, LEN(@levelName)-1), LEN(@levelName)-2)

		--DECLARE @dimPath NVARCHAR(MAX) =@cubePath + N'/CubeDimension[@Name=''' + @dimName + N''']'
		--SELECT @dimPath

		IF @hierName = @levelName
		BEGIN
		SELECT hle.RefPath, hle.ProjectConfigId, hle.ModelElementId 
		FROM BIDoc.ModelElements e 
		INNER JOIN BIDoc.ModelLinks hll ON hll.ElementToId = e.ModelElementId AND hll.Type = N'parent'
		INNER JOIN BIDoc.ModelElements hle ON hle.ModelElementId = hll.ElementFromId

		WHERE 
		LEFT(e.RefPath, LEN(@cubePath)) = @cubePath 
		AND e.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement') 
		AND e.Caption = @dimName
		AND hle.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement') 
		AND hle.Caption = @hierName
		END

		ELSE
		BEGIN
		
		SELECT dae.RefPath, dae.ProjectConfigId, dae.ModelElementId
		FROM BIDoc.ModelElements cde 		
		-- database dimension
		INNER JOIN BIDoc.ModelLinks ddl ON ddl.ElementFromId = cde.ModelElementId AND ddl.Type = N'DatabaseDimension'
		-- children (attributes and hierarchies)
		INNER JOIN BIDoc.ModelLinks hl ON hl.ElementToId = ddl.ElementToId AND hl.Type = N'parent'
		INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementFromId
		-- children (hierarchy levels)
		INNER JOIN BIDoc.ModelLinks hll ON hll.ElementToId = he.ModelElementId AND hll.Type = N'parent'
		INNER JOIN BIDoc.ModelElements hle ON hle.ModelElementId = hll.ElementFromId
		-- link to dimension attribute
		INNER JOIN BIDoc.ModelLinks dal ON dal.ElementFromId = hle.ModelElementId AND dal.Type = N'Attribute'
		INNER JOIN BIDoc.ModelElements dae ON dae.ModelElementId = dal.ElementToId
				
		WHERE 	
			LEFT(cde.RefPath, LEN(@cubePath)) = @cubePath 		
			AND cde.Type = N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement' 
			AND cde.Caption = @dimName
			AND he.Type = N'CD.DLS.Model.Mssql.Ssas.HierarchyElement'
			AND he.Caption = @hierName
			AND hle.Type = N'CD.DLS.Model.Mssql.Ssas.HierarchyLevelElement'
			AND hle.Caption = @levelName
			AND dae.Type = N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement'		
		END		
	END

     */
}
