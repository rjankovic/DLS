CREATE PROCEDURE [BIDoc].[sp_GetGraphNodesUnderPath]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@graphkind NVARCHAR(50),
	@path NVARCHAR(MAX),
	@nodeType NVARCHAR(200) = NULL
--)
AS
--RETURNS TABLE AS RETURN
--(

DECLARE @nodeId INT = (SELECT BasicGraphNodeId FROM [BIDoc].[f_GetGraphNodeIdByRefPath](@projectConfigId, @graphkind, @path))
DECLARE 
	@intervalFrom INT,
	@intervalTo INT

SELECT @intervalFrom = n.RefPathIntervalStart, @intervalTo = n.RefPathIntervalEnd FROM BIDoc.BasicGraphNodes n WHERE n.BasicGraphNodeId = @nodeId

SELECT n.[BasicGraphNodeId]
      ,n.[Name]
      ,n.[NodeType]
      ,n.[Description]
      ,n.[ParentId]
      ,n.[GraphKind]
      ,n.[ProjectConfigId]
      ,n.[SourceElementId]
      ,n.[TopologicalOrder]
	  ,e.[RefPath]
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId 
WHERE GraphKind = @graphkind AND n.ProjectConfigId = @projectconfigid --AND LEFT(e.RefPath, LEN(@path)) = @path
AND (@nodeType IS NULL OR @nodeType = n.NodeType)
AND n.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo

--)
