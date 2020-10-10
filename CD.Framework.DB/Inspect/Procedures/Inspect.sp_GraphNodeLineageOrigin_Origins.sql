CREATE PROCEDURE [Inspect].[sp_GraphNodeLineageOrigin_Origins]
  @nodeId INT,
  @maxLevels INT = 5
AS

  IF OBJECT_ID('tempdb.dbo.#resList', 'U') IS NOT NULL
  DROP TABLE #resList; 

  DECLARE @currentLevel INT = 1
  CREATE TABLE #resList 
  (
  Id INT,
  Level INT,
  Name NVARCHAR(MAX),
  RefPath NVARCHAR(MAX),
  DisplayInResult BIT DEFAULT 0,
  Description NVARCHAR(MAX)
  )

  -- the selected node and all descendants
  ;WITH descendants AS
  (
	SELECT e.BasicGraphInfoNodeId, 0 [Level], e.Name, e.RefPath, 1 DisplayInResult, e.Description
	FROM BasicGraphInfoNodes e WHERE e.BasicGraphInfoNodeId = @nodeId
	
	UNION ALL

	SELECT e.BasicGraphInfoNodeId, 0 [Level], e.Name, e.RefPath, 0 DisplayInResult, e.Description
	FROM descendants d
	INNER JOIN BasicGraphInfoLinks l ON d.BasicGraphInfoNodeId = l.NodeTo_BasicGraphInfoNodeId
	INNER JOIN BasicGraphInfoNodes e ON e.BasicGraphInfoNodeId = l.NodeFrom_BasicGraphInfoNodeId
	WHERE l.LinkType = 1
  )
  INSERT INTO #resList (Id, Level, Name, RefPath, DisplayInResult, Description)
  SELECT e.BasicGraphInfoNodeId, 0 [Level], CONVERT(NVARCHAR(1000), e.Name), e.RefPath, e.DisplayInResult, e.Description
  FROM descendants e

  WHILE @currentLevel <= @maxLevels
  BEGIN
	
	;WITH descendants AS
	(
		SELECT e.BasicGraphInfoNodeId, l.Level + 1 Level, e.Name, e.RefPath, 1 DisplayInResult, e.Description
		FROM BasicGraphInfoLinks lnk
		INNER JOIN #resList l ON l.Id = lnk.NodeTo_BasicGraphInfoNodeId
		INNER JOIN BasicGraphInfoNodes e ON e.BasicGraphInfoNodeId = lnk.NodeFrom_BasicGraphInfoNodeId
		LEFT JOIN #resList cycle ON cycle.Id = e.BasicGraphInfoNodeId
		WHERE cycle.Id IS NULL AND lnk.LinkType IN (5)

		UNION ALL

		SELECT e.BasicGraphInfoNodeId, d.Level [Level], e.Name, e.RefPath, 0 DisplayInResult, e.Description
		FROM descendants d
		INNER JOIN BasicGraphInfoLinks l ON d.BasicGraphInfoNodeId = l.NodeFrom_BasicGraphInfoNodeId
		INNER JOIN BasicGraphInfoNodes e ON e.BasicGraphInfoNodeId = l.NodeTo_BasicGraphInfoNodeId
		--LEFT JOIN #resList cycle ON cycle.Id = e.BasicGraphInfoNodeId
		WHERE /*cycle.Id IS NULL AND*/ l.LinkType = 1
	)
	-- lineage links from outside to the resultset
	INSERT INTO #resList (Id, Level, Name, RefPath, DisplayInResult, Description)
	SELECT DISTINCT d.BasicGraphInfoNodeId, d.[Level], d.Name, d.RefPath, DisplayInResult, d.Description FROM descendants d
	
	SET @currentLevel = @currentLevel + 1
  END

  SELECT Id, Level, Name, RefPath, Description FROM #resList WHERE DisplayInResult = 1 ORDER BY Level
GO
