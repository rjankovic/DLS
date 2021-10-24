CREATE PROCEDURE [BIDoc].[sp_CreateDataFlowGraph]
	@projectconfigid UNIQUEIDENTIFIER --= N'e99a3b4e-7f04-4b98-9780-10e71e6258cf'
AS

----------------------
-- clear higher ancestors (reference nodes)

DECLARE @rc INT = 1
WHILE @rc > 0
BEGIN
	DELETE TOP (10000) a FROM [BIDoc].[HigherLevelElementAncestors] a
	INNER JOIN BIDoc.ModelElements e ON a.SouceElementId = e.ModelElementId
	WHERE e.ProjectConfigId = @projectConfigId

	SELECT @rc = @@ROWCOUNT
END

CREATE TABLE #dataflowLinks
(
RuleName NVARCHAR(MAX),
ElementFromId INT,
ElementToId INT,
)


-------------------------

DECLARE @graphKind NVARCHAR(50) = N'DataFlow'

---------------------- clear graph

EXEC BIDoc.sp_ClearGraph @projectconfigid, @graphKind

------------------------ replicate graph

INSERT INTO BIDoc.BasicGraphNodes
(
Name, 
NodeType, 
ParentId, 
GraphKind, 
ProjectConfigId, 
SourceElementId, 
TopologicalOrder
)
SELECT
e.Caption,
REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(e.Type,
N'CD.DLS.Model.Mssql', N''), N'.Db.', N''), N'.Ssis.', N''), N'.Ssas.', N''), N'.Ssrs.', N''), N'.Agent.', N''), N'.Tabular.', N''), N'.Pbi.', N''), N'.PowerQuery.', N'PowerQuery.')  NodeType,
NULL,
@graphKind,
e.ProjectConfigId,
e.ModelElementId,
0 TopologicalOrder
FROM BIDoc.ModelElements e
WHERE e.ProjectConfigId = @projectconfigid

UPDATE en 
SET ParentId = pen.BasicGraphNodeId 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.Type = N'parent'
INNER JOIN BIDoc.ModelElements pe ON pe.ModelElementId = l.ElementToId
INNER JOIN BIDoc.BasicGraphNodes en ON en.SourceElementId = e.ModelElementId
INNER JOIN BIDoc.BasicGraphNodes pen ON pen.SourceElementId = pe.ModelElementId
WHERE e.ProjectConfigId = @projectConfigId
AND pe.ProjectConfigId = @projectConfigId
AND en.GraphKind = @graphKind

INSERT INTO BIDoc.BasicGraphLinks
(LinkType, NodeFromId, NodeToId)
SELECT 
N'Parent', n.BasicGraphNodeId, n.ParentId
FROM BIDoc.BasicGraphNodes n 
WHERE n.ProjectConfigId = @projectConfigId
AND n.ParentId IS NOT NULL

--------------------------- 
-- MssqlReferenceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'MssqlReferenceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Reference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type <> N'CD.DLS.Model.Mssql.Db.SqlDmlTargetReferenceElement'

-----------------
-- MssqlDmlTargetReferenceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'MssqlDmlTargetReferenceDataFlowRule', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Reference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Db.SqlDmlTargetReferenceElement'


-----------------
-- MssqlDmlSourceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'MssqlDmlSourceDataFlowRule', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'TargetReference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Db.SqlDmlSourceElement'


-----------------
-- MssqlNAryOperationOperandRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'MssqlNAryOperationOperandRule', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'OperationOutputColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Db.SqlNAryOperationOperandColumnElement'


-----------------
-- SsisDfExternalColumnsDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfExternalColumnsDataFlowRule_ExternalDestinationColumn', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'ExternalDestinationColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfColumnElement'

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfExternalColumnsDataFlowRule_ExternalSourceColumn', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'ExternalSourceColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfColumnElement'

-----------------
-- SsisDfTransformationDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfTransformationDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'SourceDfColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type IN(N'CD.DLS.Model.Mssql.Ssis.DfColumnElement', N'CD.DLS.Model.Mssql.Ssis.DfUnpivotSourceReferenceElement', N'CD.DLS.Model.Mssql.Ssis.DfLookupColumnElement')


-----------------
-- SsisDfSourceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfSourceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'SourceConnection'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfSourceElement'


-----------------
-- SsisDfUnpivotSourceDataFlowRuleTPK

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfUnpivotSourceDataFlowRuleTPK', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'TargetPivotKeyColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfUnpivotSourceReferenceElement'


-----------------
-- SsisDfUnpivotSourceDataFlowRuleTV

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfUnpivotSourceDataFlowRuleTV', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'TargetValueColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfUnpivotSourceReferenceElement'


-----------------
-- SsisDfAggregationDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfAggregationDataFlowRule', ls.ElementToId, lt.ElementToId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks ls ON e.ModelElementId = ls.ElementFromId AND ls.[Type] = N'SourceDfColumn'
INNER JOIN BIDoc.ModelLinks lt ON e.ModelElementId = lt.ElementFromId AND lt.[Type] = N'TargetDfColumn'
--INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfColumnAggregationLinkElement'

-----------------
-- SsisDfLookupOutputJoinReferenceRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsisDfLookupOutputJoinReferenceRule', ls.ElementToId, lt.ElementToId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks ls ON e.ModelElementId = ls.ElementFromId AND ls.[Type] = N'InputJoinColumn'
INNER JOIN BIDoc.ModelLinks lt ON e.ModelElementId = lt.ElementFromId AND lt.[Type] = N'OutputColumn'
--INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssis.DfLookupOutputJoinReferenceElement'


-----------------
-- SsasDsvSourceColumnDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasDsvSourceColumnDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Source'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.DatasourceViewColumnElement'

-----------------
-- SsasDimensionKeyColumnDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasDimensionKeyColumnDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'DsvColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.KeyColumnElement'


-----------------
-- SsasDimensionNameColumnDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasDimensionNameColumnDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'DsvColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.NameColumnElement'


-----------------
-- SsasHierarchyLevelDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasHierarchyLevelDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Attribute'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.HierarchyLevelElement'


-----------------
-- SsasPhysicalMeasureDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasPhysicalMeasureDataFlowRule', pse.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks pl ON e.ModelElementId = pl.ElementToId AND pl.[Type] = N'parent'
INNER JOIN BIDoc.ModelElements pse ON pse.ModelElementId = pl.ElementFromId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement'
AND pse.Type = N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasurePartitionSourceElement'


-----------------
-- SsasPhysicalMeasurePartitionSourceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasPhysicalMeasurePartitionSourceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Source'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasurePartitionSourceElement'

-----------------
-- SsasPartitionColumnDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasPartitionColumnDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Source'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.PartitionColumnElement'


-----------------
-- SsasCubeDimensionDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsasCubeDimensionDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'DatabaseDimension'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement'


-------------------
---- CubeDimensionAttributeDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'CubeDimensionAttributeDataFlowRule', re.ModelElementId, e.ModelElementId 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'DatabaseDimensionAttribute'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement'


-------------------
---- CubeDimensionHierarchyDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'CubeDimensionHierarchyDataFlowRule', re.ModelElementId, e.ModelElementId 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'DatabaseDimensionHierarchy'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.CubeDimensionHierarchyElement'


-------------------
---- CubeDimensionHierarchyLevelDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'CubeDimensionHierarchyLevelDataFlowRule', re.ModelElementId, e.ModelElementId 
FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Attribute'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.CubeDimensionHierarchyLevelElement'


-----------------
-- SsrsDataSetFieldDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsrsDataSetFieldDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Source'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssrs.DataSetFieldElement'


-----------------
-- SsrsReportParameterValidValuesDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsrsReportParameterValidValuesDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'ValueField'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssrs.ReportParameterValidValuesDataSetElement'


-----------------
-- SsrsReportParameterDefaultValuesDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'SsrsReportParameterDefaultValuesDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'ValueField'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssrs.ReportParameterDefaultValuesDataSetElement'

-------------------------
-- TabularPartitionColumnSourceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'TabularPartitionColumnSourceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'SourceElement'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Tabular.SsasTabularPartitionColumnElement'

-------------------------
-- TabularPartitionColumnTargetTableColumnDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'TabularPartitionColumnTargetTableColumnDataFlowRule', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'TargetTableColumn'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Tabular.SsasTabularPartitionColumnElement'

---------------------------------------------------
-- 

-- TabularMeasureReferenceRule
INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'PowerQueryReferenceRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Reference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Tabular.SsasTabularMeasureElement'


-------------------------
-- DaxTableReferenceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'DaxTableReferenceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Reference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.DaxTableReferenceElement'

-------------------------
-- DaxColumnReferenceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'DaxColumnReferenceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Reference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.DaxColumnReferenceElement'


-------------------------
-- DaxLinkDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'DaxLinkDataFlowRule', sre.ModelElementId, tre.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks sl ON e.ModelElementId = sl.ElementFromId AND sl.[Type] = N'Source'
INNER JOIN BIDoc.ModelElements sre ON sre.ModelElementId = sl.ElementToId
INNER JOIN BIDoc.ModelLinks tl ON e.ModelElementId = tl.ElementFromId AND tl.[Type] = N'Target'
INNER JOIN BIDoc.ModelElements tre ON tre.ModelElementId = tl.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.Ssas.DaxDataFlowLinkElement'

-------------------------
-- DaxLinkTargetDataFlowRule

--INSERT INTO #dataflowLinks
--(RuleName, ElementFromId, ElementToId)

--SELECT DISTINCT N'DaxLinkTargetDataFlowRule', e.ModelElementId, re.ModelElementId FROM BIDoc.ModelElements e
--INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Target'
--INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
--AND e.ProjectConfigId = @projectConfigId
--AND e.Type = N'CD.DLS.Model.Mssql.Ssas.DaxDataFlowLinkElement'

-------------------------
--PivotTableFieldSourceDataFlowRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'PivotTableFieldSourceDataFlowRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'SourceField'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Business.Excel.PivotTableFieldElement'

-------------------------
--PivotTableFieldVaulesFilterSourceMeasureRule

INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'PivotTableFieldVaulesFilterSourceMeasureRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'SourceMeasure'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Business.Excel.PivotTableValuesFilterElement'


--------------------------
-- Power Query Rules

-- PowerQueryDFLinkDataFlowRule
INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'PowerQueryDFLinkDataFlowRule', sre.ModelElementId, tre.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks sl ON e.ModelElementId = sl.ElementFromId AND sl.[Type] = N'Source'
INNER JOIN BIDoc.ModelElements sre ON sre.ModelElementId = sl.ElementToId
INNER JOIN BIDoc.ModelLinks tl ON e.ModelElementId = tl.ElementFromId AND tl.[Type] = N'Target'
INNER JOIN BIDoc.ModelElements tre ON tre.ModelElementId = tl.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type = N'CD.DLS.Model.Mssql.PowerQuery.DataFlowLinkElement'


-- PowerQueryReferenceRule
INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT N'PowerQueryReferenceRule', re.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks l ON e.ModelElementId = l.ElementFromId AND l.[Type] = N'Reference'
INNER JOIN BIDoc.ModelElements re ON re.ModelElementId = l.ElementToId
AND e.ProjectConfigId = @projectConfigId
AND e.Type LIKE N'CD.DLS.Model.Mssql.PowerQuery.%'



--
/*
INSERT INTO #dataflowLinks
(RuleName, ElementFromId, ElementToId)

SELECT DISTINCT  N'PowerBIDataFlowRule',pse.ModelElementId, e.ModelElementId FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelLinks pl ON e.ModelElementId = pl.ElementToId AND pl.[Type] = N'parent'
INNER JOIN BIDoc.ModelElements pse ON pse.ModelElementId = pl.ElementFromId
AND 
(
e.Type = N'CD.DLS.Model.Mssql.Pbi.ReportSectionElement'
OR e.Type = N'CD.DLS.Model.Mssql.Pbi.ReportElement'
OR e.Type = N'CD.DLS.Model.Mssql.Pbi.VisualElement'
OR e.Type = N'CD.DLS.Model.Mssql.Pbi.ConnectionElement'
OR e.Type = N'CD.DLS.Model.Mssql.Pbi.PbiTableElement'
)
*/

----


--- add DF links to the graph

;WITH dataflowLinks AS (
SELECT DISTINCT ElementFromId, ElementToId FROM #dataflowLinks
)
INSERT INTO BIDoc.BasicGraphLinks
(LinkType, NodeFromId, NodeToId)
SELECT
N'DataFlow',
nf.BasicGraphNodeId,
nt.BasicGraphNodeId
FROM dataflowLinks l
INNER JOIN BIDoc.BasicGraphNodes nf ON nf.SourceElementId = l.ElementFromId
INNER JOIN BIDoc.BasicGraphNodes nt ON nt.SourceElementId = l.ElementToId
WHERE nf.GraphKind = @graphKind AND nt.GraphKind = @graphKind

DROP TABLE #dataflowLinks


