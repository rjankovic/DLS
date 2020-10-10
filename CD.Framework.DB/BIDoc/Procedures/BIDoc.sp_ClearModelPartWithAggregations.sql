CREATE PROCEDURE [BIDoc].[sp_ClearModelPartWithAggregations]
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX)
AS


--EXEC [BIDOc].[sp_SetRefPathIntervals] @projectconfigid
	
--DECLARE @elementId INT = (SELECT [ModelElementId] FROM [BIDoc].[f_GetModelElementIdByRefPath] (@projectConfigId, @path))
--DECLARE 
--	@intervalFrom INT,
--	@intervalTo INT

--SELECT @intervalFrom = e.RefPathIntervalStart, @intervalTo = e.RefPathIntervalEnd FROM BIDoc.ModelElements e WHERE e.ModelElementId = @elementId


--EXEC [Adm].[sp_WriteLogInfo] N'Clearing machine learning'

--DELETE p FROM Learning.OlapRulePremises p
--INNER JOIN Learning.OlapRules r ON p.OlapRuleId = r.OlapRuleId
--WHERE r.ProjectConfigId = @projectconfigid

--DELETE c FROM Learning.OlapRuleConclusions c
--INNER JOIN Learning.OlapRules r ON c.OlapRuleId = r.OlapRuleId
--WHERE r.ProjectConfigId = @projectconfigid

--DELETE FROM Learning.OlapRules WHERE ProjectConfigId = @projectconfigid

--DELETE FROM Learning.OlapQueryFields WHERE ProjectConfigId = @projectconfigid

--DELETE FROM Learning.OlapFields WHERE ProjectConfigId = @projectconfigid

--DELETE FROM Learning.OlapFieldReferences WHERE ProjectConfigId = @projectconfigid

---------------

EXEC [Adm].[sp_WriteLogInfo] N'Clearing high level ancestors'

DECLARE @rc INT = 1

WHILE @rc > 0
BEGIN
DELETE TOP (10000) ea FROM BIDoc.HigherLevelElementAncestors ea
INNER JOIN BIDoc.ModelElements e ON ea.SouceElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectConfigId 
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing documents'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) d FROM [BIDoc].GraphDocuments d
INNER JOIN [BIDoc].[BasicGraphNodes] n ON d.GraphNode_Id = n.BasicGraphNodeId
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
WHERE n.ProjectConfigId = @projectconfigid 
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND n.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END


EXEC [Adm].[sp_WriteLogInfo] N'Clearing graph links'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) l FROM [BIDoc].[BasicGraphLinks] l
INNER JOIN [BIDoc].[BasicGraphNodes] n ON l.NodeFromId = n.BasicGraphNodeId
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
WHERE n.ProjectConfigId = @projectconfigid 
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND n.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END


EXEC [Adm].[sp_WriteLogInfo] N'Clearing graph nodes'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) n FROM [BIDoc].[BasicGraphNodes] n
INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
WHERE n.ProjectConfigId = @projectconfigid 
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END


EXEC [Adm].[sp_WriteLogInfo] N'Clearing data messages'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) dm FROM [BIDoc].[DataMessages] dm
INNER JOIN [BIDoc].[ModelElements] e ON dm.SourceElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectconfigid
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing fulltext'

DELETE fts FROM Search.FullTextSearch fts
INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = fts.ModelElementId
WHERE e.ProjectConfigId = @projectConfigId 
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo

UPDATE ae  SET ModelElementId = NULL 
FROM Annotate.AnnotationElements ae
INNER JOIN BIDoc.ModelElements e ON ae.ModelElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectconfigid 
AND LEFT(e.RefPath, LEN(@path)) = @path
--AND e.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo

EXEC [Adm].[sp_WriteLogInfo] N'Clearing model links'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) l FROM [BIDoc].[ModelLinks] l
INNER JOIN [BIDoc].[ModelElements] ef ON l.ElementFromId = ef.ModelElementId
INNER JOIN [BIDoc].[ModelElements] et ON l.ElementToId = et.ModelElementId
WHERE ef.ProjectConfigId = @projectconfigid 
AND LEFT(ef.RefPath, LEN(@path)) = @path
--AND ef.RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing model elements'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) FROM [BIDoc].[ModelElements] 
WHERE ProjectConfigId = @projectconfigid
AND LEFT(RefPath, LEN(@path)) = @path
--AND RefPathIntervalStart BETWEEN @intervalFrom AND @intervalTo
SELECT @rc = @@ROWCOUNT
END




