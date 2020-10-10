CREATE PROCEDURE [BIDoc].[sp_ClearModel]
	@projectconfigid UNIQUEIDENTIFIER,
	@requestId UNIQUEIDENTIFIER = NULL
AS

DECLARE @rc INT = 1

EXEC [BIDoc].[sp_ClearAggregations] @projectConfigId

--SET @rc = 1
--WHILE @rc > 0
--BEGIN
--DELETE TOP (10000) FROM [BIDoc].[BasicGraphNodes] WHERE ProjectConfigId = @projectconfigid --AND GraphKind = @graphkind
--SELECT @rc = @@ROWCOUNT
--END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing model links'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) l FROM [BIDoc].[ModelLinks] l
INNER JOIN [BIDoc].[ModelElements] ef ON l.ElementFromId = ef.ModelElementId
INNER JOIN [BIDoc].[ModelElements] et ON l.ElementToId = et.ModelElementId
WHERE ef.ProjectConfigId = @projectconfigid AND (ef.Type LIKE N'CD.DLS.Model.Mssql.%' OR et.Type LIKE N'CD.DLS.Model.Mssql.%')
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Clearing model elements'

SET @rc = 1
WHILE @rc > 0
BEGIN
DELETE TOP (10000) FROM [BIDoc].[ModelElements] 
WHERE ProjectConfigId = @projectconfigid AND [Type] LIKE N'CD.DLS.Model.Mssql.%'
SELECT @rc = @@ROWCOUNT
END

EXEC [Adm].[sp_WriteLogInfo] N'Done clearing model'

IF @requestId IS NOT NULL
BEGIN
-- notify the WF followup
EXEC [Adm].[sp_SaveDbOperationFinishedMessage] @requestId
END