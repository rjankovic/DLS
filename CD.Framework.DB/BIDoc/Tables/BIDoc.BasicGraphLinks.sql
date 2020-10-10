CREATE TABLE [BIDoc].[BasicGraphLinks](
	[BasicGraphLinkId] [int] NOT NULL IDENTITY(1,1),
	[LinkType] NVARCHAR(50) NOT NULL,
	[NodeFromId] [int] NOT NULL,
	[NodeToId] [int] NOT NULL,
	[ExtendedProperties] NVARCHAR(MAX) NULL,
 CONSTRAINT [PK_dbo.BasicGraphLinks] PRIMARY KEY CLUSTERED 
(
	[BasicGraphLinkId] ASC
),
CONSTRAINT FK_BIDoc_BasciGraphLinks_NodeFromId FOREIGN KEY(NodeFromId) REFERENCES [BIDoc].[BasicGraphNodes]([BasicGraphNodeId]),
CONSTRAINT FK_BIDoc_BasciGraphLinks_NodeToId FOREIGN KEY(NodeToId) REFERENCES [BIDoc].[BasicGraphNodes]([BasicGraphNodeId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphLinks_NodeFromId] ON [BIDoc].[BasicGraphLinks]([NodeFromId] ASC, [LinkType] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphLinks_NodeToId] ON [BIDoc].[BasicGraphLinks]([NodeToId] ASC, [LinkType] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_BIDoc_BasicGraphLinks_LinkType]
ON [BIDoc].[BasicGraphLinks] ([LinkType])
INCLUDE ([NodeFromId],[NodeToId])
GO

