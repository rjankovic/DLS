CREATE PROCEDURE [Annotate].[sp_CreateLinkType]
	@linkTypeName NVARCHAR(MAX),
	@projectConfigId UNIQUEIDENTIFIER
AS

IF EXISTS(SELECT TOP 1 1 FROM Annotate.LinkTypes WHERE LinkTypeName = @linkTypeName AND ProjectConfigId = @projectConfigId)
BEGIN
	UPDATE Annotate.LinkTypes SET Deleted = 0 WHERE LinkTypeName = @linkTypeName AND ProjectConfigId = @projectConfigId
END
ELSE
BEGIN
	INSERT INTO Annotate.LinkTypes(LinkTypeName, ProjectConfigId, Deleted) VALUES(@linkTypeName, @projectConfigId, 0)
END