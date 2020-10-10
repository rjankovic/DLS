CREATE TYPE [BIDoc].[UDTT_GraphDocuments] AS TABLE(
	[Content] [nvarchar](max) NULL,
	[DocumentType] NVARCHAR(50) NOT NULL,
	[GraphNode_Id] [int] NULL
)
