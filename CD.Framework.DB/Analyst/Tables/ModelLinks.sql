CREATE TABLE [Analyst].[ModelLinks](
	[ModelLinkId] [int] NOT NULL IDENTITY(1,1),
	[ElementFromId] [int] NOT NULL,
	[ElementToId] [int] NOT NULL,
	[ModelLinkTypeId] INT NOT NULL,
	[ModelElementTypeAttributeId] INT NULL,
	[ObjectId]			INT NOT NULL
 CONSTRAINT [PK_Analyst_ModelLinks] PRIMARY KEY CLUSTERED 
(
	[ModelLinkId] ASC
),
CONSTRAINT [FK_Analyst_ModelLinks_ElementFromId] FOREIGN KEY ([ElementFromId]) REFERENCES [Analyst].[ModelElements]([ModelElementId]),
CONSTRAINT [FK_Analyst_ModelLinks_ElementToId] FOREIGN KEY ([ElementToId]) REFERENCES [Analyst].[ModelElements]([ModelElementId]),
CONSTRAINT [FK_Analyst_ModelLinks_ModelLinkTypeId] FOREIGN KEY ([ModelLinkTypeId]) REFERENCES [Analyst].[ModelLinkTypes]([ModelLinkTypeId]),
CONSTRAINT [FK_Analyst_ModelLinks_ModelElementTypeAttributeId] FOREIGN KEY ([ModelElementTypeAttributeId]) REFERENCES [Analyst].[ModelElementTypeAttributes]([ModelElementTypeAttributeId]),
CONSTRAINT [FK_Analyst_ModelLinks_ObjectId] FOREIGN KEY ([ObjectId]) REFERENCES [Analyst].[Objects]([ObjectId]),
)
GO

CREATE NONCLUSTERED INDEX [IX_Analyst_ModelLinks_ElementFromId] ON [Analyst].[ModelLinks]([ElementFromId] ASC, [ModelLinkTypeId] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_Analyst_ModelLinks_ElementToId] ON [Analyst].[ModelLinks]([ElementToId] ASC, [ModelLinkTypeId] ASC)
GO
