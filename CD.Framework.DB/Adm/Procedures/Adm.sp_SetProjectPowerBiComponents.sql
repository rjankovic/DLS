CREATE PROCEDURE [Adm].[sp_SetProjectPowerBiComponents]
	@projectconfigid UNIQUEIDENTIFIER,
	@components Adm.UDTT_PowerBiProjectComponents READONLY
AS
DELETE FROM Adm.PowerBiProjectComponents WHERE ProjectConfig_ProjectConfigId = @projectconfigid
INSERT INTO Adm.PowerBiProjectComponents(
			[RedirectUri]
			,[ApplicationID]
			,[WorkspaceID]
			,[ReportServerURL]
			,[ReportServerFolder]
			,[ProjectConfig_ProjectConfigId]
			,DiskFolder
			,ConfigType
		   )
		   SELECT
		   RedirectUri
		   ,ApplicationID
		   ,WorkspaceID
		   ,ReportServerURL
		   ,ReportServerFolder
		   ,@projectconfigid
		   ,DiskFolder
		   ,ConfigType
		   FROM @components