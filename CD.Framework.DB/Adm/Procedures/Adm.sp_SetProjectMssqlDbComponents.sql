CREATE PROCEDURE [Adm].[sp_SetProjectMssqlDbComponents]
	@projectconfigid UNIQUEIDENTIFIER,
	@components Adm.UDTT_MssqlDbProjectComponents READONLY
AS
DELETE FROM Adm.MssqlDbProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
INSERT INTO Adm.MssqlDbProjectComponents(
			[ServerName]
           ,[DbName]
		   ,[ProjectConfig_ProjectConfigId]
		   )
		   SELECT
		   ServerName
		   ,DbName
		   ,@projectconfigid
		   FROM @components

