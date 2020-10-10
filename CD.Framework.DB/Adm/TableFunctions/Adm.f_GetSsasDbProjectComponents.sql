CREATE FUNCTION [Adm].[f_GetSsasDbProjectComponents]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS RETURN
(
SELECT [SsaslDbProjectComponentId]
      ,[ServerName]
      ,[DbName]
	  ,[SSASType]
      ,[ProjectConfig_ProjectConfigId]
FROM adm.SsasDbProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
)