CREATE FUNCTION [Adm].[f_GetMssqlAgentProjectComponents]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS RETURN
(
	SELECT
	[MssqlAgentProjectComponentId]
    ,[ServerName]
	,[JobName]
	,[ProjectConfig_ProjectConfigId] 
	FROM adm.MssqlAgentProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
)