CREATE TABLE [Adm].[RequestMessages](
	[MessageDbId] [int] NOT NULL IDENTITY(1,1) PRIMARY KEY,
	[MessageId] [uniqueidentifier] NOT NULL,
	[RequestId] [uniqueidentifier] NOT NULL,
	[Content] [nvarchar](max) NULL,
	[RequestForCoreType] [nvarchar](50) NOT NULL,
	[RequestProcessingMethod] [nvarchar](20) NOT NULL,
	[MessageFromId] [uniqueidentifier] NOT NULL,
	[MessageOriginName] [nvarchar](200) NULL,
	[MessageOriginId] [uniqueidentifier] NOT NULL,
	[MessageFromName] [nvarchar](200) NULL,
	[MessageToObjectId] [uniqueidentifier] NOT NULL,
	[MessageToProjectId] [uniqueidentifier] NULL,
	[MessageToObjectName] [nvarchar](200) NULL,
	[MessageType] [nvarchar](50) NOT NULL,
	[CreatedDateTime] [datetimeoffset](7) NOT NULL,
	--[TypeName] [nvarchar](128) NOT NULL,
	[Project_ProjectConfigId] [uniqueidentifier] NULL,
	[Received] BIT NOT NULL CONSTRAINT DF_Adm_RequestMessages_Received DEFAULT 0,
	[CustomerCode] NVARCHAR(200) NULL,
	[RequestFromUserId] INT NULL,
CONSTRAINT [FK_Adm_RequestMessages_Adm_ProjectConfigs_Project_ProjectConfigId] FOREIGN KEY([Project_ProjectConfigId])
REFERENCES [Adm].[ProjectConfigs] ([ProjectConfigId])
)
