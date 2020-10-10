CREATE PROCEDURE [Adm].[sp_SetProjectSsasDbComponents]
	@projectconfigid UNIQUEIDENTIFIER,
	@components Adm.UDTT_SsasDbProjectComponents READONLY
AS
DELETE FROM Adm.SsasDbProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
INSERT INTO Adm.SsasDbProjectComponents(
			[ServerName]
           ,[DbName]
		   ,[SSASType]
		   ,[ProjectConfig_ProjectConfigId]
		   )
		   SELECT
		   ServerName
		   ,DbName
		   ,SSASType
		   ,@projectconfigid
		   FROM @components

