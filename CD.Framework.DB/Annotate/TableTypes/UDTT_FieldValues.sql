CREATE TYPE [Annotate].[UDTT_FieldValues] AS TABLE
(
	ModelElementId INT, 
	FieldId INT,
	Value NVARCHAR(MAX)
)
