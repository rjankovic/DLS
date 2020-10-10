CREATE FUNCTION [Adm].[f_GetProjectConfigName]
(
@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(
SELECT [Name] FROM Adm.ProjectConfigs WHERE ProjectConfigId = @projectconfigid
)
