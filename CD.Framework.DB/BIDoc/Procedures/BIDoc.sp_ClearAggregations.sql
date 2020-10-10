CREATE PROCEDURE [BIDoc].[sp_ClearAggregations]
	@projectconfigid UNIQUEIDENTIFIER
AS

EXEC [Adm].[sp_WriteLogInfo] N'Clearing machine learning'

DELETE p FROM Learning.OlapRulePremises p
INNER JOIN Learning.OlapRules r ON p.OlapRuleId = r.OlapRuleId
WHERE r.ProjectConfigId = @projectconfigid

DELETE c FROM Learning.OlapRuleConclusions c
INNER JOIN Learning.OlapRules r ON c.OlapRuleId = r.OlapRuleId
WHERE r.ProjectConfigId = @projectconfigid

DELETE FROM Learning.OlapRules WHERE ProjectConfigId = @projectconfigid

DELETE FROM Learning.OlapQueryFields WHERE ProjectConfigId = @projectconfigid

DELETE FROM Learning.OlapFields WHERE ProjectConfigId = @projectconfigid

DELETE FROM Learning.OlapFieldReferences WHERE ProjectConfigId = @projectconfigid

------

EXEC [Adm].[sp_WriteLogInfo] N'Clearing high level ancestors'

DECLARE @rc INT = 1

WHILE @rc > 0
BEGIN
DELETE TOP (10000) ea FROM BIDoc.HigherLevelElementAncestors ea
INNER JOIN BIDoc.ModelElements e ON ea.SouceElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectConfigId
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing documents'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) d FROM [BIDoc].GraphDocuments d
INNER JOIN [BIDoc].[BasicGraphNodes] n ON d.GraphNode_Id = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid --AND n.GraphKind = @graphkind
SELECT @rc = @@ROWCOUNT
END


EXEC [Adm].[sp_WriteLogInfo] N'Clearing graph links'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) l FROM [BIDoc].[BasicGraphLinks] l
INNER JOIN [BIDoc].[BasicGraphNodes] n ON l.NodeFromId = n.BasicGraphNodeId
WHERE n.ProjectConfigId = @projectconfigid --AND n.GraphKind = @graphkind
SELECT @rc = @@ROWCOUNT
END


EXEC [Adm].[sp_WriteLogInfo] N'Clearing graph nodes'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) FROM [BIDoc].[BasicGraphNodes] WHERE ProjectConfigId = @projectconfigid --AND GraphKind = @graphkind
SELECT @rc = @@ROWCOUNT
END


EXEC [Adm].[sp_WriteLogInfo] N'Clearing data messages'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) dm FROM [BIDoc].[DataMessages] dm
INNER JOIN [BIDoc].[ModelElements] e ON dm.SourceElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectconfigid
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing fulltext'

DELETE fts FROM Search.FullTextSearch fts
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = fts.ModelElementId
WHERE e.ProjectConfigId = @projectConfigId

UPDATE Annotate.AnnotationElements SET ModelElementId = NULL WHERE ProjectConfigId = @projectconfigid


UPDATE BIDoc.LineageGridHistory 
SET SourceRootElementId = NULL, TargetRootElementId = NULL 
WHERE ProjectConfigId = @projectconfigid