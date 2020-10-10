GO
CREATE TABLE #DataMessageData
(
[Message] NVARCHAR(30),
[Code] NVARCHAR(10)
)
INSERT INTO #DataMessageData
(
	[Message],
	[Code]
)
VALUES ('Truncation may occur', 'Truncat');

MERGE BIDoc.DataMessagesType  AS t 
USING #DataMessageData AS s
ON t.[DataMessageType] = s.[Message] AND t.[DataMessageCode] = s.[Code]
WHEN NOT MATCHED THEN INSERT([DataMessageType],[DataMessageCode])
VALUES(
[Message],
[Code]
)
WHEN MATCHED THEN UPDATE SET t.[DataMessageType] = s.[Message], t.[DataMessageCode] = s.[Code];