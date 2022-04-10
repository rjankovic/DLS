CREATE PROCEDURE [Adm].[sp_CheckAppliedDbVersion]
	@appliedVersion INT
AS
	IF @appliedVersion = 0
	BEGIN
		IF OBJECT_ID('Adm.DatabaseVersions') IS NOT NULL
		BEGIN
			RAISERROR (N'The DLS database has aready been initialized', 16, 127);	
		END
	END ELSE
	BEGIN
		IF NOT EXISTS(SELECT TOP 1 1 FROM adm.DatabaseVersions WHERE VersionNumber = @appliedVersion)
		BEGIN
			DECLARE @err NVARCHAR(MAX) = N'The DB version ' + CONVERT(NVARCHAR(10), @appliedVersion) + ' has not yet been applied'
			RAISERROR (@err, 16, 127);	
		END
	END
