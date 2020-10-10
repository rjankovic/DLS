CREATE PROCEDURE [Inspect].[sp_GetDataFlowLinksAmongNodes]
@nodeIds [BIDoc].[UDTT_IdList] READONLY
AS
SELECT DISTINCT
	nl.BasicGraphLinkId, 
	nl.NodeFromId, 
	nl.NodeToId 
FROM 
BIDoc.BasicGraphLinks nl
INNER JOIN @nodeIds nf ON nf.Id = nl.NodeFromId
INNER JOIN @nodeIds nt ON nt.Id = nl.NodeToId
WHERE nl.LinkType = N'Dataflow'
