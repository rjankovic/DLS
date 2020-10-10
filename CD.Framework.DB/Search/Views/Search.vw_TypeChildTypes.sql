CREATE VIEW [Search].[vw_TypeChildTypes]
	AS 
SELECT tct.ParentType, tct.ChildType, td.TypeDescription ChildTypeDescription FROM Search.TypeChildTypes tct
INNER JOIN BIDoc.ModelElementTypeDescriptions td ON td.ElementType = tct.ChildType
WHERE ParentType IS NOT NULL

