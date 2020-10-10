CREATE FUNCTION [Adm].[f_GetSsisProjectComponents]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS RETURN
(
SELECT [SsisProjectComponentId]
      ,[ServerName]
      ,[FolderName]
      ,[ProjectName]
      ,[ProjectConfig_ProjectConfigId]
FROM adm.SsisProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
)