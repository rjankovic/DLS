CREATE PROCEDURE [Annotate].[f_GetHistory]
	@modelElementId INT,
	@projectConfigId UNIQUEIDENTIFIER
AS
    SELECT f.FieldName AS 'Field Name', fv.Value AS 'Value', cu.[DisplayName] AS 'Created By', uu.[DisplayName] AS 'Updated By',
    ae.Date AS 'Date', iif(ae.IsCurrentVersion = '1', N'Current', N'Old')  AS 'Current Version'
    FROM BIDoc.ModelElements me
    INNER JOIN Annotate.AnnotationElements ae ON ae.ModelElementId = me.ModelElementId
    INNER JOIN Annotate.FieldValues fv ON fv.AnnotationElementId = ae.AnnotationElementId
    INNER JOIN Annotate.Fields f ON f.FieldId = fv.FieldId
    INNER JOIN adm.Users cu ON cu.UserId = ae.CreatedBy
    INNER JOIN adm.Users uu ON uu.UserId = ae.UpdatedBy
    WHERE me.ProjectConfigId = @projectConfigId
    AND ae.ProjectConfigId = @projectConfigId
    AND f.ProjectConfigId = @projectConfigId
    AND me.ModelElementId = @modelElementId
    ORDER BY  Date DESC
	
GO