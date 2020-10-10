CREATE FUNCTION [Adm].[f_SimpleJsonValue]
(
	@json nvarchar(MAX),
	@key nvarchar(MAX)
)
RETURNS nvarchar(MAX)
AS
BEGIN

IF LEN(@json) < 3
RETURN NULL

DECLARE @trim NVARCHAR(MAX) = REVERSE(LEFT(REVERSE(LEFT(@json, LEN(@json)-1)), LEN(@json)-3))
DECLARE @res NVARCHAR(MAX)

SELECT @res /*key_item ItemNumber, [1] [Key],*/  = [2] -- [Value]
FROM  
(
SELECT spl.rwn key_item, pt.item, pt.rwn FROM adm.f_splitstring(@trim, N',"') spl
OUTER APPLY adm.f_splitstring(spl.item, N'":') pt
) AS src
PIVOT  
(  
MAX(item)  
FOR rwn IN ([1], [2])  
) AS PivotTable  
WHERE [1] = @key

RETURN (REPLACE(@res,'"',''))

END