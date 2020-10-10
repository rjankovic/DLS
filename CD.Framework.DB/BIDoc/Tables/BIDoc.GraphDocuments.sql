CREATE TABLE [BIDoc].[GraphDocuments](
	[GraphDocumentId] [int] IDENTITY(1,1) NOT NULL,
	[Content] [nvarchar](max) NULL,
	[DocumentType] NVARCHAR(50) NOT NULL,
	[GraphNode_Id] [int] NULL,
 CONSTRAINT [PK_dbo.GraphDocuments] PRIMARY KEY CLUSTERED 
(
	[GraphDocumentId] ASC
),
CONSTRAINT [FK_BIDoc_GraphDocuments_BIDoc_BasicGraphInfoNodes_GraphNode_Id] FOREIGN KEY([GraphNode_Id])
REFERENCES [BIDoc].[BasicGraphNodes] ([BasicGraphNodeId])
)
