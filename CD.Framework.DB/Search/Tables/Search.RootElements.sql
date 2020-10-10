CREATE TABLE [Search].[RootElements]
(
	[RootElementId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ProjectConfigId] UNIQUEIDENTIFIER NOT NULL,
	[ModelElementId] INT NOT NULL,
	[Caption] NVARCHAR(255) NOT NULL
)
