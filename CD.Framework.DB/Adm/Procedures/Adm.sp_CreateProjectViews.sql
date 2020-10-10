CREATE PROCEDURE [Adm].[sp_CreateProjectViews]
AS

DECLARE @CreateSQL NVARCHAR(MAX)
DECLARE @CreateSQL1 NVARCHAR(MAX)

IF NOT EXISTS (SELECT TOP 1 1 FROM sys.views vw INNER JOIN sys.schemas s ON vw.schema_id = s.schema_id WHERE s.name = N'BIDoc' AND vw.name = N'DataFlowSequences')
	BEGIN
		SET @CreateSQL = 'CREATE VIEW [BIDoc].[DataFlowSequences] 
						  AS '
	END
ELSE
	BEGIN
		SET @CreateSQL = 'ALTER VIEW [BIDoc].[DataFlowSequences] 
						  AS '
	END

IF NOT EXISTS (SELECT TOP 1 1 FROM sys.views vw INNER JOIN sys.schemas s ON vw.schema_id = s.schema_id WHERE s.name = N'BIDoc' AND vw.name = N'DataFlowSequenceSteps')
	BEGIN
		SET @CreateSQL1 = 'CREATE VIEW [BIDoc].[DataFlowSequenceSteps] 
						  AS '
	END
ELSE
	BEGIN
		SET @CreateSQL1 = 'ALTER VIEW [BIDoc].[DataFlowSequenceSteps] 
						  AS '
	END

DECLARE @Enumerator CURSOR

SET @Enumerator = CURSOR LOCAL FAST_FORWARD FOR
SELECT REPLACE(CAST(ProjectConfigId AS NVARCHAR(100)),'-','')
FROM Adm.ProjectConfigs

OPEN @Enumerator

DECLARE @id NVARCHAR(100)

WHILE (1=1)
BEGIN
  FETCH NEXT FROM @Enumerator INTO @id
  IF (@@FETCH_STATUS <> 0) BREAK

  SET @CreateSQL = @CreateSQL + 'SELECT [SequenceId], [SourceNode], [TargetNode], [DetailLevel], [ProjectConfigid] FROM ['+ @id +'].[DataFlowSequences]
					  UNION ALL '

  SET @CreateSQL1 = @CreateSQL1 + 'SELECT [Id], [SourceNodeId], [TargetNodeId], [SequenceId], [StepNumber] FROM ['+ @id +'].[DataFlowSequenceSteps]
					  UNION ALL '
END

SET @CreateSQL = LEFT( @CreateSQL,LEN( @CreateSQL)-10)
SET @CreateSQL1 = LEFT( @CreateSQL1,LEN( @CreateSQL1)-10)


EXEC sp_executesql @CreateSQL
EXEC sp_executesql @CreateSQL1
