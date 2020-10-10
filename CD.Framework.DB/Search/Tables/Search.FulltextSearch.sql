CREATE TABLE [Search].[FulltextSearch]
(
	[FulltextSearchId] INT NOT NULL IDENTITY(1,1) CONSTRAINT PK_FullTextSearch PRIMARY KEY,
	[ModelElementId] INT NULL CONSTRAINT FK_FullTextSearch_ModelElements FOREIGN KEY REFERENCES [BIDoc].[ModelElements]([ModelElementId]),
	[ElementName] NVARCHAR(MAX) NOT NULL,
	[ElementNameSplit] NVARCHAR(MAX) NOT NULL,
	[BusinessFields] NVARCHAR(MAX) NULL,
	[SearchPriority] INT NOT NULL DEFAULT (1),
	[TypeDescription] NVARCHAR(MAX) NULL,
	[DescriptiveRootPath] NVARCHAR(MAX) NULL,
	[ElementType] NVARCHAR(255) NULL,
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL,
	[RefPath] NVARCHAR(MAX) NOT NULL,
)

GO

GO  
CREATE FULLTEXT INDEX ON [Search].[FulltextSearch]
 (   
  ElementName  
     Language 1033,
  ElementNameSplit
     Language 1033,
  BusinessFields  
     Language 1033
 --,BusinessDescription   
 --    Language 1033       
 )   
  KEY INDEX PK_FullTextSearch   
      ON fulltext_default;  

GO


CREATE NONCLUSTERED INDEX IX_Search_FulltextSearch_ProjectConfigId ON [Search].[FulltextSearch] (ProjectConfigId)