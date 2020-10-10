CREATE PROCEDURE [BIDoc].[sp_AddOrUpdateElements]
	@projectconfigid UNIQUEIDENTIFIER,
	@elements [BIDoc].[UDTT_ModelElements] READONLY
AS


UPDATE e 
SET
	e.ExtendedProperties = ne.ExtendedProperties,
	e.Definition = ne.Definition,
	e.Caption = ne.Caption
FROM [BIDoc].[ModelElements] e
INNER JOIN @elements ne ON e.RefPath = ne.RefPath AND e.ProjectConfigId = @projectconfigid

DECLARE @newElements [BIDoc].[UDTT_ModelElements]
INSERT INTO @newElements
(
	[ModelElementId]
	  ,[ExtendedProperties]
      ,[RefPath]
      ,[Definition]
      ,[Caption]
      ,[Type]
	  ,[RefPathSuffix]
)
SELECT
	e.ModelElementId
	  ,e.[ExtendedProperties]
      ,e.[RefPath]
      ,e.[Definition]
      ,e.[Caption]
      ,e.[Type]
	  ,LEFT(e.RefPathSuffix, 300)
FROM @elements e
LEFT JOIN BIDoc.ModelElements me ON me.ProjectConfigId = @projectconfigid AND me.RefPath = e.RefPath
WHERE me.ModelElementId IS NULL


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
	  ,RefPathSuffix
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
FROM @newElements
--LEFT JOIN BIDoc.ModelElements me ON me.RefPath = @elements.RefPath AND me.ProjectConfigId = @projectconfigid
--WHERE me.ModelElementId IS NULL

SELECT oe.ModelElementId SequentialId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN @elements oe ON oe.RefPath = e.RefPath COLLATE SQL_Latin1_General_CP1_CS_AS
WHERE e.ProjectConfigId = @projectconfigid
