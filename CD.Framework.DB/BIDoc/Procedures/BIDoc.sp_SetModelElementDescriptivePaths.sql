CREATE PROCEDURE [BIDoc].[sp_SetModelElementDescriptivePaths]
	@projectconfigid UNIQUEIDENTIFIER
AS

DELETE dp FROM BIDoc.ModelElementDescriptivePaths dp
INNER JOIN BIDoc.ModelElements e ON dp.ModelElementId = e.ModelElementId
WHERE e.ProjectConfigId = @projectconfigid

INSERT INTO BIDoc.ModelElementDescriptivePaths(ModelElementId, DescriptivePath, DescriptiveRootPath)
SELECT 
e.ModelElementId,
IIF(e.[Type] IN (N'CD.DLS.Model.Mssql.Db.SqlScriptElement', N'CD.DLS.Model.Mssql.Ssas.MdxFragmentElement'),

ped.TypeDescription + N' [' + pe.Caption + N']'
	+ IIF(ISNULL(ppe.RefPath, N'') = N'', 
	N'', N' in ' + pped.TypeDescription + N' [' + ppe.Caption + N']')

,

--ed.TypeDescription + IIF(pe.[Type] <> N'CD.DLS.Model.Mssql.Db.SchemaElement', N' ', N' [' + ppe.Caption + N']') + N'[' + pe.Caption + N']'

ed.TypeDescription + IIF(pe.[Type] <> N'CD.DLS.Model.Mssql.Db.SchemaElement', N' ', N' [' + ppe.Caption + N'].') + IIF(pe.[Type] = N'CD.DLS.Model.Mssql.Db.SchemaElement', N'[' + pe.Caption + N'].', N'') +  N'[' + e.Caption + N']' 
	+ 
	IIF(pe.[Type] = N'CD.DLS.Model.Mssql.Db.SchemaElement', '',
		--IIF(ISNULL(ppe.RefPath, N'') = N'', N'', N' in ' + pped.TypeDescription + N' [' + ppe.Caption + N']'),
		IIF(ISNULL(pe.RefPath, N'') = N'', N'', N' in ' + ped.TypeDescription + N' [' + pe.Caption + N']')
	)
	--+ IIF(ISNULL(ppe.RefPath, N'Solution') = N'Solution', N'', N' in ' + pped.TypeDescription + N' [' + ppe.Caption + N']')
),

----
NULL

FROM BIDoc.ModelElements e
INNER JOIN BIDoc.ModelElementTypeDescriptions ed ON ed.ElementType = e.Type
LEFT JOIN BIDoc.ModelLinks l ON l.Type = N'parent' AND l.ElementFromId = e.ModelElementId
LEFT JOIN BIDoc.ModelElements pe ON pe.ModelElementId = l.ElementToId
LEFT JOIN BIDoc.ModelElementTypeDescriptions ped ON ped.ElementType = pe.Type
LEFT JOIN BIDoc.ModelLinks pl ON pl.Type = N'parent' AND pl.ElementFromId = pe.ModelElementId
LEFT JOIN BIDoc.ModelElements ppe ON ppe.ModelElementId = pl.ElementToId
LEFT JOIN BIDoc.ModelElementTypeDescriptions pped ON pped.ElementType = ppe.Type
--WHERE pe.[Type] = N'CD.DLS.Model.Mssql.Db.SchemaElement'
WHERE e.ProjectConfigId = @projectconfigid


---- set root paths

UPDATE e SET DescriptiveRootPath = hle1d.TypeDescription + N' [' + hle1.Caption + N']' +
IIF(hle2.ModelElementId IS NULL, N'', N', ' + hle2d.TypeDescription + N' ' + 
	IIF(hle2p.[Type] = N'CD.DLS.Model.Mssql.Db.SchemaElement', N'[' + hle2p.Caption + N'].', N'')
	+ N'[' + hle2.Caption + N']'
)
FROM BIDoc.ModelElementDescriptivePaths e
LEFT JOIN BIDoc.HigherLevelElementAncestors hl1 ON hl1.SouceElementId = e.ModelElementId AND hl1.DetailLevel = 3
LEFT JOIN BIDoc.HigherLevelElementAncestors hl2 ON hl2.SouceElementId = e.ModelElementId AND hl2.DetailLevel = 2
LEFT JOIN BIDoc.ModelLinks hl2pl ON hl2pl.Type = N'parent' AND hl2pl.ElementFromId = hl2.AncestorElementId
LEFT JOIN BIDoc.ModelElements hle1 ON hle1.ModelElementId = hl1.AncestorElementId
LEFT JOIN BIDoc.ModelElements hle2 ON hle2.ModelElementId = hl2.AncestorElementId
LEFT JOIN BIDoc.ModelElements hle2p ON hle2p.ModelElementId = hl2pl.ElementToId
LEFT JOIN BIDoc.ModelElementTypeDescriptions hle1d ON hle1d.ElementType = hle1.Type
LEFT JOIN BIDoc.ModelElementTypeDescriptions hle2d ON hle2d.ElementType = hle2.Type
INNER JOIN BIDoc.ModelElements ee ON e.ModelElementId = ee.ModelElementId
WHERE ee.ProjectConfigId = @projectconfigid


UPDATE dp SET DescriptiveRootPath = e.Caption
FROM BIDoc.ModelElementDescriptivePaths dp
INNER JOIN BIDoc.ModelElements e ON dp.ModelElementId = e.ModelElementId
INNER JOIN Adm.ProjectConfigs p ON p.ProjectConfigId = e.ProjectConfigId
WHERE dp.DescriptiveRootPath IS NULL AND e.ProjectConfigId = @projectconfigid

UPDATE dp SET DescriptivePath = e.Caption
FROM BIDoc.ModelElementDescriptivePaths dp
INNER JOIN BIDoc.ModelElements e ON dp.ModelElementId = e.ModelElementId
INNER JOIN Adm.ProjectConfigs p ON p.ProjectConfigId = e.ProjectConfigId
WHERE dp.DescriptivePath IS NULL AND e.ProjectConfigId = @projectconfigid
