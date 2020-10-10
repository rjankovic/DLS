CREATE FUNCTION [Stg].[f_GetExtractItems]
(
	@ExtractId	UNIQUEIDENTIFIER,
	@ComponentId INT,
	@ObjectType NVARCHAR(200)
)
RETURNS TABLE AS RETURN
(
SELECT
	[ExtractItemId],	
	[ComponentId],
	[ObjectType],
	[ObjectName],
	[Content]
FROM Stg.ExtractItems
WHERE ExtractId = @ExtractId AND ComponentId = @ComponentId AND ObjectType = @ObjectType
)
