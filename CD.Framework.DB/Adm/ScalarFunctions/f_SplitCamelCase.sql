CREATE FUNCTION [adm].[f_SplitCamelCase](@Temp NVARCHAR(MAX))
Returns NVARCHAR(MAX)
AS
Begin

    Declare @KeepValues as varchar(50)
    Set @KeepValues = '%[^ ][A-Z][a-z]%'
    While PatIndex(@KeepValues collate Latin1_General_Bin, @Temp) > 0
        Set @Temp = Stuff(@Temp, PatIndex(@KeepValues collate Latin1_General_Bin, @Temp) + 1, 0, ' ')

    Return @Temp
End