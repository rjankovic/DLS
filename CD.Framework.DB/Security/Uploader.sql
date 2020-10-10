CREATE ROLE [uploader]
GO

GRANT SELECT ON SCHEMA :: [stg] TO [uploader]
GO
GRANT INSERT ON SCHEMA :: [stg] TO [uploader]  
GO
GRANT EXEC ON SCHEMA :: [stg] TO [uploader] 
GO
GRANT SELECT ON SCHEMA :: [adm] TO [uploader]
GO
GRANT EXEC ON SCHEMA :: [adm] TO [uploader]
GO