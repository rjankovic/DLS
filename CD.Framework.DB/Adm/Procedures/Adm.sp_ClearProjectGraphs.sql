CREATE PROCEDURE [Adm].[sp_ClearProjectGraphs]
  @projectConfigId UNIQUEIDENTIFIER
  AS
  BEGIN

  --DELETE dfn FROM DataFlowGraphInfoNodes dfn
  --INNER JOIN BasicGraphInfoNodes n ON n.BasicGraphInfoNodeId = dfn.BasicGraphInfoNodeId
  --WHERE n.ProjectConfigId = @projectConfigId

  DELETE FROM BasicGraphInfoNodes WHERE ProjectConfigId = @projectConfigId

  DELETE FROM BasicGraphInfoLinks WHERE ProjectConfigId = @projectConfigId

  END
