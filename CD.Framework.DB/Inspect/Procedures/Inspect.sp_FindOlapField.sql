
CREATE PROCEDURE Inspect.sp_FindOlapField
    @cubePath NVARCHAR(MAX), 
	@fieldName NVARCHAR(MAX)	
AS
BEGIN

	DECLARE @CubePathEscaped NVARCHAR(MAX) 
	IF LEFT(@fieldName, LEN(N'[Measures]')) = N'[Measures]'
	BEGIN
		DECLARE @FieldNameTrimmed NVARCHAR(MAX) = SUBSTRING(@FieldName, LEN('[Measures].[') + 1, 1000)
		SET @FieldNameTrimmed = REPLACE(LEFT(@FieldNameTrimmed, LEN(@FieldNameTrimmed) - 1), '_', '\_')

		SELECT e.RefPath, e.ProjectConfigId, e.ModelElementId 
		FROM BIDoc.ModelElements e 
		WHERE 
		LEFT(e.RefPath, LEN(@cubePath)) = @cubePath 
		AND e.Type 
		IN (N'CD.DLS.Model.Mssql.Ssas.PhysicalMeasureElement', N'CD.DLS.Model.Mssql.Ssas.CubeCalculatedMeasureElement') 
		AND e.Caption = @FieldNameTrimmed

	END
	ELSE
	BEGIN
		DECLARE @dimName NVARCHAR(MAX) = (SELECT item FROM adm.f_SplitString(@fieldName, N'.') x WHERE rwn = 1)
		DECLARE @hierName NVARCHAR(MAX) = (SELECT item FROM adm.f_SplitString(@fieldName, N'.') x WHERE rwn = 2)
		DECLARE @levelName NVARCHAR(MAX) = (SELECT item FROM adm.f_SplitString(@fieldName, N'.') x WHERE rwn = 3)
	
	
		 IF LEFT(@dimName, 1) = N'['
			SET @dimName = RIGHT(LEFT(@dimName, LEN(@dimName)-1), LEN(@dimName)-2)
		IF LEFT(@hierName, 1) = N'['
			SET @hierName = RIGHT(LEFT(@hierName, LEN(@hierName)-1), LEN(@hierName)-2)
		IF LEFT(@levelName, 1) = N'['
			SET @levelName = RIGHT(LEFT(@levelName, LEN(@levelName)-1), LEN(@levelName)-2)

		--DECLARE @dimPath NVARCHAR(MAX) =@cubePath + N'/CubeDimension[@Name=''' + @dimName + N''']'
		--SELECT @dimPath

		IF @hierName = @levelName
		BEGIN
		SELECT hle.RefPath, hle.ProjectConfigId, hle.ModelElementId 
		FROM BIDoc.ModelElements e 
		INNER JOIN BIDoc.ModelLinks hll ON hll.ElementToId = e.ModelElementId AND hll.Type = N'parent'
		INNER JOIN BIDoc.ModelElements hle ON hle.ModelElementId = hll.ElementFromId

		WHERE 
		LEFT(e.RefPath, LEN(@cubePath)) = @cubePath 
		AND e.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement') 
		AND e.Caption = @dimName
		AND hle.Type IN (N'CD.DLS.Model.Mssql.Ssas.CubeDimensionAttributeElement') 
		AND hle.Caption = @hierName
		END

		ELSE
		BEGIN
		
		SELECT dae.RefPath, dae.ProjectConfigId, dae.ModelElementId
		FROM BIDoc.ModelElements cde 		
		-- database dimension
		INNER JOIN BIDoc.ModelLinks ddl ON ddl.ElementFromId = cde.ModelElementId AND ddl.Type = N'DatabaseDimension'
		-- children (attributes and hierarchies)
		INNER JOIN BIDoc.ModelLinks hl ON hl.ElementToId = ddl.ElementToId AND hl.Type = N'parent'
		INNER JOIN BIDoc.ModelElements he ON he.ModelElementId = hl.ElementFromId
		-- children (hierarchy levels)
		INNER JOIN BIDoc.ModelLinks hll ON hll.ElementToId = he.ModelElementId AND hll.Type = N'parent'
		INNER JOIN BIDoc.ModelElements hle ON hle.ModelElementId = hll.ElementFromId
		-- link to dimension attribute
		INNER JOIN BIDoc.ModelLinks dal ON dal.ElementFromId = hle.ModelElementId AND dal.Type = N'Attribute'
		INNER JOIN BIDoc.ModelElements dae ON dae.ModelElementId = dal.ElementToId
				
		WHERE 	
			LEFT(cde.RefPath, LEN(@cubePath)) = @cubePath 		
			AND cde.Type = N'CD.DLS.Model.Mssql.Ssas.CubeDimensionElement' 
			AND cde.Caption = @dimName
			AND he.Type = N'CD.DLS.Model.Mssql.Ssas.HierarchyElement'
			AND he.Caption = @hierName
			AND hle.Type = N'CD.DLS.Model.Mssql.Ssas.HierarchyLevelElement'
			AND hle.Caption = @levelName
			AND dae.Type = N'CD.DLS.Model.Mssql.Ssas.DimensionAttributeElement'		
		END		
	END

END
