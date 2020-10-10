CREATE PROCEDURE [Adm].[sp_SetProjectSsisComponents]
	@projectconfigid UNIQUEIDENTIFIER,
	@components Adm.UDTT_SsisProjectComponents READONLY
AS
DELETE FROM Adm.SsisProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
INSERT INTO Adm.SsisProjectComponents(
			[ServerName]
           ,[FolderName]
		   ,[ProjectName]
		   ,[ProjectConfig_ProjectConfigId]
		   )
		   SELECT
		   ServerName
		   ,FolderName
		   ,ProjectName
		   ,@projectconfigid
		   FROM @components

