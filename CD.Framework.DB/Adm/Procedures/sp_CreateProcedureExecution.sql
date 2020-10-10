CREATE PROCEDURE [Adm].[sp_CreateProcedureExecution]
	@procedureName NVARCHAR(MAX),
	@projectConfigId UNIQUEIDENTIFIER,
	@requestId UNIQUEIDENTIFIER
AS
	INSERT INTO adm.ProcedureExecutionQueue(ProcedureName, ProjectConfigId, RequestId) VALUES(@procedureName, @projectConfigId, @requestId)

GO
