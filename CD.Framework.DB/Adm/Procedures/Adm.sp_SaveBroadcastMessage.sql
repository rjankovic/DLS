CREATE PROCEDURE [Adm].[sp_SaveBroadcastMessage]
	@BroadcastMessageId UNIQUEIDENTIFIER,
    @BroadcastMessageType NVARCHAR(200),
    @ProjectConfigId UNIQUEIDENTIFIER,
    @Active BIT,
	@Content NVARCHAR(MAX)
AS

INSERT INTO [Adm].[BroadcastMessages]
(
 BroadcastMessageId,
 BroadcastMessageType,
 ProjectConfigId,
 Active,
 Content
)
VALUES
(
 @BroadcastMessageId,
 @BroadcastMessageType,
 @ProjectConfigId,
 @Active,
 @Content
)