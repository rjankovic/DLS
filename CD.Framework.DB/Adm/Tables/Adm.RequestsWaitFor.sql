CREATE TABLE [Adm].[RequestsWaitFor](
	[RequestsWiaitForItemId] INT NOT NULL IDENTITY(1,1),
	[RequestId] [uniqueidentifier] NOT NULL,
	[WaitForRequestId] [uniqueidentifier] NOT NULL,
	[Active] [bit] NOT NULL CONSTRAINT DF_RequestsWaitFor_Active DEFAULT (1),
CONSTRAINT [PK_Adm_RequestsWaitFor] PRIMARY KEY ([RequestsWiaitForItemId])
)
GO
CREATE NONCLUSTERED INDEX IX_Adm_RequestsWaitFor_RequestId ON [Adm].[RequestsWaitFor]([RequestId]) WHERE [Active] = 1
GO
CREATE NONCLUSTERED INDEX IX_Adm_RequestsWaitFor_WaitForRequestId ON [Adm].[RequestsWaitFor]([WaitForRequestId]) WHERE [Active] = 1
GO