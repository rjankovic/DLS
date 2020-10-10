CREATE FUNCTION [BIDoc].[f_GetMessages]
(
	@projectconfigid UNIQUEIDENTIFIER
)
RETURNS TABLE AS RETURN
(
SELECT 
se.[Caption] AS 'SourceName'
,sed.[DescriptiveRootPath] AS 'SourcePath'
,te.[Caption]	AS'TargetName'
,ted.[DescriptiveRootPath] AS 'TargetPath'
,dmt.[DataMessageType]
,dm.[Message]
FROM BIDoc.DataMessages dm
INNER JOIN BIDoc.ModelElements se ON se.ModelElementId = dm.SourceElementId
INNER JOIN BIDoc.ModelElements te ON te.ModelElementId = dm.TargetElementId
INNER JOIN BIDoc.DataMessagesType dmt ON dmt.DataMessagesTypeId = dm.DataMessagesTypeId
INNER JOIN BIDoc.ModelElementDescriptivePaths sed ON sed.ModelElementId = dm.SourceElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths ted ON Ted.ModelElementId = dm.TargetElementId
WHERE se.ProjectConfigId = @projectconfigid
)
