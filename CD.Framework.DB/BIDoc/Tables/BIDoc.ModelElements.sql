CREATE TABLE [BIDoc].[ModelElements](
	[ModelElementId] [int] NOT NULL IDENTITY(1,1),
	[ExtendedProperties] [nvarchar](max) NULL,
	[RefPath] [nvarchar](max) NULL,
	[Definition] [nvarchar](max) NULL,
	[Caption] [nvarchar](max) NULL,
	[Type] [nvarchar](255) NULL,
	[ProjectConfigId] [uniqueidentifier] NOT NULL,
	[RefPathPrefix] NVARCHAR(300) NULL,
	[RefPathIntervalStart] INT NOT NULL DEFAULT 0,
	[RefPathIntervalEnd] INT NOT NULL DEFAULT 0,
	[RefPathSuffix] NVARCHAR(300) NULL,
	[SubtreeContent] NVARCHAR(MAX) NULL
 CONSTRAINT [PK_dbo.BIDocModelElements] PRIMARY KEY CLUSTERED 
(
	[ModelElementId] ASC
),
CONSTRAINT [FK_BIDoc_ModelElements_ProjectConfigId] FOREIGN KEY ([ProjectConfigId]) REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),

)
GO

CREATE NONCLUSTERED INDEX [IX_ModelElements_ProjectConfigId] ON [BIDoc].[ModelElements] ([ProjectConfigId], [RefPathPrefix]) INCLUDE ([RefPath], [Type]) --WITH (ONLINE = ON)
GO

--CREATE NONCLUSTERED INDEX [IX_BIDoc_ModelElements_RefPathPrefix]
--ON [BIDoc].[ModelElements] ([RefPathPrefix]) INCLUDE ([RefPath], [Type])
--GO

CREATE NONCLUSTERED INDEX [IX_ModelElements_Project_Type]
ON [BIDoc].[ModelElements] ([ProjectConfigId],[Type])
INCLUDE ([ModelElementId])
GO

CREATE NONCLUSTERED INDEX [ModelElements_RefPath] 
ON [BIDoc].[ModelElements] ([ProjectConfigId]) INCLUDE ([RefPath]) 
WITH (ONLINE = ON)
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_ModelElements_ProjectConfigId_RefPathIntervalStart]
ON [BIDoc].[ModelElements] ([ProjectConfigId] ASC, [RefPathIntervalStart] ASC)
INCLUDE (
      [ExtendedProperties]
      ,[RefPath]
      ,[Caption]
      ,[Type])
GO

CREATE NONCLUSTERED INDEX [IX_BIDoc_ModelElements_ModelElementId_Type]
ON [BIDoc].[ModelElements] ([ModelElementId] ASC, [Type] ASC)
INCLUDE (
      [ExtendedProperties]
      ,[RefPath]
      ,[Caption])
GO