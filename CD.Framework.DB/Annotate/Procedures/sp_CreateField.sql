CREATE PROCEDURE [Annotate].[sp_CreateField]
	@fieldName NVARCHAR(MAX),
	@projectConfigId UNIQUEIDENTIFIER
AS

IF EXISTS(SELECT TOP 1 1 FROM Annotate.Fields WHERE FieldName = @fieldName AND ProjectConfigId = @projectConfigId)
BEGIN
	UPDATE Annotate.Fields SET Deleted = 0 WHERE FieldName = @fieldName AND ProjectConfigId = @projectConfigId
END
ELSE
BEGIN
	INSERT INTO Annotate.Fields(FieldName, ProjectConfigId, Deleted) VALUES(@fieldName, @projectConfigId, 0)
END