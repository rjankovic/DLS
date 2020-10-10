CREATE PROCEDURE [Adm].[sp_CreateNewProjectSchemaAndTables]
	@projectconfigid UNIQUEIDENTIFIER
AS
 DECLARE @id NVARCHAR(100)
 DECLARE @CreateSQL NVARCHAR(MAX)
 DECLARE @CreateSQL1 NVARCHAR(MAX)
 DECLARE @CreateSQL2 NVARCHAR(MAX)
 DECLARE @CreateSQL3 NVARCHAR(MAX)
 DECLARE @CreateSQL4 NVARCHAR(MAX)
 DECLARE @CreateSQL5 NVARCHAR(MAX)
 DECLARE @CreateSQL6 NVARCHAR(MAX)
 DECLARE @CreateSQL7 NVARCHAR(MAX)
 DECLARE @CreateSQL8 NVARCHAR(MAX)
 DECLARE @CreateSQL8_1 NVARCHAR(MAX)
 DECLARE @CreateSQL9 NVARCHAR(MAX)
 DECLARE @CreateSQL10 NVARCHAR(MAX)

 SET @id = REPLACE(CAST(@projectconfigid AS NVARCHAR(100)),'-','')

  /*Create Project Schema*/
 SET @CreateSQL = 
   'CREATE SCHEMA [' + @id  +']'

   /* Creata DataFlowSequences*/
 SET @CreateSQL1 = 	  
	'CREATE TABLE [' + @id + '].[DataFlowSequences]
    (
    	[SequenceId] INT NOT NULL PRIMARY KEY, --IDENTITY(1,1),
    	[SourceNode] INT NOT NULL,
    	[TargetNode] INT NOT NULL,
    	[DetailLevel] INT NOT NULL,
    	[ProjectConfigid] UNIQUEIDENTIFIER NOT NULL
    )'

 SET @CreateSQL2 = 
    'CREATE NONCLUSTERED INDEX [IX_'+ @id +'_DataFlowSequences_TargetNode]
    ON ['+ @id +'].[DataFlowSequences] ([TargetNode])
    INCLUDE ([SourceNode])'

 SET @CreateSQL3 = 
    'CREATE NONCLUSTERED INDEX [IX_'+ @id +'_DataFlowSequences_SourceNode_TargetNode]
    ON ['+ @id +'].[DataFlowSequences] ([SourceNode],[TargetNode])'

	/* Creata DataFlowSequenceSteps*/
 SET @CreateSQL4 = 
	'CREATE TABLE ['+ @id +'].[DataFlowSequenceSteps]
	 (
	 	[Id] INT NOT NULL PRIMARY KEY IDENTITY(1,1),
	 	[SourceNodeId] INT NOT NULL,
	 	[TargetNodeId] INT NOT NULL,
	 	[SequenceId] INT NOT NULL,
	 	[StepNumber] INT NOT NULL
	 )'
	
 SET @CreateSQL5 =  
	 'CREATE NONCLUSTERED INDEX [IX_'+ @id +'_DataFlowSequenceSteps_SequenceId]
	 ON ['+ @id +'].[DataFlowSequenceSteps] ([SequenceId]) INCLUDE([Id])'
 
 SET @CreateSQL6 = 	 
	 'CREATE NONCLUSTERED INDEX [IX_'+ @id +'_DataFlowSequenceSteps_SequenceId_SourceNodeId]
	 ON ['+ @id +'].[DataFlowSequenceSteps] ([SequenceId], [SourceNodeId]) INCLUDE([Id], [TargetNodeId])'
 
 SET @CreateSQL7 = 	 
	 'CREATE NONCLUSTERED INDEX [IX_'+ @id +'_DataFlowSequenceSteps_SequenceId_TargetNodeId]
	 ON ['+ @id +'].[DataFlowSequenceSteps] ([SequenceId], [TargetNodeId]) INCLUDE([Id], [SourceNodeId])'


    SET @CreateSQL8 = 	  
 	'CREATE TABLE [' + @id + '].[DataFlowSequences_Heap]
     (
     	[SequenceId] INT NOT NULL IDENTITY(1,1),
     	[SourceNode] INT NOT NULL,
     	[TargetNode] INT NOT NULL,
     	[DetailLevel] INT NOT NULL,
     	[ProjectConfigid] UNIQUEIDENTIFIER NOT NULL
     )'

 SET @CreateSQL8_1 = 
 	'CREATE NONCLUSTERED INDEX [IX_Sequences_Heap_Soource_Target_' + @id + ']
ON [' + @id + '].[DataFlowSequences_Heap] ([SourceNode],[TargetNode])
INCLUDE ([SequenceId])'
 
 
 
 SET @CreateSQL9 = 
 	'CREATE TABLE ['+ @id +'].[DataFlowSequenceSteps_Heap]
 	 (
 	 	[Id] INT NOT NULL IDENTITY(1,1),
 	 	[SourceNodeId] INT NOT NULL,
 	 	[TargetNodeId] INT NOT NULL,
 	 	[SequenceId] INT NOT NULL,
 	 	[StepNumber] INT NOT NULL
 	 )'

 SET @CreateSQL10 = 
 	'CREATE NONCLUSTERED INDEX [IX_SequenceSteps_Heap_SequenceId_' + @id + ']
ON [' + @id + '].[DataFlowSequenceSteps_Heap] ([SequenceId]) INCLUDE([StepNumber], [SourceNodeId], [TargetNodeId], [Id])'
 
 

 EXEC sp_executesql @CreateSQL
 EXEC sp_executesql @CreateSQL1
 EXEC sp_executesql @CreateSQL2
 EXEC sp_executesql @CreateSQL3
 EXEC sp_executesql @CreateSQL4
 EXEC sp_executesql @CreateSQL5
 EXEC sp_executesql @CreateSQL6
 EXEC sp_executesql @CreateSQL7
 EXEC sp_executesql @CreateSQL8
 EXEC sp_executesql @CreateSQL8_1
 EXEC sp_executesql @CreateSQL9
 EXEC sp_executesql @CreateSQL10