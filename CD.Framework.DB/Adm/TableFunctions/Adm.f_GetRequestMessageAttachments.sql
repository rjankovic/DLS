CREATE FUNCTION [Adm].[f_GetRequestMessageAttachments](
	@messageid UNIQUEIDENTIFIER
	)
RETURNS TABLE
AS RETURN(
SELECT 		[AttachmentId]
           ,[Type]
           ,[Name]
           ,[Uri]
           ,[MessageId]
           ,[OriginalRequestMessage_MessageId]
FROM [Adm].[RequestMessageAttachments] WHERE @messageid = MessageId    
)