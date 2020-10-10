CREATE PROCEDURE [BIDoc].[sp_GetModelLinksUnderPathToChildrenOfType]
--(
	@projectconfigid UNIQUEIDENTIFIER,
	@path NVARCHAR(MAX),
	@type NVARCHAR(200)
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
EXEC BIDoc.sp_GetModelElementsUnderPathToChildrenOfTypeNoDef @projectConfigId, @path, @type

SELECT DISTINCT 
	   l.[ModelLinkId]
      ,l.[ElementFromId]
      ,l.[ElementToId]
      ,l.[Type]
      ,l.[ExtendedProperties]
  FROM [BIDoc].[ModelLinks] l
  INNER JOIN @elements ef ON ef.ModelElementId = l.ElementFromId
  INNER JOIN @elements et ON et.ModelElementId = l.ElementToId
--)
