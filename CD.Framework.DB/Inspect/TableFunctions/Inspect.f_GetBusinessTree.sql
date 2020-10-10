CREATE FUNCTION Inspect.f_GetBusinessTree
(
@projectConfigId UNIQUEIDENTIFIER--,
--@userIdentity NVARCHAR(255)
)
RETURNS TABLE
AS RETURN

WITH elemTree AS(
	SELECT e.ModelElementId, e.Caption, e.Type, e.RefPath, 0 ParentLevel
	FROM BIDoc.ModelElements e 
	WHERE e.ProjectConfigId = @projectConfigId AND e.Type IN (
	N'CD.DLS.Model.Business.Organization.BusinessFolderElement',
	N'CD.DLS.Model.Business.Excel.PivotTableTemplateElement'
	)
	--AND (e.RefPath LIKE N'Business/SharedFolder%' OR e.RefPath LIKE N'%Name=''' + @userIdentity + N'''%')

	UNION ALL

	SELECT e.ModelElementId, e.Caption, e.Type, e.RefPath, ParentLevel + 1
	FROM elemTree t
	INNER JOIN BIDoc.ModelLinks l ON l.ElementFromId = t.ModelElementId AND l.Type = N'parent'
	INNER JOIN BIDoc.ModelElements e ON e.ModelElementId = l.ElementToId
	WHERE e.Type = N'CD.DLS.Model.Business.Organization.BusinessFolderElement'
)
,x AS(

SELECT ModelElementId, Caption, elemTree.[Type], td.TypeDescription, MAX(ParentLevel) MaxParentLevel, RefPath
FROM elemTree
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON [Type] = td.ElementType
GROUP BY ModelElementId, Caption, elemTree.[Type], td.TypeDescription, RefPath
)
SELECT ModelElementId, Caption, x.[Type], TypeDescription, MaxParentLevel, l.ElementToId ParentElementId, RefPath
FROM x
LEFT JOIN BIDoc.ModelLinks l ON l.ElementFromId = ModelElementId AND l.Type = N'parent'
