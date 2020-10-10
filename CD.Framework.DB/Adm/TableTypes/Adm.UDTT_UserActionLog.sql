CREATE TYPE [Adm].[UDTT_UserActionLog] AS TABLE(
	[CreatedDate] [datetimeoffset](7),
	[EventType] NVARCHAR(MAX) NOT NULL,
	[UserId] INT NULL,
	[ApplicationName] NVARCHAR(MAX) NULL,
	[FrameworkElement] NVARCHAR(MAX) NULL,
	[DataContext] NVARCHAR(MAX) NULL,
	[ExtendedProperties] NVARCHAR(MAX) NULL
)
