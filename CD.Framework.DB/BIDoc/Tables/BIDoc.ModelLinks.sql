CREATE TABLE [BIDoc].[ModelLinks](
	[ModelLinkId] [int] NOT NULL IDENTITY(1,1),
	[ElementFromId] [int] NOT NULL,
	[ElementToId] [int] NOT NULL,
	[Type] [nvarchar](255) NULL,
	[ExtendedProperties] [nvarchar](max) NULL,
 CONSTRAINT [PK_BIDoc_BIDocModelLinks] PRIMARY KEY CLUSTERED 
(
	[ModelLinkId] ASC
),
CONSTRAINT [FK_BIDoc_ModelLinks_ElementFromId] FOREIGN KEY ([ElementFromId]) REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
CONSTRAINT [FK_BIDoc_ModelLinks_ElementToId] FOREIGN KEY ([ElementToId]) REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_ModelLinks_ElementFromId] ON [BIDoc].[ModelLinks]([ElementFromId] ASC, [Type] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_BIDoc_ModelLinks_ElementToId] ON [BIDoc].[ModelLinks]([ElementToId] ASC, [Type] ASC)
GO