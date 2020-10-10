CREATE PROCEDURE [Adm].[sp_SaveBroadcastMessageSingleton]
	@BroadcastMessageId UNIQUEIDENTIFIER,
    @BroadcastMessageType NVARCHAR(200),
    @ProjectConfigId UNIQUEIDENTIFIER,
    @Active BIT,
	@Content NVARCHAR(MAX)
AS

BEGIN TRAN
IF(EXISTS(SELECT TOP 1 1 FROM adm.BroadcastMessages WHERE BroadcastMessageType = @BroadcastMessageType AND Active = 1))
BEGIN
SELECT 0 [Result]
END
ELSE
BEGIN
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
SELECT 1
END
COMMIT