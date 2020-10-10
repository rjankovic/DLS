CREATE PROCEDURE [BIDoc].[sp_AddLinksToModel]
	@links [BIDoc].[UDTT_ModelLinks] READONLY
AS	 
	INSERT INTO [BIDoc].[ModelLinks]
	 (
	 		[ElementFromId]
	       ,[ElementToId]
	       ,[Type]
	       ,[ExtendedProperties]
	 )
	 SELECT
	 		l.[ElementFromId]
	       ,l.[ElementToId]
	       ,l.[Type]
	       ,l.[ExtendedProperties]
	 FROM @links l
	 LEFT JOIN BIDoc.ModelLinks el ON el.ElementFromId = l.ElementFromId AND el.ElementToId = l.ElementToId AND el.Type = l.Type
	 WHERE el.ModelLinkId IS NULL