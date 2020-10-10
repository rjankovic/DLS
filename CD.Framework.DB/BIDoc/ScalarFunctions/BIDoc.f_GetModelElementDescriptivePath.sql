CREATE FUNCTION [BIDoc].[f_GetModelElementDescriptivePath]
(
	@modelElementId int
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
	RETURN (SELECT DescriptivePath FROM BIDoc.ModelElementDescriptivePaths WHERE ModelElementId = @modelElementId)
END
