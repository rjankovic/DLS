CREATE FUNCTION Annotate.f_ProjectOlapMeasuresLookupTable
(@projectconfigid UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT
e.RefPath, e.ModelElementId MeasureElementId, e.Caption MeasureName, e.Type ElementType
FROM BIDoc.ModelElements e
WHERE e.ProjectConfigId = @projectconfigid
AND e.Type IN (N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement')
AND e.RefPath NOT LIKE 'SSRS%'