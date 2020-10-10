CREATE FUNCTION [Stg].[f_GetExtractItem]
(
	@ExtractItemId	INT
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
WHERE ExtractItemId = @ExtractItemId
)
