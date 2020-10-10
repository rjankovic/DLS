CREATE TABLE [BIDoc].[SequenceEndpointTypes](
	[SequenceEndpointTypeId] [int] NOT NULL IDENTITY(1,1),
	[TypeName] [nvarchar](255) NULL
	
 CONSTRAINT [PK_BIDoc_SequenceEndpointTypes] PRIMARY KEY CLUSTERED 
(
	[SequenceEndpointTypeId] ASC
)
)
