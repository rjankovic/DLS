CREATE PROCEDURE [BIDoc].[sp_RenameModelElement]
	@elementId INT
	,@newName NVARCHAR(MAX)
AS

UPDATE BIDoc.ModelElements SET Caption = @newName WHERE ModelElementId = @elementId