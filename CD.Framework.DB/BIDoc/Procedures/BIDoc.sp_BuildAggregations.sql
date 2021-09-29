CREATE PROCEDURE [BIDoc].[sp_BuildAggregations]
	@projectconfigid UNIQUEIDENTIFIER --= N'e99a3b4e-7f04-4b98-9780-10e71e6258cf'
	,@requestId UNIQUEIDENTIFIER
AS
	PRINT N'Clearing aggregations'
	EXEC [Adm].[sp_WriteLogInfo] N'Clearing aggregations'
	EXEC [BIDoc].[sp_ClearAggregations] @projectConfigId

	/*
	PRINT N'Rebuilding indexes'
	EXEC [Adm].[sp_WriteLogInfo] N'Rebuilding indexes'
	ALTER INDEX ALL ON BIDoc.ModelElements  
	REBUILD ;
	ALTER INDEX ALL ON BIDoc.ModelLinks
	REBUILD ;   
	*/

	PRINT N'Building dependency graph'
	EXEC [Adm].[sp_WriteLogInfo] N'Building dependency graph'

	EXEC BIDoc.sp_CreateDataFlowGraph @projectconfigid

	PRINT N'Propagating dataflow vertically'
	EXEC [Adm].[sp_WriteLogInfo] N'Propagating dataflow vertically'
	EXEC BIDoc.sp_PropagateDataFlowVertically @projectconfigid
	/*
	PRINT N'Building transitive dependency graph'
	EXEC [Adm].[sp_WriteLogInfo] N'Building transitive dependency graph'
	EXEC BIDoc.sp_BuildTransitiveGraph @projectconfigid, N'DataFlow', N'DataFlowTransitive', 'DataFlow'
	*/
	PRINT N'Building dataflow sequences'
	EXEC [Adm].[sp_WriteLogInfo] N'Building dataflow sequences'
	EXEC BIDoc.sp_BuildDataFlowSequences @projectconfigid

	PRINT N'Propagating nodes to higher level nodes'
	EXEC [Adm].[sp_WriteLogInfo] N'Propagating nodes to higher level nodes'
	EXEC BIDoc.sp_FillHigherLevelElementAncestors @projectconfigid

	PRINT N'Clensing dataflow sequences'
	EXEC [Adm].[sp_WriteLogInfo] N'Clensing dataflow sequences'
	EXEC BIDoc.sp_ClenseDataFlowSequences @projectconfigid

	PRINT N'Building high level dataflow graph'
	EXEC [Adm].[sp_WriteLogInfo] N'Building high level dataflow graph'
	EXEC BIDoc.sp_BuildHighLevelGraph @projectconfigid

	PRINT N'Setting descriptive element paths'
	EXEC [Adm].[sp_WriteLogInfo] N'Setting descriptive element paths'
	EXEC BIDoc.sp_SetModelElementDescriptivePaths @projectconfigid

	PRINT N'Propagating dataflow sequences to higher level nodes'
	EXEC [Adm].[sp_WriteLogInfo] N'Propagating dataflow sequences to higher level nodes'
	EXEC BIDoc.sp_BuildHigherDataFlowSequences @projectconfigid

	PRINT N'Creating medium level dataflow graph'
	EXEC [Adm].[sp_WriteLogInfo] N'Creating medium level dataflow graph'
	EXEC [BIDoc].[sp_CreateDataFlowMediumDetailGraph] @projectconfigid

	PRINT N'Creating low level dataflow graph'
	EXEC [Adm].[sp_WriteLogInfo] N'Creating low level dataflow graph'
	EXEC [BIDoc].[sp_CreateDataFlowLowDetailGraph] @projectconfigid

	PRINT N'Creating links betweeen Elements and Annotations'
	EXEC [Adm].[sp_WriteLogInfo] N'Creating links betweeen Elements and Annotations'
	EXEC Annotate.sp_UpdateModelElememntReferences @projectconfigid
	
	PRINT N'Building fulltext indexes'
	EXEC [Adm].[sp_WriteLogInfo] N'Building fulltext indexes'
	EXEC Search.sp_FindRootElements @projectconfigid
	EXEC Search.sp_IndexFulltext @projectconfigid

	/**/
	PRINT N'Finding and saving errors in dataflow'
	EXEC [Adm].[sp_WriteLogInfo] N'Finding and saving errors in dataflow'
	EXEC BIDoc.sp_FillDataMessages @projectconfigid
	/**/
	
	--UPDATE bm SET bm.Active = 0 
	--FROM adm.BroadcastMessages bm WHERE bm.Active = 1 AND bm.ProjectConfigId = @projectconfigid AND bm.BroadcastMessageType = N'ProjectUpdateStarted'

	PRINT N'Setting RefPath intervals'
	EXEC [Adm].[sp_WriteLogInfo] N'Setting RefPath intervals'
	
	EXEC [BIDOc].[sp_SetRefPathIntervals] @projectConfigId

	UPDATE n
	SET RefPathIntervalStart = e.RefPathIntervalStart, RefPathIntervalEnd = e.RefPathIntervalEnd
	FROM BIDoc.BasicGraphNodes n
	INNER JOIN BIDoc.ModelElements e ON n.SourceElementId = e.ModelElementId
	WHERE e.ProjectConfigId = @projectconfigid

	PRINT N'Building high level solution tree'
	EXEC [Adm].[sp_WriteLogInfo] N'Building high level solution tree'
	
	EXEC [Inspect].[sp_FillHighLevelSolutionTree] @projectconfigid

	PRINT N'Building high level solution tree'
	EXEC [Adm].[sp_WriteLogInfo] N'Building high level solution tree'
	
	/*
	PRINT N'Setting subtree contents for comparison'
	EXEC [Adm].[sp_WriteLogInfo] N'Setting subtree contents for comparison'
	
	EXEC [BIDoc].[sp_SetElementSubtreeContents] @projectconfigid
	*/

	PRINT N'Relinking lineage grid history'
	EXEC [Adm].[sp_WriteLogInfo] N'Relinking lineage grid history'
	
	UPDATE h SET SourceRootElementId = e.ModelElementId
	FROM BIDoc.LineageGridHistory h
	INNER JOIN BIDoc.ModelElements e ON h.SourceRootElementPath = e.RefPath
	WHERE h.ProjectConfigId = @projectconfigid AND e.ProjectConfigId = @projectconfigid

	UPDATE h SET TargetRootElementId = e.ModelElementId
	FROM BIDoc.LineageGridHistory h
	INNER JOIN BIDoc.ModelElements e ON h.TargetRootElementPath = e.RefPath
	WHERE h.ProjectConfigId  = @projectconfigid AND e.ProjectConfigId = @projectconfigid

	DELETE FROM BIDoc.LineageGridHistory
	WHERE ProjectConfigId = @projectconfigid AND (SourceRootElementId IS NULL OR TargetRootElementId IS NULL)
	
	/*
	PRINT N'Rebuilding indexes'
	EXEC [Adm].[sp_WriteLogInfo] N'Rebuilding indexes'
	ALTER INDEX ALL ON BIDoc.BasicGraphNodes  
	REBUILD ;
	ALTER INDEX ALL ON BIDoc.BasicGraphLinks
	REBUILD ;
	*/
	
	EXEC [Adm].[sp_SaveDbOperationFinishedMessage] @requestId

GO