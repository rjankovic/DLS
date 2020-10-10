CREATE PROCEDURE [BIDoc].[sp_GetLineageGridFavorities]
	@userId INT,
	@projectConfigId UNIQUEIDENTIFIER
AS
SELECT TOP 500
h.SourceElementType, h.TargetElementType,
h.SourceRootElementId, h.TargetRootElementId,

MAX(sdp.DescriptivePath) SourceRootDescriptivePath, MAX(tdp.DescriptivePath) TargetRootDescriptivePath,
MAX(st.TypeDescription) SourceTypeDescription, MAX(tt.TypeDescription) TargetTypeDescription,
h.SourceRootElementPath, h.TargetRootElementPath

FROM BIDoc.LineageGridHistory h
INNER JOIN BIDoc.ModelElementDescriptivePaths sdp ON sdp.ModelElementId = h.SourceRootElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths tdp ON tdp.ModelElementId = h.TargetRootElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions st ON st.ElementType = h.SourceElementType
INNER JOIN BIDoc.ModelElementTypeDescriptions tt ON tt.ElementType = h.TargetElementType
WHERE h.UserId = @userId AND h.ProjectConfigId = @projectConfigId
GROUP BY h.SourceRootElementId, h.TargetRootElementId, 
h.SourceElementType, h.TargetElementType,
h.SourceRootElementPath, h.TargetRootElementPath

ORDER BY COUNT(*) * 10 * MAX(DATEDIFF(DAY, h.CreatedDateTime, GETDATE()) + 1) DESC