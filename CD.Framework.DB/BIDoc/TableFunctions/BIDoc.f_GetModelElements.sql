CREATE FUNCTION [BIDoc].[f_GetModelElements]
(
	@projectconfigid UNIQUEIDENTIFIER
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
  FROM [BIDoc].[ModelElements] WHERE ProjectConfigId = @projectconfigid
)
