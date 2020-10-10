CREATE TABLE [Annotate].[AnnotationViews]
(
	[AnnotationViewId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES [Adm].[ProjectConfigs]([ProjectConfigId]),
	[ViewName] NVARCHAR(255)
)
