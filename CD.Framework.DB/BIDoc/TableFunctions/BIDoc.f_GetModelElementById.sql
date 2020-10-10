CREATE FUNCTION [BIDoc].[f_GetModelElementById]
(
	@elementid INT
)
RETURNS TABLE AS RETURN
(
SELECT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] WHERE ModelElementId = @elementid
)
