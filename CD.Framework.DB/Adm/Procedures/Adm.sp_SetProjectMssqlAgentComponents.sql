CREATE PROCEDURE [Adm].[sp_SetProjectMssqlAgentComponents]
	@projectconfigid UNIQUEIDENTIFIER,
	@components Adm.UDTT_MssqlAgentProjectComponents READONLY
AS
DELETE FROM Adm.MssqlAgentProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
INSERT INTO Adm.MssqlAgentProjectComponents(
			[ServerName]
           ,[JobName]
		   ,[ProjectConfig_ProjectConfigId]
		   )
		   SELECT
		   ServerName
		   ,JobName
		   ,@projectconfigid
		   FROM @components

