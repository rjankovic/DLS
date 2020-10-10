CREATE PROCEDURE [Analyst].[sp_SetModelElementType]

	@ProjectConfigId UNIQUEIDENTIFIER,
	@Name	NVARCHAR(MAX),
	@Attributes [Analyst].[UDTT_ModelElementTypeAttributes] READONLY,
	@AuthorIdentity NVARCHAR(MAX)
AS

DECLARE @typeId INT = (SELECT ModelElementTypeId FROM Analyst.ModelElementTypes t WHERE t.Name = @Name AND t.ProjectConfigId = @ProjectConfigId)
IF @typeId IS NOT NULL
BEGIN
	BEGIN TRAN
		INSERT INTO ModelElementTypes(ProjectConfigId, Name) VALUES (@ProjectConfigId, @Name)
		SELECT @typeId = @@IDENTITY
	COMMIT
END

SELECT ea.ModelElementTypeAttributeId, ea.ModelElementAttributeTypeId, ea.ModelElementTypeId, ea.Name, ea.ExtendedProperties, ModelElementAttributeTypes.ModelElementAttributeTypeId ModelElementAttributeTypeIdNew, a.ExtendedProperties ExtendedPropertiesNew
INTO #modifiedAttrs
FROM Analyst.ModelElementTypeAttributes ea 
INNER JOIN @Attributes a ON a.Name = ea.Name
INNER JOIN Analyst.ModelElementAttributeTypes attrTypes ON attrTypes.Code = a.ModelElementAttributeTypeCode
WHERE ea.ModelElementTypeId = @typeId

SELECT ea.ModelElementTypeAttributeId, ea.ModelElementTypeId, ea.Name, ea.ExtendedProperties
INTO #deletedAttrs
FROM Analyst.ModelElementTypeAttributes ea 
LEFT JOIN @Attributes a ON a.Name = ea.Name
WHERE ea.ModelElementTypeId = @typeId
AND a.Name IS NULL

SELECT a.Name, attrTypes.ModelElementAttributeTypeId, a.ExtendedProperties
INTO #addedAttrs
FROM @Attributes a
LEFT JOIN Analyst.ModelElementTypeAttributes ea ON a.Name = ea.Name AND ea.ModelElementTypeId = @typeId
INNER JOIN Analyst.ModelElementAttributeTypes attrTypes ON attrTypes.Code = a.ModelElementAttributeTypeCode
AND ea.ModelElementTypeAttributeId IS NULL

-- delete attributes that were either deleted or had their types changed
;WITH attrsToDelete AS
(
SELECT a.ModelElementTypeAttributeId FROM #deletedAttrs a
UNION ALL
SELECT m.ModelElementTypeAttributeId FROM #modifiedAttrs m 
WHERE m.ModelElementAttributeTypeId <> m.ModelElementAttributeTypeIdNew 
	OR m.ExtendedProperties <> m.ExtendedPropertiesNew
)
UPDATE o SET o.IsCurrent = 0, o.ValidTo = GETDATE() 
FROM ModelElementAttributes mea
INNER JOIN attrsToDelete atd ON atd.ModelElementTypeAttributeId = mea.ModelElementTypeAttributeId
INNER JOIN Analyst.Objects o ON mea.ObjectId = o.ObjectId

;WITH attrsToDelete AS
(
SELECT a.ModelElementTypeAttributeId FROM #deletedAttrs a
UNION ALL
SELECT m.ModelElementTypeAttributeId FROM #modifiedAttrs m 
WHERE m.ModelElementAttributeTypeId <> m.ModelElementAttributeTypeIdNew 
	OR m.ExtendedProperties <> m.ExtendedPropertiesNew
)

INSERT INTO Objects o SET o.IsCurrent = 0, o.ValidTo = GETDATE() 
FROM ModelElementAttributes mea
INNER JOIN attrsToDelete atd ON atd.ModelElementTypeAttributeId = mea.ModelElementTypeAttributeId
INNER JOIN Analyst.Objects o ON mea.ObjectId = o.ObjectId



;WITH attrsToDelete AS
(
SELECT a.ModelElementTypeAttributeId FROM #deletedAttrs a
UNION ALL
SELECT m.ModelElementTypeAttributeId FROM #modifiedAttrs m 
WHERE m.ModelElementAttributeTypeId <> m.ModelElementAttributeTypeIdNew 
	OR m.ExtendedProperties <> m.ExtendedPropertiesNew
)
DELETE meta FROM ModelElementAttributeTypes meta
INNER JOIN attrsToDelete atd ON atd.ModelElementTypeAttributeId = meta.ModelElementAttributeTypeId


-- add new attributes and attributes whose types were changed
INSERT INTO Analyst.ModelElementTypeAttributes(Name, ModelElementTypeId, ModelElementAttributeTypeId, ExtendedProperties)
SELECT aa.Name, @typeId, aa.ModelElementAttributeTypeId, aa.ExtendedProperties FROM #addedAttrs aa
UNION ALL
SELECT m.Name, m.ModelElementTypeId, m.ModelElementAttributeTypeIdNew, m.ExtendedPropertiesNew
FROM #modifiedAttrs m 
WHERE m.ExtendedProperties <> m.ExtendedPropertiesNew OR m.ModelElementAttributeTypeId <> m.ModelElementAttributeTypeIdNew


INSERT INTO Analyst.ModelElementAttributes(

	[ModelElementAttributeId] [int] NOT NULL IDENTITY(1,1),
	[RefPath] [nvarchar](max) NULL,
	[Value] [nvarchar](max) NULL,
	[ModelElementId] INT NOT NULL,
	[ModelElementTypeAttributeId] INT NOT NULL,
	[ObjectId]	INT	NOT NULL


DROP TABLE #modifiedAttrs
DROP TABLE #deletedAttrs
DROP TABLE #addedAttrs