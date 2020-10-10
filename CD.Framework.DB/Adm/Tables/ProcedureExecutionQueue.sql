CREATE TABLE [Adm].[ProcedureExecutionQueue]
(
	[ProcedureExecutionQueueId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ProcedureName] NVARCHAR(MAX),
	[ProjectConfigId] UNIQUEIDENTIFIER,
	[RequestId] UNIQUEIDENTIFIER
)
