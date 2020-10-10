CREATE FUNCTION Annotate.f_ListLinkTypes
(@projectConfigId UNIQUEIDENTIFIER)
RETURNS TABLE
AS RETURN
SELECT l.Deleted, l.LinkTypeId, l.LinkTypeName, IIF((EXISTS (SELECT TOP 1 1 FROM Annotate.ElementLinks el WHERE el.LinkTypeId = l.LinkTypeId)), 1, 0) UsedInLinks
FROM Annotate.LinkTypes l
WHERE l.ProjectConfigId = @projectConfigId
AND DELETED = 0