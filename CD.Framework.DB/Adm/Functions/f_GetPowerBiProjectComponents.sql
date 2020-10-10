CREATE FUNCTION [Adm].[f_GetPowerBiProjectComponents]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE
AS RETURN
(
	SELECT
	[PowerBiProjectComponentId]
    ,[RedirectUri]
	,[ApplicationID]
	,[WorkspaceID]
	,[ReportServerURL]
	,[ReportServerFolder]
	,[ProjectConfig_ProjectConfigId] 
	FROM adm.PowerBiProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
)