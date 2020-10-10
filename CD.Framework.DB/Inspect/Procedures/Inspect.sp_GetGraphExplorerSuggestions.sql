CREATE PROCEDURE [Inspect].[sp_GetGraphExplorerSuggestions]
(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@pattern NVARCHAR(200) = NULL
)
AS

;WITH priorities AS
  (
  SELECT NodeType, Priority
  FROM 
  (
  VALUES
  ('SchemaTableElement', 1),
  ('ReportElement', 2),
  ('PhysicalMeasureElement', 3),
  ('CubeCalculatedMeasureElement', 4),
  ('ReportCalculatedMeasureElement', 5),
  ('PackageElement', 6),
  ('ProcedureElement', 7),
  ('MeasureGroupElement', 8),
  ('CubeElement', 9),
  ('CubeDimensionElement', 10),
  ('DimensionElement', 11),
  ('DatabaseElement', 12),
  ('ServerElement', 13)) AS x(NodeType, Priority)
  )
SELECT n.[BasicGraphNodeId]
      ,n.[Name]
      ,n.[NodeType]
      ,NULL [Description]
      ,n.[ParentId]
      ,n.[GraphKind]
      ,n.[ProjectConfigId]
      ,n.[SourceElementId]
      ,n.[TopologicalOrder]
	  ,e.[RefPath]
	  ,td.TypeDescription
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.Type
INNER JOIN priorities p ON p.NodeType = n.NodeType
WHERE GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid 
AND (@pattern IS NULL OR n.Name LIKE '%' + Adm.f_EscapeForLike(@pattern) + '%')
ORDER BY p.Priority
