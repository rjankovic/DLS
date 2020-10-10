CREATE FUNCTION [BIDoc].[f_GetModelElementType]
(	
	@modelElementId INT 
)
RETURNS TABLE AS RETURN
(
	SELECT Type FROM BIDoc.ModelElements WHERE ModelElementId = @modelElementId
)