CREATE PROCEDURE [Adm].[sp_AddAppliedDbVersion]
	@appliedVersion INT
AS
	INSERT INTO adm.DatabaseVersions(VersionNumber)
	VALUES(@appliedVersion)