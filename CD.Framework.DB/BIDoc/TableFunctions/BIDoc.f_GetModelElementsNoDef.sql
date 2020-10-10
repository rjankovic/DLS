CREATE FUNCTION [BIDoc].[f_GetModelElementsNoDef]
(
	@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(
SELECT [ModelElementId]
      ,[ExtendedProperties]
      ,[RefPath]
      ,NULL [Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
  FROM [BIDoc].[ModelElements] WHERE ProjectConfigId = @projectconfigid
)
