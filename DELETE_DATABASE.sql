-- Script to delete the DICIS database
-- Run this in SQL Server Management Studio or sqlcmd

USE master;
GO

-- Close all connections to the database
ALTER DATABASE DICIS SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Drop the database
DROP DATABASE DICIS;
GO

PRINT 'Database DICIS has been deleted successfully.';
GO
