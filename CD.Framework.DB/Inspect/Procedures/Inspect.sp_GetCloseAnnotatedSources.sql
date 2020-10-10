CREATE PROCEDURE [Inspect].[sp_GetCloseAnnotatedSources]
	@modelElementId INT
AS

DECLARE @nodeId INT = (SELECT BasicGraphNodeId FROM BIDoc.BasicGraphNodes WHERE SourceElementId = @modelElementId AND GraphKind = N'DataFlow')

DECLARE @currentNodes TABLE(NodeId INT, ModelElementId INT)
DECLARE @nextNodes TABLE(NodeId INT, ModelElementId INT)

/*
;WITH descendants AS
(
SELECT @nodeId NodeId, @modelElementId ModelElementId

UNION ALL

SELECT n.BasicGraphNodeId NodeId, n.SourceElementId ModelElementId 
FROM BIDoc.BasicGraphNodes n
INNER JOIN descendants d ON d.NodeId = n.ParentId
)
*/
INSERT INTO @currentNodes(NodeId, ModelElementId)
--SELECT NodeId, ModelElementId FROM descendants
--OPTION (MAXRECURSION 1000)
VALUES(@nodeId, @modelElementId)

DECLARE @stepsLeft INT = 7
DECLARE @currentNodesCount INT = 1

WHILE @stepsLeft > 0 AND @currentNodesCount > 0
BEGIN
	DELETE FROM @nextNodes
	INSERT INTO @nextNodes (NodeId, ModelElementId)
		SELECT DISTINCT n.BasicGraphNodeId, n.SourceElementId
		FROM @currentNodes c
		INNER JOIN BIDoc.BasicGraphLinks l ON l.LinkType = N'DataFlow' AND l.NodeToId = c.NodeId
		INNER JOIN BIDoc.BasicGraphNodes n ON n.BasicGraphNodeId = l.NodeFromId

	DELETE FROM @currentNodes
	INSERT INTO @currentNodes(NodeId, ModelElementId)
		SELECT NodeId, ModelElementId FROM @nextNodes

	--SELECT * FROM @currentNodes

	-- annotations found at this level
	IF EXISTS(
		SELECT TOP 1 1 FROM @currentNodes c
		INNER JOIN Annotate.AnnotationElements ae ON ae.ModelElementId = c.ModelElementId
		INNER JOIN Annotate.FieldValues fv ON fv.AnnotationElementId = ae.AnnotationElementId
		WHERE ae.IsCurrentVersion = 1 AND fv.[Value] <> N''
	)
	BEGIN
		SELECT e.ModelElementId, e.Caption ModelElementName, td.TypeDescription, f.FieldName, fv.[Value] FieldValue 
		FROM @currentNodes c
		INNER JOIN BIDoc.ModelElements e ON c.ModelElementId = e.ModelElementId
		INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = e.Type
		INNER JOIN Annotate.AnnotationElements ae ON ae.ModelElementId = e.ModelElementId
		INNER JOIN Annotate.FieldValues fv ON fv.AnnotationElementId = ae.AnnotationElementId
		INNER JOIN Annotate.Fields f ON f.FieldId = fv.FieldId
		WHERE ae.IsCurrentVersion = 1
		
		RETURN
	END

	SET @stepsLeft = @stepsLeft - 1
END

SELECT TOP 0 0 [0]

