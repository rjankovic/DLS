CREATE PROCEDURE [Inspect].[sp_ElementUsage]
  @elementId INT,
  @maxLevels INT = 5
AS
  IF OBJECT_ID('tempdb.dbo.#resList', 'U') IS NOT NULL
  DROP TABLE #resList; 

  DECLARE @currentLevel INT = 1
  CREATE TABLE #resList 
  (
  Id INT,
  Level INT,
  Name NVARCHAR(1000),
  PrevLevelName NVARCHAR(1000)
  )

  INSERT INTO #resList (Id, Level, Name, PrevLevelName)
  SELECT e.Id, 0 [Level], CONVERT(NVARCHAR(1000), e.Caption + ' (' +  e.RefPath + ')') Name, '' PrevLevelName 
  FROM BIDocModelElements e WHERE e.Id = @elementId

  WHILE @currentLevel <= @maxLevels
  BEGIN
	INSERT INTO #resList (Id, Level, Name, PrevLevelName)
	SELECT e.Id, l.Level + 1 Level, lnk.Type + ' -> ' + e.Caption + ' (' + e.RefPath + ')' Name, l.Name
	FROM BIDocModelLinks lnk
	INNER JOIN #resList l ON l.Id = lnk.ElementFromId
	INNER JOIN BIDocModelElements e ON e.Id = lnk.ElementToId
	LEFT JOIN #resList cycle ON cycle.Id = e.Id
	WHERE cycle.Id IS NULL AND lnk.Type != 'parent'
	
	INSERT INTO #resList (Id, Level, Name, PrevLevelName)
	SELECT e.Id, l.Level + 1 Level, lnk.Type + ' <- ' + e.Caption + ' (' + e.RefPath + ')' Name, l.Name
	FROM BIDocModelLinks lnk
	INNER JOIN #resList l ON l.Id = lnk.ElementToId
	INNER JOIN BIDocModelElements e ON e.Id = lnk.ElementFromId
	LEFT JOIN #resList cycle ON cycle.Id = e.Id
	WHERE cycle.Id IS NULL
	
	SET @currentLevel = @currentLevel + 1
  END

  SELECT Id, Level, PrevLevelName, Name FROM #resList

GO
