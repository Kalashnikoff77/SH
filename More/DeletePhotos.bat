@echo off
rem sqlcmd -E -S .\SQLEXPRESS -Q "ALTER DATABASE swinghouse SET SINGLE_USER WITH ROLLBACK IMMEDIATE; drop database swinghouse"

rem rmdir /s /Q Migrations
rem dotnet ef migrations add Start

rmdir /s /Q ..\UI\wwwroot\images\AccountsPhotos\1
rmdir /s /Q ..\UI\wwwroot\images\AccountsPhotos\2
rmdir /s /Q ..\UI\wwwroot\images\AccountsPhotos\3
rmdir /s /Q ..\UI\wwwroot\images\AccountsPhotos\4
rmdir /s /Q ..\UI\wwwroot\images\AccountsPhotos\5
rmdir /s /Q ..\UI\wwwroot\images\AccountsPhotos\6

rmdir /s /Q ..\UI\wwwroot\images\EventsPhotos\1
rmdir /s /Q ..\UI\wwwroot\images\EventsPhotos\2
rmdir /s /Q ..\UI\wwwroot\images\EventsPhotos\3

cd ..\Console\bin\Debug\net8.0\
Console.exe
