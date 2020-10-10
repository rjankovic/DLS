CREATE PROCEDURE [Stg].[sp_SaveNewExtract]
	@extractId UNIQUEIDENTIFIER,
	@projectConfigId UNIQUEIDENTIFIER,
	@extractedBy NVARCHAR(MAX),
	@extractStartTime DATETIME

AS
	INSERT INTO stg.Extracts
	(
	ExtractId,
	ProjectConfigId,
	ExtractedBy,
	ExtractStartTime
	)
	VALUES
	(
	@extractId,
	@projectConfigId,
	@extractedBy,
	@extractStartTime
	)

GO