CREATE PROCEDURE [BIDoc].[sp_BuildHighLevelGraph]
	--DECLARE 
	@projectconfigid UNIQUEIDENTIFIER --= N'e99a3b4e-7f04-4b98-9780-10e71e6258cf'
AS

DECLARE @grphDf NVARCHAR(50) = N'DataFlow'
DECLARE @grphHigh NVARCHAR(50) = N'DFHigh'
DECLARE @grphMid NVARCHAR(50) = N'DFMedium'


DECLARE @highNodes TABLE
(
 SourceGraphNodeId INT,
 [Name] NVARCHAR(MAX),
 NodeType NVARCHAR(MAX),
 Description NVARCHAR(MAX),
 ParentId INT NULL,
 SourceElementId INT
)


DELETE l 
FROM BIDoc.BasicGraphLinks l
INNER JOIN BIDoc.BasicGraphNodes n ON l.NodeFromId = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind IN (@grphHigh/*, @grphMid*/)

DELETE FROM BIDoc.BasicGraphNodes
WHERE ProjectConfigId = @projectconfigid AND GraphKind IN (@grphHigh/*, @grphMid*/)

-- the nodes to be included

INSERT INTO @highNodes(SourceGraphNodeId, [Name], NodeType, Description, ParentId, SourceElementId) 
SELECT n.BasicGraphNodeId, n.Name, n.NodeType, n.Name, NULL ParentId, n.SourceElementId
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE e.Type = N'CD.DLS.Model.Mssql.Db.DatabaseElement' AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @grphDf


INSERT INTO @highNodes(SourceGraphNodeId, [Name], NodeType, Description, ParentId, SourceElementId) 
SELECT n.BasicGraphNodeId, n.Name, n.NodeType, fld.Name + N'/' + n.Name, NULL ParentId, n.SourceElementId
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.BasicGraphNodes fld ON fld.BasicGraphNodeId = n.ParentId
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssis.ProjectElement' AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @grphDf

INSERT INTO @highNodes(SourceGraphNodeId, [Name], NodeType, Description, ParentId, SourceElementId) 
SELECT n.BasicGraphNodeId, n.Name, n.NodeType, n.Name Description, NULL ParentId, n.SourceElementId
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.BasicGraphNodes fld ON fld.BasicGraphNodeId = n.ParentId
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssas.CubeElement' AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @grphDf

INSERT INTO @highNodes(SourceGraphNodeId, [Name], NodeType, Description, ParentId, SourceElementId) 
SELECT n.BasicGraphNodeId, n.Name, n.NodeType, n.Name Description, NULL ParentId, n.SourceElementId
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.BasicGraphNodes fld ON fld.BasicGraphNodeId = n.ParentId
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
WHERE e.Type = N'CD.DLS.Model.Mssql.Pbi.TenantElement' AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @grphDf


;WITH ssrsFolders AS
(
SELECT n.BasicGraphNodeId, n.Name, n.NodeType, n.Name Description, NULL ParentId, n.SourceElementId, n.ParentId CurrentParent
FROM BIDoc.BasicGraphNodes n
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = n.SourceElementId
LEFT JOIN BIDoc.BasicGraphNodes chf ON chf.ParentId = n.BasicGraphNodeId AND chf.NodeType = N'FolderElement'
WHERE e.Type = N'CD.DLS.Model.Mssql.Ssrs.FolderElement' AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @grphDf AND chf.BasicGraphNodeId IS NULL

UNION ALL

SELECT f.BasicGraphNodeId, f.Name, f.NodeType, p.Name + N'/' + f.Description, f.ParentId, f.SourceElementId, p.ParentId
FROM ssrsFolders f
INNER JOIN BIDoc.BasicGraphNodes p ON p.BasicGraphNodeId = f.CurrentParent
)
INSERT INTO @highNodes(SourceGraphNodeId, [Name], NodeType, Description, ParentId, SourceElementId) 
SELECT BasicGraphNodeId, Name, NodeType, REPLACE(Description, N'//', N'') Description,ParentId, SourceElementId 
FROM ssrsFolders WHERE Description LIKE '/%'


INSERT INTO BIDoc.BasicGraphNodes(Name, NodeType, Description, ParentId, GraphKind, ProjectConfigId, SourceElementId)
SELECT DISTINCT Name, NodeType, Description, ParentId, @grphHigh, @projectconfigid, SourceElementId FROM @highNodes


;WITH descendantMap AS
(
SELECT hn.SourceGraphNodeId HighNode, hn.SourceGraphNodeId LowNode FROM @highNodes hn

UNION ALL

SELECT m.HighNode, c.BasicGraphNodeId LowNode FROM descendantMap m
INNER JOIN BIDoc.BasicGraphNodes c ON c.ParentId = m.LowNode
)
,links AS(
SELECT src.HighNode SrcSrcNode, tgt.HighNode TgtSrcNode, COUNT(*) Strength 
FROM descendantMap src
INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = src.LowNode
INNER JOIN descendantMap tgt ON l.NodeToId = tgt.LowNode
WHERE l.LinkType = N'DataFlow' AND src.HighNode <> tgt.HighNode
GROUP BY src.HighNode, tgt.HighNode

)
INSERT INTO BIDoc.BasicGraphLinks
(NodeFromId, NodeToId, LinkType, ExtendedProperties)
SELECT 
--*
srcn.BasicGraphNodeId, tgtn.BasicGraphNodeId, N'DataFlow', N'{"Strength":' + CONVERT(NVARCHAR(20), l.Strength) + N'}'
FROM links l
INNER JOIN @highNodes srcHn ON srcHn.SourceGraphNodeId = l.SrcSrcNode
INNER JOIN @highNodes tgtHn ON tgtHn.SourceGraphNodeId = l.TgtSrcNode
INNER JOIN BIDoc.BasicGraphNodes srcn ON srcn.SourceElementId = srcHn.SourceElementId AND srcn.GraphKind = @grphHigh
INNER JOIN BIDoc.BasicGraphNodes tgtn ON tgtn.SourceElementId = tgtHn.SourceElementId AND tgtn.GraphKind = @grphHigh
OPTION(MAXRECURSION 10000)

--SELECT * FROM @highNodes

-- delete nodes without links
DELETE n FROM BIDoc.BasicGraphNodes n
LEFT JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = n.BasicGraphNodeId
LEFT JOIN BIDoc.BasicGraphLinks l2 ON l2.NodeToId = n.BasicGraphNodeId
WHERE l.BasicGraphLinkId IS NULL AND l2.BasicGraphLinkId IS NULL AND n.ProjectConfigId = @projectconfigid AND n.GraphKind = @grphHigh

--SELECT src.Description, tgt.Description, l.ExtendedProperties FROM BIDoc.BasicGraphNodes src
--INNER JOIN BIDoc.BasicGraphLinks l ON l.NodeFromId = src.BasicGraphNodeId
--INNER JOIN BIDoc.BasicGraphNodes tgt ON l.NodeToId = tgt.BasicGraphNodeId
--WHERE src.GraphKind = N'DFHigh'
