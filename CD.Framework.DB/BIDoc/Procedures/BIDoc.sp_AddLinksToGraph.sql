CREATE PROCEDURE [BIDoc].[sp_AddLinksToGraph]
	@links  [BIDoc].[UDTT_BasicGraphLinks] READONLY
AS
INSERT INTO [BIDoc].[BasicGraphLinks]
(
	--[BasicGraphLinkId]
      --,
	  [LinkType]
      ,[NodeFromId]
      ,[NodeToId]
)
SELECT
--[BasicGraphLinkId]
      --,
	  [LinkType]
      ,[NodeFromId]
      ,[NodeToId]
	  FROM 
@links
