CREATE TABLE [Analyst].[ModelElementAttributeTypes](
	[ModelElementAttributeTypeId] [int] NOT NULL IDENTITY(1,1),
	[Name] [nvarchar](max) NOT NULL,
	[Code] [nvarchar](30)  NOT NULL,
 CONSTRAINT [PK_Analyst_ModelElementAttributeTypes] PRIMARY KEY CLUSTERED 
(
	[ModelElementAttributeTypeId] ASC
)

)
