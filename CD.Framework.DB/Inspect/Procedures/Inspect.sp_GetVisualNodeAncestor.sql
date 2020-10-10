CREATE PROCEDURE [Inspect].[sp_GetVisualNodeAncestor]
@nodeId INT

AS

DECLARE @nodeType NVARCHAR(200)
DECLARE @parentId INT
DECLARE @refPath NVARCHAR(MAX)
SELECT @nodeType = NodeType, @parentId = ParentId, @refPath = e.RefPath 
FROM BIDoc.BasicGraphNodes n 
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE n.BasicGraphNodeId = @nodeId

IF(@nodeType IN (N'ColumnElement', N'MdxFragmentElement', 
	N'SsisExpressionFragmentElement'/*, N'SsrsExpressionFragmentElement'*/))
BEGIN
	SELECT @parentId
END
ELSE IF @nodeType IN(N'CubeCalculatedMeasureElement', N'ReportCalculatedMeasureElement', N'PackageElement', N'SchemaTableElement', N'ProcedureElement', N'ViewElement', N'ReportElement')
BEGIN
	SELECT @nodeId
END
ELSE IF @nodeType IN (N'SqlScriptElement', N'SqlDmlSourceElement', N'SqlDmlTargetReferenceElement')
BEGIN
	;WITH ancestors AS
	(
	SELECT @nodeId NodeId, @parentId ParentId, @refPath RefPath
	UNION ALL
	SELECT n.BasicGraphNodeId NodeId, n.ParentId, e.RefPath
	FROM BIDoc.BasicGraphNodes n
	INNER JOIN ancestors a ON n.BasicGraphNodeId = a.ParentId
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
	WHERE n.NodeType IN (N'SqlScriptElement', N'SqlDmlSourceElement', N'SqlDmlTargetReferenceElement')
	)
	SELECT TOP 1 NodeId 
	FROM ancestors
	ORDER BY RefPath
END
ELSE IF @nodeType IN (N'DfColumnElement')
BEGIN
	;WITH ancestors AS
	(
	SELECT @nodeId NodeId, @parentId ParentId, @refPath RefPath, @nodeType NodeType
	UNION ALL
	SELECT n.BasicGraphNodeId NodeId, n.ParentId, e.RefPath, n.NodeType
	FROM BIDoc.BasicGraphNodes n
	INNER JOIN ancestors a ON n.BasicGraphNodeId = a.ParentId
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
	)
	SELECT NodeId 
	FROM ancestors
	WHERE ancestors.NodeType = N'PackageElement'
END
ELSE IF @nodeType IN (N'SsrsExpressionFragmentElement', N'TextBoxElement')
BEGIN
	;WITH ancestors AS
	(
	SELECT @nodeId NodeId, @parentId ParentId, @refPath RefPath, @nodeType NodeType
	UNION ALL
	SELECT n.BasicGraphNodeId NodeId, n.ParentId, e.RefPath, n.NodeType
	FROM BIDoc.BasicGraphNodes n
	INNER JOIN ancestors a ON n.BasicGraphNodeId = a.ParentId
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
	)
	SELECT NodeId 
	FROM ancestors
	WHERE ancestors.NodeType = N'ReportElement'
END
ELSE IF @refPath LIKE N'%]/DaxScript%'
BEGIN
	;WITH ancestors AS
	(
	SELECT @nodeId NodeId, @parentId ParentId, @refPath RefPath, @nodeType NodeType
	UNION ALL
	SELECT n.BasicGraphNodeId NodeId, n.ParentId, e.RefPath, n.NodeType
	FROM BIDoc.BasicGraphNodes n
	INNER JOIN ancestors a ON n.BasicGraphNodeId = a.ParentId
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
	)
	SELECT NodeId 
	FROM ancestors
	WHERE ancestors.NodeType = N'DaxScriptElement'
END
BEGIN
	SELECT NULL
END
