CREATE TABLE [Adm].[UserActionLog](
	[UserActionLogId] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetimeoffset](7) CONSTRAINT DF_Adm_UserActionLog_CreatedDate DEFAULT GETDATE(),
	[EventType] NVARCHAR(MAX) NOT NULL,
	[UserId] INT NULL CONSTRAINT FK_Adm_UserActionLog_UserId FOREIGN KEY REFERENCES [Adm].[Users]([UserId]),
	[ApplicationName] NVARCHAR(MAX) NULL,
	[FrameworkElement] NVARCHAR(MAX) NULL,
	[DataContext] NVARCHAR(MAX) NULL,
	[ExtendedProperties] NVARCHAR(MAX) NULL
 CONSTRAINT [PK_Adm_UserActionLog] PRIMARY KEY CLUSTERED 
(
	[UserActionLogId] ASC
)
)
