CREATE PROCEDURE [Annotate].[sp_DeleteLinkType]
	@linkTypeId INT
AS

UPDATE  Annotate.LinkTypes
SET Deleted = 1 WHERE LinkTypeId = @linkTypeId