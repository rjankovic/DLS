CREATE TABLE [Search].[TypeChildTypes]
(
	[TypeChildTypesId] INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[ParentType] NVARCHAR(300) NULL,
	[ChildType] NVARCHAR(300) NOT NULL
)
