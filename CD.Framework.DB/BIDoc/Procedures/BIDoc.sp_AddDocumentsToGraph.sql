CREATE PROCEDURE [BIDoc].[sp_AddDocumentsToGraph]
	@documents  [BIDoc].[UDTT_GraphDocuments] READONLY
AS
INSERT INTO [BIDoc].[GraphDocuments]
(
	[Content],
	[DocumentType],
	[GraphNode_Id]
)
SELECT
	[Content],
	[DocumentType],
	[GraphNode_Id]
FROM @documents
