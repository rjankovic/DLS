CREATE PROCEDURE [BIDoc].[sp_SaveLineageGridHistory]
	
	@ProjectConfigId UNIQUEIDENTIFIER,
	@SourceRootElementPath NVARCHAR(MAX),
	@TargetRootElementPath NVARCHAR(MAX),
	@SourceElementType NVARCHAR(MAX),
	@TargetElementType NVARCHAR(MAX),
	@SourceRootElementId INT,
	@TargetRootElementId INT,
	@UserId INT

AS

INSERT INTO [BIDoc].[LineageGridHistory](
	[ProjectConfigId],
	[SourceRootElementPath],
	[TargetRootElementPath],
	[SourceElementType],
	[TargetElementType],
	[SourceRootElementId],
	[TargetRootElementId],
	[CreatedDateTime],
	[UserId]
)
VALUES(
	@ProjectConfigId,
	@SourceRootElementPath,
	@TargetRootElementPath,
	@SourceElementType,
	@TargetElementType,
	@SourceRootElementId,
	@TargetRootElementId,
	GETDATE(),
	@UserId
)