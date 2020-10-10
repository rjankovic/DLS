CREATE TABLE [Adm].[Users](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[Identity] NVARCHAR(300) NOT NULL,
	[DisplayName] NVARCHAR(MAX) NULL,
 CONSTRAINT [PK_Adm_Users] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
),
 CONSTRAINT[UI_Adm_Users] UNIQUE
 (
 [Identity]
 )
)