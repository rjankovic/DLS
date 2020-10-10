CREATE FUNCTION [Adm].[f_GetMssqlDbProjectComponents]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS RETURN
(
SELECT [MssqlDbProjectComponentId]
      ,[ServerName]
      ,[DbName]
      ,[ProjectConfig_ProjectConfigId]
	  FROM adm.MssqlDbProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
)