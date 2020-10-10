CREATE FUNCTION [Adm].[f_splitstring]
(	
	@vInputList nvarchar(max),
	@vDelimiter nvarchar(10) = ';'
)
RETURNS @List TABLE (item varchar(max),rwn int) 
AS
begin

 declare @Item varchar(8000);
 declare @i int = 1;

 while CHARINDEX(@vDelimiter,@vInputList,0)<>0
   begin
    Select
     @Item = rtrim(ltrim(substring(@vInputList,1,CHARINDEX(@vDelimiter,@vInputList,0)-1))),
	 @vInputList = ltrim(rtrim(substring(@vInputList,CHARINDEX(@vDelimiter,@vInputList,0)+Len(@vDelimiter),Len(@vInputList))));

	 if len(@Item)>0
	   insert into @List select @Item,@i;
     
	 set @i = @i + 1;

   end

 if len(@vInputList)>0
  insert into @List select @vInputList,@i;

 return;
 
end

GO