CREATE PROCEDURE [Adm].[sp_SetProjectSsrsComponents]
	@projectconfigid UNIQUEIDENTIFIER,
	@components Adm.UDTT_SsrsProjectComponents READONLY
AS
DELETE FROM Adm.SsrsProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
INSERT INTO Adm.SsrsProjectComponents(
			[SsrsMode]
			,[ServerName]
			,[FolderPath]
			,[SsrsServiceUrl]
			,[SsrsExecutionServiceUrl]
			,[SharepointBaseUrl]
			,[SharepointFolder]
			,[ProjectConfig_ProjectConfigId]
		   )
		   SELECT
		   SsrsMode
		   ,ServerName
		   ,FolderPath
		   ,SsrsServiceUrl
		   ,SsrsExecutionServiceUrl
		   ,SharepointBaseUrl
		   ,SharepointFolder
		   ,@projectconfigid
		   FROM @components

