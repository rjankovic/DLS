CREATE FUNCTION Annotate.f_ProjectOlapAttributesLookupTable
(@projectconfigid UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT DISTINCT

e.RefPath, e.ModelElementId CubeAttributeElementId, da.ModelElementId DimensionAttributeElementId,
	e.Caption AttributeName, cde.Caption CubeDimensionName, dde.Caption DatabaseDimensionName, dhe.Caption HierarchyName, hle.Caption HierarchyLevelName, da.Type ElementType

FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks dal ON dal.ElementFromId = e.ModelElementId AND dal.Type = N'DatabaseDimensionAttribute'
INNER JOIN BIDoc.ModelElements da ON da.ModelElementId = dal.ElementToId
INNER JOIN BIDoc.ModelLinks cdl ON cdl.ElementFromId = e.ModelElementId AND cdl.Type = N'parent'
INNER JOIN BIDoc.ModelElements cde ON cde.ModelElementId = cdl.ElementToId
INNER JOIN BIDoc.ModelLinks ddl ON ddl.ElementFromId = cde.ModelElementId AND ddl.Type = N'DatabaseDimension'
INNER JOIN BIDoc.ModelElements dde ON ddl.ElementToId = dde.ModelElementId
-- hierarchies
LEFT JOIN BIDoc.ModelLinks dhl ON dhl.ElementToId = dde.ModelElementId AND dhl.Type = N'parent'
LEFT JOIN BIDoc.ModelElements dhe ON dhe.ModelElementId = dhl.ElementFromId AND dhe.Type = N'CD.DLS.Model.Mssql.Ssas.HierarchyElement'
-- hierarchy levels
LEFT JOIN BIDoc.ModelLinks hll ON hll.ElementToId = dhe.ModelElementId AND hll.Type = N'parent'
LEFT JOIN BIDoc.ModelElements hle ON hle.ModelElementId = hll.ElementFromId
-- level attributes
LEFT JOIN BIDoc.ModelLinks hlal ON hlal.ElementFromId = hle.ModelElementId AND hlal.Type = N'Attribute'

WHERE e.ProjectConfigId = @projectconfigid
AND e.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement')
AND ISNULL(hlal.ElementToId, da.ModelElementId) = da.ModelElementId
