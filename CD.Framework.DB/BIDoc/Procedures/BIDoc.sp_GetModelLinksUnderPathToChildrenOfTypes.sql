CREATE PROCEDURE [BIDoc].[sp_GetModelLinksUnderPathToChildrenOfTypes]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX),
	@types BIDoc.UDTT_StringList READONLY
AS
/*
)
RETURNS TABLE AS RETURN
(
*/

DECLARE @elements TABLE
(
[ModelElementId] INT,
[ExtendedProperties] NVARCHAR(MAX),
[RefPath] NVARCHAR(MAX),
[Definition] NVARCHAR(MAX),
[Caption] NVARCHAR(MAX),
[Type] NVARCHAR(MAX),
[ProjectConfigId] UNIQUEIDENTIFIER
)

INSERT INTO @elements
EXEC BIDoc.sp_GetModelElementsUnderPathToChildrenOfTypesNoDef @projectConfigId, @path, @types

SELECT DISTINCT 
	   l.[ModelLinkId]
      ,l.[ElementFromId]
      ,l.[ElementToId]
      ,l.[Type]
      ,l.[ExtendedProperties]
  FROM [BIDoc].[ModelLinks] l
  INNER JOIN /*BIDoc.f_GetModelElementsUnderPathToChildrenOfTypesNoDef(@projectConfigId, @path, @types)*/ @elements ef ON ef.ModelElementId = l.ElementFromId
  INNER JOIN /*BIDoc.f_GetModelElementsUnderPathToChildrenOfTypesNoDef(@projectConfigId, @path, @types)*/ @elements et ON et.ModelElementId = l.ElementToId
--)
