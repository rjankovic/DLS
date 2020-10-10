CREATE PROCEDURE [Annotate].[sp_DeleteLink]
	@linkId INT
AS

UPDATE  Annotate.ElementLinks
SET UpdatedVersion = 1 WHERE ElementLinkId = @linkId