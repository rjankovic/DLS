CREATE PROCEDURE [BIDoc].[sp_AddElementsToModel]
	@projectconfigid UNIQUEIDENTIFIER,
	@elements [BIDoc].[UDTT_ModelElements] READONLY
AS

INSERT INTO [BIDoc].[ModelElements]
(
	  --[ModelElementId]
      --,
	  [ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
      ,[ProjectConfigId]
	  ,[RefPathPrefix]
	  ,[RefPathSuffix]
)
SELECT
	--[ModelElementId]
      --,
	  [ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
	  ,@projectconfigid
	  ,LEFT([RefPath], 300)
	  ,LEFT(RefPathSuffix, 300)
FROM @elements

SELECT oe.ModelElementId SequentialId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN @elements oe ON oe.RefPath = e.RefPath COLLATE SQL_Latin1_General_CP1_CS_AS
WHERE e.ProjectConfigId = @projectconfigid
