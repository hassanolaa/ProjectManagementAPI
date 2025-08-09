IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'testdocker')
BEGIN
    CREATE DATABASE [testdocker]
END
GO

-- Use the database
USE [testdocker]
GO
