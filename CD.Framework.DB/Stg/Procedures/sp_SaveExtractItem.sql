CREATE PROCEDURE [Stg].[sp_SaveExtractItem]
	@ExtractId	UNIQUEIDENTIFIER,
	@ComponentId INT,
	@ObjectType NVARCHAR(MAX),
	@ObjectName NVARCHAR(MAX),
	@Content NVARCHAR(MAX)
AS
	INSERT INTO Stg.ExtractItems(
	[ExtractId],
	[ComponentId],
	[ObjectType],
	[ObjectName],
	[Content]
	)
	VALUES
	(
	@ExtractId,
	@ComponentId,
	@ObjectType,
	@ObjectName,
	@Content
	)
RETURN 0
