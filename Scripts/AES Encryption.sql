GO
Sp_configure 'clr enable',1
GO
EXEC sp_configure 'show advanced options', 1;
RECONFIGURE;

ALTER DATABASE UPCI SET TRUSTWORTHY ON;

CREATE ASSEMBLY AESEncryption
FROM 'C:\Users\JR\Desktop\EQB Projects\UPCI\AESEncryption\bin\Debug\AESEncryption.dll'  

WITH PERMISSION_SET = unsafe

--kapag di na create may mali dun sa DLL, 
 
CREATE FUNCTION dbo.Encrypt(@text NVARCHAR(MAX), @key NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME AESEncryption.[AESEncryption.AESEncryption].Encrypt

CREATE FUNCTION dbo.Decrypt(@text NVARCHAR(MAX), @key NVARCHAR(MAX))
RETURNS NVARCHAR(MAX)
AS EXTERNAL NAME AESEncryption.[AESEncryption.AESEncryption].Decrypt
 
 
EXEC sp_configure 'clr strict security', 1; 
