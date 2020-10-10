CREATE PROCEDURE [BIDoc].[sp_BuildDataFlowSequences]
	@projectconfigid UNIQUEIDENTIFIER
AS

--DECLARE @id NVARCHAR(100)
--	SET @id = REPLACE(CAST(@projectconfigid AS NVARCHAR(100)),'-','')
	

--	DECLARE @IdentityInsert1 NVARCHAR(MAX)
--	SET @IdentityInsert1 = '
--		SET IDENTITY_INSERT  [' + @id + '].DataFlowSequences ON'
--	EXEC sp_executesql @IdentityInsert1	

--GO

------------------------------------------------------------------------

	--DECLARE @projectconfigid UNIQUEIDENTIFIER = N'8BEDB75E-5D79-449F-9480-F26FEC6A1BE1'

	DECLARE @id NVARCHAR(100)
	SET @id = REPLACE(CAST(@projectconfigid AS NVARCHAR(100)),'-','')
	
	DECLARE @message NVARCHAR(MAX)

	/**/
	-- clear sequences
	DECLARE @SQLTask2 NVARCHAR(MAX)
	SET @SQLTask2 = 'TRUNCATE TABLE [' + @id + '].[DataFlowSequenceSteps]
					 TRUNCATE TABLE [' + @id + '].[DataFlowSequences]
					 TRUNCATE TABLE [' + @id + '].[DataFlowSequenceSteps_Heap]
					 TRUNCATE TABLE [' + @id + '].[DataFlowSequences_Heap]'
	EXEC sp_executesql  @SQLTask2
	
	--ALTER INDEX [IX_BIDoc_DataFlowSequenceSteps_SequenceId] ON [BIDoc].[DataFlowSequenceSteps]
	--DISABLE

	-- init structures
	DECLARE @graphKind NVARCHAR(50) = N'DataFlow'
	DECLARE @linkType NVARCHAR(50) = N'DataFlow'

	CREATE TABLE #usedLinks
	(
		SourceNodeId INT NOT NULL,
		LinkId INT NOT NULL 
		--CONSTRAINT PK_UsedLinks PRIMARY KEY(SourceNodeId, LinkId)
	)


	CREATE TABLE #waitingLinks
	(
		SourceNodeId INT NOT NULL,
		LinkId INT NOT NULL
		--CONSTRAINT PK_WaitingLinks PRIMARY KEY(SourceNodeId, LinkId)
	)

	CREATE TABLE #sequenceCopyTargets
	(
		SourceSequenceId INT NOT NULL,
		TargetSequenceId INT NOT NULL,
		NextSequenceStepNo INT NOT NULL,
		FinalLinkId INT NOT NULL
	)

	---- find initial front nodes
	
	SELECT DISTINCT n.BasicGraphNodeId SourceNodeId, n.BasicGraphNodeId NodeId INTO #frontNodes
	FROM BIDoc.BasicGraphNodes n 
	INNER JOIN BIDoc.BasicGraphLinks lf ON lf.NodeFromId = n.BasicGraphNodeId AND lf.LinkType = @linkType
	--LEFT JOIN BIDoc.BasicGraphLinks lt ON lt.NodeToId = n.BasicGraphNodeId AND lt.LinkType = @linkType
	WHERE n.ProjectConfigId = @projectconfigid AND n.GraphKind = @graphKind --AND lt.BasicGraphLinkId IS NULL

	SET @message = CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' initial front nodes'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message

	-- find initial links

	SELECT l.NodeFromId SourceNodeId, l.BasicGraphLinkId LinkId INTO #newReachableLinks
	FROM BIDoc.BasicGraphLinks l
	INNER JOIN #frontNodes n ON n.NodeId = l.NodeFromId
	WHERE l.LinkType = @linkType

	SET @message = CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' initial front links'
	PRINT @message
	EXEC [Adm].[sp_WriteLogInfo] @message

	-- create new one-step sequences from the new links
		DECLARE @InsertSQL NVARCHAR(MAX)
		SET @InsertSQL = 'INSERT INTO [' + @id + '].[DataFlowSequences_Heap] WITH(TABLOCK)
		(
		SourceNode,
		TargetNode,
		DetailLevel,
		ProjectConfigid--,
		--LastLinkId
		)
		SELECT DISTINCT
		l.NodeFromId,
		l.NodeToId,
		1,
		@projectconfigid--,
		--l.BasicGraphLinkId
		FROM #newReachableLinks ll
		INNER JOIN BIDoc.BasicGraphLinks l ON ll.LinkId = l.BasicGraphLinkId
		--INNER JOIN BIDoc.BasicGraphNodes sn ON sn.BasicGraphNodeId = s.SourceNode
INNER JOIN BIDoc.BasicGraphNodes sn ON sn.BasicGraphNodeId = l.NodeFromId
INNER JOIN BIDoc.ModelElements se ON se.ModelElementId = sn.SourceElementId
INNER JOIN [BIDoc].[SequenceEndpointTypes] stp ON stp.TypeName = se.Type
		LEFT JOIN [' + @id + '].DataFlowSequences_Heap s ON s.SourceNode = l.NodeFromId AND s.TargetNode = l.NodeToId
		WHERE s.SequenceId IS NULL'
		EXEC sp_executesql  @InsertSQL, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid

		-- insert one-step sequence steps using the newly discovered links
		
		DECLARE @InsertSQL2 NVARCHAR(MAX)
		SET @InsertSQL2 = 'INSERT INTO ['  + @id + '].[DataFlowSequenceSteps_Heap] WITH(TABLOCK)
		(
		SourceNodeId,
		TargetNodeId,
		StepNumber,
		SequenceId
		)
		SELECT DISTINCT
		l.NodeFromId
		,l.NodeToId
		,0
		,s.SequenceId
		FROM #newReachableLinks ll
		INNER JOIN BIDoc.BasicGraphLinks l ON ll.LinkId = l.BasicGraphLinkId
		INNER JOIN [' + @id + '].DataFlowSequences_Heap s ON s.SourceNode = l.NodeFromId AND s.TargetNode = l.NodeToId
		-- only if the one-step sequence isnt filled already
		LEFT JOIN [' + @id + '].DataFlowSequenceSteps_Heap st ON st.SequenceId = s.SequenceId
		WHERE st.Id IS NULL'
		EXEC sp_executesql  @InsertSQL2

	DECLARE @iteration INT = 1

	CREATE NONCLUSTERED INDEX [IX_NewReachableLinks]
ON [#newReachableLinks] ([SourceNodeId],[LinkId])

	CREATE NONCLUSTERED INDEX [IX_temp_SequenceCopyTargets]
ON [#sequenceCopyTargets] ([SourceSequenceId]) INCLUDE([TargetSequenceId])

	-- while new unused links are being discovered (such that all other links with the same target node are also available)
	WHILE (EXISTS(SELECT TOP 1 1 FROM #newReachableLinks))
	BEGIN

		
		-- create new sequences from those that end in the front nodes (and such sequence does not exist already)
		DECLARE @InsertSQL3 NVARCHAR(MAX)
		SET @InsertSQL3 = 'INSERT INTO [' + @id + '].[DataFlowSequences_Heap] WITH(TABLOCK)
		(
		SourceNode,
		TargetNode,
		DetailLevel,
		ProjectConfigid--,
		--LastLinkId
		)
		SELECT DISTINCT
		s.SourceNode
		,l.NodeToId
		,1
		,@projectconfigid
		--,l.BasicGraphLinkId
		FROM #newReachableLinks ll
		INNER JOIN BIDoc.BasicGraphLinks l ON l.BasicGraphLinkId = ll.LinkId
		INNER JOIN [' + @id + '].DataFlowSequences_Heap s ON s.TargetNode = l.NodeFromId AND s.SourceNode = ll.SourceNodeId
		LEFT JOIN [' + @id + '].DataFlowSequences_Heap es ON es.SourceNode = s.SourceNode AND es.TargetNode = l.NodeToId
		WHERE es.SequenceId IS NULL'
		EXEC sp_executesql  @InsertSQL3, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid

		SET @message = CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' extended sequences in iteration ' + CONVERT(NVARCHAR(10), @iteration)
		PRINT @message
		EXEC [Adm].[sp_WriteLogInfo] @message

		DECLARE @InsertSQL10 NVARCHAR(MAX)
		
		SET @InsertSQL10 = 
		-- find copied sequences (sequences ending in the source node of the new links are copied to the sequences ending in the links' target)
		N'
		INSERT INTO #sequenceCopyTargets WITH(TABLOCK)
		(
		SourceSequenceId,
		TargetSequenceId,
		NextSequenceStepNo,
		FinalLinkId
		)
		SELECT DISTINCT
		s.SequenceId,
		ts.SequenceId,
		@iteration + 1,
		l.BasicGraphLinkId
		FROM #newReachableLinks ll
		INNER JOIN BIDoc.BasicGraphLinks l ON l.BasicGraphLinkId = ll.LinkId
		INNER JOIN [' + @id + '].DataFlowSequences_Heap s ON s.TargetNode = l.NodeFromId AND s.SourceNode = ll.SourceNodeId
		INNER JOIN [' + @id + '].DataFlowSequences_Heap ts ON ts.SourceNode = s.SourceNode AND ts.TargetNode = l.NodeToId
		--LEFT JOIN [' + @id + '].DataFlowSequenceSteps_Heap stp ON stp.SequenceId = ts.SequenceId
		--WHERE stp.Id IS NULL	-- not ideal...
		'
		EXEC sp_executesql  @InsertSQL10, N'@projectconfigid UNIQUEIDENTIFIER, @iteration INT', @projectconfigid = @projectconfigid, @iteration = @iteration

		SET @message = CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' sequence copy targets'
		PRINT @message
		EXEC [Adm].[sp_WriteLogInfo] @message

					ALTER INDEX [IX_temp_SequenceCopyTargets]
ON [#sequenceCopyTargets] --([SourceNodeId],[LinkId])
REBUILD

		
		
		--SELECT * FROM #sequenceCopyTargets

		-- insert copied sequence steps
		DECLARE @InsertSQL4 NVARCHAR(MAX)
		SET @InsertSQL4 = '
		INSERT INTO [' + @id + '].DataFlowSequenceSteps_Heap WITH(TABLOCK)
		(
		SourceNodeId,
		TargetNodeId,
		StepNumber,
		SequenceId
		)
		SELECT DISTINCT
		st.SourceNodeId
		,st.TargetNodeId
		,st.StepNumber
		,ct.TargetSequenceId
		FROM #sequenceCopyTargets ct
		INNER JOIN [' + @id + '].DataFlowSequenceSteps_Heap st ON st.SequenceId = ct.SourceSequenceId
		LEFT JOIN [' + @id + '].DataFlowSequenceSteps_Heap est ON est.SequenceId = ct.TargetSequenceId AND est.SourceNodeId = st.SourceNodeId AND est.TargetNodeId = st.TargetNodeId
		WHERE est.Id IS NULL'
		EXEC sp_executesql  @InsertSQL4 
		SET @message = CONVERT(NVARCHAR(20), @@ROWCOUNT) + N' copied steps'

		PRINT @message
		EXEC [Adm].[sp_WriteLogInfo] @message

		-- add final links to copied sequences
		DECLARE @InsertSQL5 NVARCHAR(MAX)
		SET @InsertSQL5 = 'INSERT INTO [' + @id + '].DataFlowSequenceSteps_Heap WITH(TABLOCK)
		(
		SourceNodeId,
		TargetNodeId,
		StepNumber,
		SequenceId
		)
		SELECT DISTINCT
		l.NodeFromId
		,l.NodeToId
		,ct.NextSequenceStepNo
		,ct.TargetSequenceId
		FROM #sequenceCopyTargets ct
		--INNER JOIN [' + @id + '].DataFlowSequenceSteps_Heap stp ON stp.SequenceId = ct.SourceSequenceId
		INNER JOIN BIDoc.BasicGraphLinks l ON l.BasicGraphLinkId = ct.FinalLinkId
		LEFT JOIN [' + @id + '].DataFlowSequenceSteps_Heap stp ON l.NodeFromId = stp.SourceNodeId AND l.NodeToId = stp.TargetNodeId AND stp.SequenceId = ct.TargetSequenceId
		WHERE stp.Id IS NULL'
		EXEC sp_executesql  @InsertSQL5

			ALTER INDEX [IX_temp_SequenceCopyTargets]
ON [#sequenceCopyTargets] --([SourceNodeId],[LinkId])
DISABLE


		-- prepare for the next iteration
		TRUNCATE TABLE #sequenceCopyTargets

		INSERT INTO #usedLinks WITH(TABLOCK)
		(
			SourceNodeId,
			LinkId
		)
		SELECT SourceNodeId, LinkId FROM #newReachableLinks

		TRUNCATE TABLE #frontNodes

		-- new front nodes - step towards new discovered links
		INSERT INTO #frontNodes WITH(TABLOCK)
		(
			SourceNodeId,
			NodeId
		)
		SELECT DISTINCT ll.SourceNodeId, l.NodeToId 
		FROM #newReachableLinks ll
		INNER JOIN BIDoc.BasicGraphLinks l ON l.BasicGraphLinkId = ll.LinkId

			ALTER INDEX [IX_NewReachableLinks]
ON [#newReachableLinks] --([SourceNodeId],[LinkId])
DISABLE

		TRUNCATE TABLE #newReachableLinks

		-- candidate new found links - not used links from the source nodes
		INSERT INTO #newReachableLinks WITH(TABLOCK)
		(
			SourceNodeId,
			LinkId
		)
		SELECT DISTINCT fn.SourceNodeId, l.BasicGraphLinkId 
		FROM #frontNodes fn
		INNER JOIN BIDoc.BasicGraphLinks l ON fn.NodeId = l.NodeFromId
		LEFT JOIN #usedLinks ul ON ul.LinkId = l.BasicGraphLinkId AND ul.SourceNodeId = fn.SourceNodeId
		WHERE l.LinkType = @linkType AND ul.LinkId IS NULL

			ALTER INDEX [IX_NewReachableLinks]
ON [#newReachableLinks] --([SourceNodeId],[LinkId])
REBUILD

		-- this is slow and only influences the topological order assignment
		
		SET @iteration = @iteration + 1

	END

	DROP TABLE #waitingLinks
	DROP TABLE #sequenceCopyTargets
	DROP TABLE #frontNodes
	DROP TABLE #newReachableLinks
	DROP TABLE #usedLinks

	--insert filtered sequences to std. tables

	DECLARE @InsertSQL11 NVARCHAR(MAX)
		SET @InsertSQL11 = N'INSERT INTO [' + @id + '].[DataFlowSequences] WITH(TABLOCK)
		(
		[SequenceId],
		SourceNode,
		TargetNode,
		DetailLevel,
		ProjectConfigid--,
		--LastLinkId
		)
		SELECT
		s.[SequenceId],
		s.SourceNode,
		s.TargetNode,
		s.DetailLevel,
		s.ProjectConfigid

		FROM [' + @id + '].[DataFlowSequences_Heap] s
INNER JOIN BIDoc.BasicGraphNodes sn ON sn.BasicGraphNodeId = s.SourceNode
INNER JOIN BIDoc.BasicGraphNodes tn ON tn.BasicGraphNodeId = s.TargetNode
INNER JOIN BIDoc.ModelElements se ON se.ModelElementId = sn.SourceElementId
INNER JOIN BIDoc.ModelElements te ON te.ModelElementId = tn.SourceElementId
INNER JOIN [BIDoc].[SequenceEndpointTypes] stp ON stp.TypeName = se.Type
INNER JOIN [BIDoc].[SequenceEndpointTypes] ttp ON ttp.TypeName = te.Type
WHERE s.DetailLevel = 1
'
		EXEC sp_executesql  @InsertSQL11, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid




	
	DECLARE @InsertSQL12 NVARCHAR(MAX)
		SET @InsertSQL12 = N'INSERT INTO [' + @id + '].DataFlowSequenceSteps WITH(TABLOCK)
		(
		SourceNodeId,
		TargetNodeId,
		StepNumber,
		SequenceId
		)
		
SELECT 
		st.SourceNodeId,
		st.TargetNodeId,
		st.StepNumber,
		st.SequenceId

FROM [' + @id + '].[DataFlowSequences_Heap] s
INNER JOIN [' + @id + '].DataFlowSequenceSteps_Heap st ON st.SequenceId = s.SequenceId
INNER JOIN BIDoc.BasicGraphNodes sn ON sn.BasicGraphNodeId = s.SourceNode
INNER JOIN BIDoc.BasicGraphNodes tn ON tn.BasicGraphNodeId = s.TargetNode
INNER JOIN BIDoc.ModelElements se ON se.ModelElementId = sn.SourceElementId
INNER JOIN BIDoc.ModelElements te ON te.ModelElementId = tn.SourceElementId
INNER JOIN [BIDoc].[SequenceEndpointTypes] stp ON stp.TypeName = se.Type
INNER JOIN [BIDoc].[SequenceEndpointTypes] ttp ON ttp.TypeName = te.Type
WHERE s.DetailLevel = 1
'

EXEC sp_executesql  @InsertSQL12, N'@projectconfigid UNIQUEIDENTIFIER', @projectconfigid = @projectconfigid


	

	PRINT N'Rebuilding indexes...'
	EXEC [Adm].[sp_WriteLogInfo] N'Rebuilding indexes...'

	DECLARE @SQLTask1 NVARCHAR(MAX)
	SET @SQLTask1 = 'ALTER INDEX [IX_' + @id + '_DataFlowSequenceSteps_SequenceId] ON [' + @id + '].[DataFlowSequenceSteps]
	REBUILD
	ALTER INDEX [IX_' + @id + '_DataFlowSequenceSteps_SequenceId_SourceNodeId] ON [' + @id + '].[DataFlowSequenceSteps]
	REBUILD
	ALTER INDEX [IX_' + @id + '_DataFlowSequenceSteps_SequenceId_TargetNodeId] ON [' + @id + '].[DataFlowSequenceSteps]
	REBUILD'
	EXEC sp_executesql  @SQLTask1
	

GO