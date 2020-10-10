CREATE PROCEDURE [Annotate].[sp_DeleteField]
	@fieldId INT
AS

UPDATE  Annotate.Fields
SET Deleted = 1 WHERE FieldId = @fieldId