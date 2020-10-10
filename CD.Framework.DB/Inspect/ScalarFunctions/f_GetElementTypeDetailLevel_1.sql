CREATE FUNCTION [Inspect].[f_GetElementTypeDetailLevel]
(
	@elementType NVARCHAR(255)
)
RETURNS INT
AS
BEGIN
	
	RETURN (SELECT DetailLevel FROM BIDoc.ModelElementTypeDetailLevels WHERE ElementType = @elementType)

END
GO

