CREATE FUNCTION [Adm].[f_GetSsrsProjectComponents]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS RETURN
(
SELECT [SsrsProjectComponentId]
		,[SsrsMode]
      ,[ServerName]
      ,[SsrsServiceUrl]
      ,[SsrsExecutionServiceUrl]
      ,[FolderPath]
	  ,[SharepointBaseUrl]
	  ,[SharepointFolder]
      ,[ProjectConfig_ProjectConfigId]
FROM adm.SsrsProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
)