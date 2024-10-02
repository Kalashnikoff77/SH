USE [master]
GO
/****** Object:  Database [SwingHouse]    Script Date: 02.10.2024 18:48:55 ******/
CREATE DATABASE [SwingHouse]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'SwingHouse', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\SwingHouse.mdf' , SIZE = 73728KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'SwingHouse_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\SwingHouse_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [SwingHouse] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [SwingHouse].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [SwingHouse] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SwingHouse] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SwingHouse] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SwingHouse] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SwingHouse] SET ARITHABORT OFF 
GO
ALTER DATABASE [SwingHouse] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [SwingHouse] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SwingHouse] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SwingHouse] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SwingHouse] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SwingHouse] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SwingHouse] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SwingHouse] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SwingHouse] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SwingHouse] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SwingHouse] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SwingHouse] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SwingHouse] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SwingHouse] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SwingHouse] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SwingHouse] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [SwingHouse] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SwingHouse] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SwingHouse] SET  MULTI_USER 
GO
ALTER DATABASE [SwingHouse] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SwingHouse] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SwingHouse] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SwingHouse] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [SwingHouse] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [SwingHouse] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [SwingHouse] SET QUERY_STORE = ON
GO
ALTER DATABASE [SwingHouse] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [SwingHouse]
GO
/****** Object:  FullTextCatalog [EventsCatalog]    Script Date: 02.10.2024 18:48:55 ******/
CREATE FULLTEXT CATALOG [EventsCatalog] WITH ACCENT_SENSITIVITY = ON
AS DEFAULT
GO
/****** Object:  UserDefinedFunction [dbo].[GetAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT *,
		(SELECT JSON_QUERY(dbo.GetUsersForAccountFunction(a.Id))) AS Users,
		(SELECT JSON_QUERY(dbo.GetCountriesAndRegionsFunction(a.RegionId))) AS Country,
		(SELECT JSON_QUERY(dbo.GetAvatarForAccountFunction(a.Id))) AS Avatar,
		(SELECT JSON_QUERY(dbo.GetLastVisitForAccountFunction(a.Id))) AS LastVisit
		FROM Accounts a
		WHERE a.Id = @AccountId
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetAvatarForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetAvatarForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT TOP 1 * FROM PhotosForAccounts WHERE AccountId = @AccountId AND IsAvatar = 1 AND IsDeleted = 0 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetCountriesAndRegionsFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetCountriesAndRegionsFunction] (
	@RegionId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT TOP 1 *,
		(SELECT JSON_QUERY((SELECT TOP 1 * FROM Regions r WHERE CountryId = c.Id and r.Id = @RegionId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER))) AS Region
		FROM Countries AS c
		WHERE c.Id = (SELECT TOP 1 CountryId FROM Regions WHERE Id = @RegionId)
		FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetHobbiesForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetHobbiesForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT h.* 
		FROM Hobbies h 
		JOIN HobbiesForAccounts ah ON ah.AccountId = @AccountId
		WHERE h.Id = ah.HobbyId 
		FOR JSON PATH)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetLastVisitForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetLastVisitForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT TOP 1 * FROM VisitsForAccounts v WHERE v.AccountId = @AccountId ORDER BY Id DESC FOR JSON PATH, WITHOUT_ARRAY_WRAPPER)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetPhotosForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetPhotosForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT * FROM PhotosForAccounts WHERE AccountId = @AccountId AND IsDeleted = 0 FOR JSON PATH)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetRelationsForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetRelationsForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT * FROM RelationsForAccounts
		WHERE SenderId = @AccountId OR RecipientId = @AccountId
		FOR JSON PATH)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetScheduleForEventFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetScheduleForEventFunction] (
	@EventId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (
		SELECT *,
		(SELECT JSON_QUERY((SELECT f.Id, f.Name FROM FeaturesForSchedules fs JOIN Features f ON f.Id = fs.FeatureId WHERE fs.ScheduleId = s.Id FOR JSON PATH))) AS Features
		FROM SchedulesForEvents s
		WHERE EventId = @EventId 
		FOR JSON PATH
		)

	RETURN (SELECT JSON_QUERY(@result));
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetSchedulesForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetSchedulesForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT * FROM SchedulesForAccounts WHERE AccountId = @AccountId AND IsDeleted = 0 FOR JSON PATH)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetUsersForAccountFunction]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetUsersForAccountFunction] (
	@AccountId INT
)

RETURNS NVARCHAR(MAX)
AS
BEGIN
	DECLARE @result NVARCHAR(MAX);

	SET @result = (SELECT * FROM Users WHERE AccountId = @AccountId AND IsDeleted = 0 FOR JSON PATH)

	RETURN (SELECT JSON_QUERY(@result));

END
GO
/****** Object:  Table [dbo].[SchedulesForAccounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulesForAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ScheduleId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
	[AccountGender] [tinyint] NULL,
	[PurchaseDate] [date] NOT NULL,
	[TicketCost] [int] NOT NULL,
	[IsPaid] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_SchedulesForAccounts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[SchedulesForAccountsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[SchedulesForAccountsView]
AS

SELECT *,
		(SELECT JSON_QUERY(dbo.GetAccountFunction(afs.AccountId))) AS Account
		FROM SchedulesForAccounts afs
GO
/****** Object:  Table [dbo].[Accounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[Email] [nvarchar](75) NOT NULL,
	[Password] [nvarchar](35) NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Informing] [varchar](255) NOT NULL,
	[IsConfirmed] [bit] NOT NULL,
	[RegionId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Accounts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Events]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Events](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](2048) NOT NULL,
	[AdminId] [int] NOT NULL,
	[RegionId] [int] NOT NULL,
	[Address] [nvarchar](150) NOT NULL,
	[MaxMen] [smallint] NULL,
	[MaxWomen] [smallint] NULL,
	[MaxPairs] [smallint] NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Events] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SchedulesForEvents]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulesForEvents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[Description] [nvarchar](2048) NULL,
	[StartDate] [datetime] NOT NULL,
	[EndDate] [datetime] NOT NULL,
	[CostMan] [int] NOT NULL,
	[CostWoman] [int] NOT NULL,
	[CostPair] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_EventsSchedules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[AdminsForEventsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




--- Получает список админов, у которых есть активные мероприятия + кол-во данных мероприятий

CREATE VIEW [dbo].[AdminsForEventsView]
AS
	SELECT a.Id, a.Name, COUNT(*) AS NumberOfEvents
	FROM Accounts a 
	JOIN Events e ON e.AdminId = a.Id
	JOIN SchedulesForEvents se ON se.EventId = e.Id
	GROUP BY a.Id, a.Name
GO
/****** Object:  Table [dbo].[FeaturesForSchedules]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeaturesForSchedules](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ScheduleId] [int] NOT NULL,
	[FeatureId] [int] NOT NULL,
 CONSTRAINT [PK_FeaturesForSchedules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Features]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Features](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](20) NOT NULL,
 CONSTRAINT [PK_Features] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[FeaturesForEventsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




--- Получает список админов, у которых есть активные мероприятия + кол-во данных мероприятий

CREATE VIEW [dbo].[FeaturesForEventsView]
AS
	SELECT f.Id, f.Name, COUNT(*) AS NumberOfEvents
	FROM Features f
	JOIN FeaturesForSchedules fs ON fs.FeatureId = f.Id
	GROUP BY f.Id, f.Name
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ReadDate] [datetime] NULL,
	[RecipientId] [int] NOT NULL,
	[Text] [nvarchar](255) NOT NULL,
	[SenderId] [int] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Notifications] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[NotificationsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE VIEW [dbo].[NotificationsView]
AS
SELECT *,
(SELECT JSON_QUERY(dbo.GetAccountFunction(n.SenderId))) AS Sender
FROM dbo.Notifications n
WHERE n.IsDeleted = 0
GO
/****** Object:  Table [dbo].[PhotosForAccounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhotosForAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[Comment] [nvarchar](40) NULL,
	[IsAvatar] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[AccountId] [int] NOT NULL,
 CONSTRAINT [PK_AccountsPhotos] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[PhotosForAccountsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO





CREATE VIEW [dbo].[PhotosForAccountsView]
AS
SELECT *,
(SELECT JSON_QUERY(dbo.GetAccountFunction(f.AccountId))) AS Account
FROM dbo.PhotosForAccounts f
WHERE f.IsDeleted = 0
GO
/****** Object:  Table [dbo].[DiscussionsForEvents]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DiscussionsForEvents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [nchar](10) NOT NULL,
	[SenderId] [int] NOT NULL,
	[RecipientId] [int] NULL,
	[DiscussionId] [int] NULL,
	[CreateDate] [datetime] NOT NULL,
	[Text] [nvarchar](2048) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_EventsDiscussions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[DiscussionsForEventsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[DiscussionsForEventsView]
AS

SELECT *,
(SELECT JSON_QUERY(dbo.GetAccountFunction(de.SenderId))) AS Sender,
(SELECT JSON_QUERY(dbo.GetAccountFunction(de.RecipientId))) AS Recipient
FROM [SwingHouse].[dbo].[DiscussionsForEvents] de
WHERE de.IsDeleted = 0
GO
/****** Object:  Table [dbo].[PhotosForEvents]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhotosForEvents](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[Comment] [nvarchar](100) NULL,
	[IsAvatar] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[EventId] [int] NOT NULL,
 CONSTRAINT [PK_EventsPhotos] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[EventsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE VIEW [dbo].[EventsView]
AS
SELECT *,
(SELECT JSON_QUERY(dbo.GetScheduleForEventFunction(e.Id))) AS Schedule,
(SELECT JSON_QUERY(dbo.GetAccountFunction(e.AdminId))) AS Admin,
(SELECT JSON_QUERY(dbo.GetCountriesAndRegionsFunction(e.RegionId))) AS Country,
(SELECT JSON_QUERY((SELECT TOP 1 * FROM PhotosForEvents f WHERE f.EventId = e.Id AND f.IsDeleted = 0 AND f.IsAvatar = 1 FOR JSON PATH, WITHOUT_ARRAY_WRAPPER))) AS Avatar,
(SELECT JSON_QUERY((SELECT * FROM PhotosForEvents f WHERE f.EventId = e.Id AND f.IsDeleted = 0 FOR JSON PATH))) AS Photos,
(SELECT COUNT(*) FROM DiscussionsForEvents de WHERE e.Id = de.EventId AND de.IsDeleted = 0) AS NumOfDiscussions
FROM Events e
WHERE e.IsDeleted = 0
GO
/****** Object:  View [dbo].[SchedulesForEventsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO











CREATE VIEW [dbo].[SchedulesForEventsView]
AS
SELECT se.*,
(SELECT JSON_QUERY((SELECT TOP 1 * FROM EventsView ev WHERE Id = se.EventId FOR JSON PATH, WITHOUT_ARRAY_WRAPPER))) AS Event,
(SELECT JSON_QUERY((SELECT f.* FROM FeaturesForSchedules fs JOIN Features f ON f.Id = fs.FeatureId WHERE ScheduleId = se.Id FOR JSON PATH))) AS Features,
(SELECT JSON_QUERY((SELECT * FROM SchedulesForAccountsView WHERE ScheduleId = se.Id AND IsDeleted = 0 FOR JSON PATH))) AS RegisteredAccounts,
(SELECT COUNT(*) FROM DiscussionsForEvents WHERE EventId = se.EventId) AS NumberOfDiscussions
FROM SchedulesForEvents se
WHERE se.IsDeleted = 0
GO
/****** Object:  Table [dbo].[Messages]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Messages](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[ReadDate] [datetime] NULL,
	[SenderId] [int] NOT NULL,
	[RecipientId] [int] NOT NULL,
	[Text] [nvarchar](max) NOT NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Messages] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[LastMessagesListView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE VIEW [dbo].[LastMessagesListView]
AS
SELECT *,
(SELECT JSON_QUERY(dbo.GetAccountFunction(m.SenderId))) AS Sender,
(SELECT JSON_QUERY(dbo.GetAccountFunction(m.RecipientId))) AS Recipient
FROM dbo.Messages m
WHERE m.IsDeleted = 0 AND m.Id IN (SELECT MAX(Id) FROM dbo.Messages GROUP BY LEAST(SenderId, RecipientId), GREATEST(SenderId, RecipientId))
GO
/****** Object:  View [dbo].[AccountsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE VIEW [dbo].[AccountsView]
AS

SELECT *,
	(SELECT JSON_QUERY((SELECT dbo.GetUsersForAccountFunction(a.Id)))) AS Users,
	(SELECT JSON_QUERY((SELECT dbo.GetCountriesAndRegionsFunction(a.RegionId)))) AS Country,
	(SELECT JSON_QUERY((SELECT dbo.GetAvatarForAccountFunction(a.Id)))) AS Avatar,
	(SELECT JSON_QUERY((SELECT dbo.GetPhotosForAccountFunction(a.Id)))) AS Photos,
	(SELECT JSON_QUERY((SELECT dbo.GetHobbiesForAccountFunction(a.Id)))) AS Hobbies,
	(SELECT JSON_QUERY((SELECT dbo.GetRelationsForAccountFunction(a.Id)))) AS Relations,
	(SELECT JSON_QUERY((SELECT dbo.GetSchedulesForAccountFunction(a.Id)))) AS Schedules,
	(SELECT JSON_QUERY((SELECT dbo.GetLastVisitForAccountFunction(a.Id)))) AS LastVisit
FROM Accounts AS a
WHERE a.IsDeleted = 0
GO
/****** Object:  Table [dbo].[Countries]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Countries](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](30) NOT NULL,
	[Order] [tinyint] NOT NULL,
 CONSTRAINT [PK_Countries] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Regions]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Regions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Number] [int] NOT NULL,
	[CountryId] [int] NOT NULL,
	[Order] [tinyint] NOT NULL,
 CONSTRAINT [PK_Regions] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[CountriesView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[CountriesView]
AS
SELECT *,
(SELECT * FROM Regions r WHERE r.CountryId = c.Id ORDER BY [Order] ASC, Name ASC FOR JSON PATH) AS Regions
FROM Countries c
GO
/****** Object:  Table [dbo].[AccountsWishLists]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccountsWishLists](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Comment] [nvarchar](75) NULL,
	[StartDate] [datetime] NOT NULL,
	[AccountId] [int] NOT NULL,
 CONSTRAINT [PK_AccountsWishLists] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[WishListView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE VIEW [dbo].[WishListView]
AS

	SELECT TOP (10) av.Id, Guid, Name, Users, Country, LastVisit, Avatar, Relations,
	wl.Comment
	FROM AccountsView av
	JOIN AccountsWishLists wl ON wl.AccountId = av.Id
	WHERE av.Avatar IS NOT NULL
	ORDER BY wl.StartDate DESC

GO
/****** Object:  View [dbo].[RegionsForEventsView]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





--- Получает список регионов, в которых есть активные мероприятия + кол-во данных мероприятий

CREATE VIEW [dbo].[RegionsForEventsView]
AS

	SELECT r.Id, r.Name, r.[Order], COUNT(*) AS NumberOfEvents
	FROM Regions r
	JOIN Events e ON e.RegionId = r.Id
	JOIN SchedulesForEvents se ON se.EventId = e.Id
	GROUP BY r.Name, r.Id, r.[Order]

GO
/****** Object:  Table [dbo].[EventsForAccounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventsForAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[EventId] [int] NOT NULL,
	[AccountId] [int] NOT NULL,
	[UserGender] [tinyint] NULL,
	[PurchaseDate] [date] NOT NULL,
	[TicketCost] [int] NOT NULL,
	[IsPaid] [bit] NOT NULL,
 CONSTRAINT [PK_AccountsEvents] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Hobbies]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Hobbies](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](75) NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_Hobbies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[HobbiesForAccounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[HobbiesForAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccountId] [int] NOT NULL,
	[HobbyId] [int] NOT NULL,
 CONSTRAINT [PK_AccountsHobbies] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RelationsForAccounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RelationsForAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime2](7) NOT NULL,
	[SenderId] [int] NOT NULL,
	[RecipientId] [int] NOT NULL,
	[Type] [smallint] NOT NULL,
	[IsConfirmed] [bit] NOT NULL,
 CONSTRAINT [PK_Relations] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Guid] [uniqueidentifier] NOT NULL,
	[AccountId] [int] NOT NULL,
	[BirthDate] [date] NOT NULL,
	[Name] [nvarchar](40) NOT NULL,
	[Gender] [tinyint] NOT NULL,
	[Height] [tinyint] NOT NULL,
	[Weight] [tinyint] NOT NULL,
	[About] [nvarchar](255) NULL,
	[IsDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[VisitsForAccounts]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VisitsForAccounts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CreateDate] [datetime] NOT NULL,
	[LastDate] [datetime] NOT NULL,
	[AccountId] [int] NOT NULL,
 CONSTRAINT [PK_AccountsVisits] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Accounts] ON 

INSERT [dbo].[Accounts] ([Id], [Guid], [Email], [Password], [Name], [Informing], [IsConfirmed], [RegionId], [IsDeleted]) VALUES (1, N'9b338051-d1e1-4704-a03d-9662ae911bda', N'test@admin', N'pass1', N'Администратор', N'{"IsNotification":true,"IsMessage":true}', 1, 1, 0)
INSERT [dbo].[Accounts] ([Id], [Guid], [Email], [Password], [Name], [Informing], [IsConfirmed], [RegionId], [IsDeleted]) VALUES (2, N'20fb68ce-bf42-468a-83dc-57709b3a64f4', N'oleg@mail.ru', N'pass2', N'Олег и Марина Мск', N'{"IsNotification":true,"IsMessage":true}', 1, 1, 0)
INSERT [dbo].[Accounts] ([Id], [Guid], [Email], [Password], [Name], [Informing], [IsConfirmed], [RegionId], [IsDeleted]) VALUES (3, N'8110581f-e40a-473a-b329-920c64a5ac45', N'sertan@mail.ru', N'pass3', N'SerTan6970', N'{"IsNotification":true,"IsMessage":true}', 1, 2, 0)
INSERT [dbo].[Accounts] ([Id], [Guid], [Email], [Password], [Name], [Informing], [IsConfirmed], [RegionId], [IsDeleted]) VALUES (4, N'6606ea6b-81c6-44e2-87e7-1eb31faf2132', N'ser@mail.ru', N'pass4', N'Толя Бой', N'{"IsNotification":true,"IsMessage":true}', 1, 2, 0)
INSERT [dbo].[Accounts] ([Id], [Guid], [Email], [Password], [Name], [Informing], [IsConfirmed], [RegionId], [IsDeleted]) VALUES (5, N'ca77be0e-6818-4624-b08a-8fccd345a6c1', N'krym@mail.ru', N'pass5', N'Елена Крым', N'{"IsNotification":true,"IsMessage":true}', 1, 3, 0)
INSERT [dbo].[Accounts] ([Id], [Guid], [Email], [Password], [Name], [Informing], [IsConfirmed], [RegionId], [IsDeleted]) VALUES (6, N'6563ab20-07dc-4eec-9028-eaca209555ea', N'para@mail.ru', N'pass6', N'Пара Неразлучники', N'{"IsNotification":true,"IsMessage":true}', 1, 2, 0)
SET IDENTITY_INSERT [dbo].[Accounts] OFF
GO
SET IDENTITY_INSERT [dbo].[AccountsWishLists] ON 

INSERT [dbo].[AccountsWishLists] ([Id], [Comment], [StartDate], [AccountId]) VALUES (1, N'Комментарий Олега и Марины', CAST(N'2024-04-24T13:12:59.540' AS DateTime), 2)
INSERT [dbo].[AccountsWishLists] ([Id], [Comment], [StartDate], [AccountId]) VALUES (2, N'Комментарий СерТана6970', CAST(N'2024-04-24T13:12:59.540' AS DateTime), 3)
INSERT [dbo].[AccountsWishLists] ([Id], [Comment], [StartDate], [AccountId]) VALUES (3, N'Комментарий Елена Крым', CAST(N'2024-04-24T13:12:59.540' AS DateTime), 5)
SET IDENTITY_INSERT [dbo].[AccountsWishLists] OFF
GO
SET IDENTITY_INSERT [dbo].[Countries] ON 

INSERT [dbo].[Countries] ([Id], [Name], [Order]) VALUES (1, N'Россия', 1)
INSERT [dbo].[Countries] ([Id], [Name], [Order]) VALUES (2, N'Беларусь', 2)
SET IDENTITY_INSERT [dbo].[Countries] OFF
GO
SET IDENTITY_INSERT [dbo].[DiscussionsForEvents] ON 

INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (1, N'1         ', 2, 3, NULL, CAST(N'2024-06-19T18:20:00.000' AS DateTime), N'Тестовое сообщение от Марины и Олега к СерТан', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (15, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T11:24:59.377' AS DateTime), N'А теперь ещё раз и со звуком', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (16, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T11:47:22.863' AS DateTime), N'Присоединяюсь к ребятам ))-удачной  дороги, давай уже быстрей приезжай ))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (17, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T11:47:30.580' AS DateTime), N'Удачной тебе дороги! Ждём тебя все здесь!', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (40, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T17:26:10.473' AS DateTime), N'Лёха, скоро Рекстон приедет во Владик! (Для справки)', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (41, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T17:26:26.430' AS DateTime), N'Олег, молодец, что поднял этот вопрос. Актуальный ) надо реально уже задуматься', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (42, N'1         ', 3, NULL, NULL, CAST(N'2024-06-21T17:27:03.747' AS DateTime), N'Кто что думал про встречу Нового года?
В этом году 27 декабря последний рабочий день.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (43, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T17:27:14.513' AS DateTime), N'все ты меня уже списал? Я же сказала подумаю до вечера', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (44, N'1         ', 3, NULL, NULL, CAST(N'2024-06-21T17:28:46.277' AS DateTime), N'Мы всегда на все согласны ))) только блин до Владика влом ехать', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (45, N'1         ', 2, NULL, NULL, CAST(N'2024-06-21T17:32:20.037' AS DateTime), N'Не то что мы, Люберчане! Мы хотя бы за 72 однушку сдаем.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (54, N'2         ', 3, NULL, NULL, CAST(N'2024-06-21T18:14:56.910' AS DateTime), N'Теперь аренда студии обойдется в среднем в 71 000₽ в месяц, однушки — в 73 000₽, а двушки — в 126 000₽. Основная причина подорожания аренды жилья в столице — дефицит предложения и высокий спрос.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (55, N'2         ', 4, NULL, NULL, CAST(N'2024-06-21T18:16:54.500' AS DateTime), N'Я не в курсе. Сами едем в первый раз...', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (154, N'1         ', 2, NULL, NULL, CAST(N'2024-07-03T17:24:14.927' AS DateTime), N'Да, караоке там хороший )). И плясать где есть ))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (155, N'2         ', 4, NULL, NULL, CAST(N'2024-07-16T18:45:30.233' AS DateTime), N'Все должны идти. Мы много где были, но в бане ещё не были. Поэтому нужно обязательно сходить. Плюс там караоке.
Так что назначайте дату.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (156, N'2         ', 5, NULL, NULL, CAST(N'2024-07-18T18:44:20.500' AS DateTime), N'А вы в баню без меня собираетесь что ли?', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (157, N'2         ', 3, NULL, NULL, CAST(N'2024-07-18T18:44:26.523' AS DateTime), N'Ну если есть ещё спальные места, то точно идти надо. Только Оля Лёшу вряд ли отпустит...', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (158, N'2         ', 6, NULL, NULL, CAST(N'2024-07-18T18:44:39.003' AS DateTime), N'По цене не знаю че там )), но можно узнать. Даже есть спальные места )) баня офигенная ))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (159, N'2         ', 3, NULL, NULL, CAST(N'2024-07-18T18:45:06.470' AS DateTime), N'Предлагаешь в баньку в ем сгонять? Ну, вариант. Назначай дату.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (160, N'2         ', 5, NULL, NULL, CAST(N'2024-07-18T18:45:11.400' AS DateTime), N'Это вообще предлагаю сделать девизом чата', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (161, N'2         ', 2, NULL, NULL, CAST(N'2024-07-18T18:45:33.233' AS DateTime), N'Запланировано на сегодня. 
У нас строгий координатор. 
Сначала канатка.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (162, N'2         ', 6, NULL, NULL, CAST(N'2024-07-18T18:45:52.387' AS DateTime), N'Дядька похож на твоего брата Виталика)) или наоборот ))или это борода виновата ))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (163, N'2         ', 6, NULL, NULL, CAST(N'2024-07-18T18:46:52.060' AS DateTime), N'Придется по ходу встречу с Высоцкими сдвигать ))). Но я буду )).', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (164, N'2         ', 6, NULL, NULL, CAST(N'2024-07-18T18:47:09.770' AS DateTime), N'И пивоварня Горьковская имеется', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (165, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T11:59:44.550' AS DateTime), N'Оставьте мне кусочек краба, пожалуйста 🥺 
Я не знаю во сколько мы приедем', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (166, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:00:00.973' AS DateTime), N'Приглашаем всех желающих в это воскресенье к нам на рыбу, икру и свежих крабов с пивом. Где-то после обеда. Точнее время напишу завтра.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (167, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:00:11.190' AS DateTime), N'Ура!!!😄в родные пенаты )) 
Помню тоже-всегда уезжали в куртках оттуда, а на материке жара ))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (168, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:00:59.037' AS DateTime), N'Ну да, здесь две команды: Локо и Адмирал в почете.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (169, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:01:22.653' AS DateTime), N'Негласный гимн Спартака.  Пожалуй это ее лучшая аудитория.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (170, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:02:01.480' AS DateTime), N'Ты прав. 100%. На Камчатке (тут говорят "На Камче"). И живут тут камчадалы. И мы с Камчатки.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (171, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:02:10.490' AS DateTime), N'Это мой дядька. Он родом из Камчатки. Нам тут всё организовал. Огонь мужик!!!', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (172, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:13:00.300' AS DateTime), N'Мужик в очках норм', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (173, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:13:11.867' AS DateTime), N'Там выше косметика в тему как раз!', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (174, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:13:21.077' AS DateTime), N'Брат всех женщин собрал)))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (175, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:13:30.427' AS DateTime), N'Думаю, вы будете отсыпаться ))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (176, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:15:26.510' AS DateTime), N'В 15:50. Пока ещё не знаю, в каком состоянии прилечу, поэтому планов на вечер пока нет. Завтра посмотрю по ситуации...', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (177, N'2         ', 3, NULL, NULL, CAST(N'2024-07-19T12:15:38.813' AS DateTime), N'Олег, чё во сколько прилетаете?', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (178, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:16:12.110' AS DateTime), N'На 10-й день на Камчатке от крабов уже рвёт и хочется тупо картошки с местной селёдкой.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (179, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T12:16:55.373' AS DateTime), N'Правильно Вика, Я тоже сваливаю', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (180, N'1         ', 2, NULL, NULL, CAST(N'2024-07-19T16:52:15.940' AS DateTime), N'А тебе с Андреем надо выходить)))) у него тоже свои формулировки отличные от правил русского языка)))))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (181, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T16:52:30.757' AS DateTime), N'Думаю, если мы сейчас будет поднимать вопрос русского языка , многие поплывут)). Так что я все-таки остаюсь за свою формулировку', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (182, N'2         ', 2, NULL, NULL, CAST(N'2024-07-19T16:52:43.643' AS DateTime), N'Остаюсь все равно при совсем мнении', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (183, N'1         ', 2, NULL, NULL, CAST(N'2024-07-22T18:04:29.830' AS DateTime), N'Крайними бывают плоть, Север, мера, срок и необходимость!', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (184, N'1         ', 2, NULL, NULL, CAST(N'2024-07-22T18:04:36.530' AS DateTime), N'В словах последний и прощальный нет ничего страшного))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (185, N'2         ', 4, NULL, NULL, CAST(N'2024-07-20T07:24:40.000' AS DateTime), N'Крайней может быть только плоть)', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (186, N'2         ', 5, NULL, NULL, CAST(N'2024-07-20T09:03:50.000' AS DateTime), N'Это три порции суши, более 1 кг блинов с мясом, паста с рисом, макароны с сыром, овсяные хлопья, 14 овсяных блинов и творог. И все это за один день.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (206, N'2         ', 2, NULL, NULL, CAST(N'2024-09-20T09:12:39.840' AS DateTime), N'При росте 185 см его вес составлял 158 кг. Ему было 36 лет', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (207, N'2         ', 2, NULL, NULL, CAST(N'2024-09-20T10:15:31.970' AS DateTime), N'У нас прощальный ужин. Завтра возвращаемся...', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (208, N'2         ', 2, NULL, NULL, CAST(N'2024-09-20T10:33:16.470' AS DateTime), N'В заявке указано, что под брендом Ohui планируется создавать кремы, парфюмы, уход для волос и другие косметические средства.', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (209, N'1         ', 2, NULL, NULL, CAST(N'2024-09-19T18:48:30.660' AS DateTime), N'Ну вот пусть и создают дальневосточную группу', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (210, N'1         ', 2, NULL, NULL, CAST(N'2024-09-19T18:48:39.400' AS DateTime), N'У них же с Андреем одно и тоже время))', 0)
INSERT [dbo].[DiscussionsForEvents] ([Id], [EventId], [SenderId], [RecipientId], [DiscussionId], [CreateDate], [Text], [IsDeleted]) VALUES (211, N'3         ', 2, NULL, NULL, CAST(N'2024-09-19T19:04:16.750' AS DateTime), N'Просто первое сообщение в обсуждении.', 0)
SET IDENTITY_INSERT [dbo].[DiscussionsForEvents] OFF
GO
SET IDENTITY_INSERT [dbo].[Events] ON 

INSERT [dbo].[Events] ([Id], [Guid], [Name], [Description], [AdminId], [RegionId], [Address], [MaxMen], [MaxWomen], [MaxPairs], [IsDeleted]) VALUES (1, N'e120a6ea-4843-4a97-8bda-05dc2f60b2e2', N'"В гостях у сказки"', N'Старая встреча друзей. Собираемся в Тверской области в доме отдыха. Есть сауна, чан для купания, мангал, шашлыки. Питание включено.', 2, 2, N'144 км. на Тверь', 10, 10, 5, 0)
INSERT [dbo].[Events] ([Id], [Guid], [Name], [Description], [AdminId], [RegionId], [Address], [MaxMen], [MaxWomen], [MaxPairs], [IsDeleted]) VALUES (2, N'ae10b515-5834-446a-a309-47329a2d4252', N'Рамирас встреча в сказке', N'Встреча в квартире у метро Сухаревская.', 4, 1, N'Метро Сухаревская', 20, 20, 10, 0)
INSERT [dbo].[Events] ([Id], [Guid], [Name], [Description], [AdminId], [RegionId], [Address], [MaxMen], [MaxWomen], [MaxPairs], [IsDeleted]) VALUES (3, N'b146b8be-83a9-4da9-900f-93c3e8bf5e82', N'Баня у Серёги', N'Под Нарой', 3, 1, N'У реки Нара Минск', 2, 2, 2, 0)
INSERT [dbo].[Events] ([Id], [Guid], [Name], [Description], [AdminId], [RegionId], [Address], [MaxMen], [MaxWomen], [MaxPairs], [IsDeleted]) VALUES (6, N'898357db-1f52-429e-b389-4ce4e81fba77', N'Минские встречи', N'Встречаемся в центре Минска', 2, 3, N'Станция метро "Октябрьская"', 20, 20, 10, 0)
SET IDENTITY_INSERT [dbo].[Events] OFF
GO
SET IDENTITY_INSERT [dbo].[EventsForAccounts] ON 

INSERT [dbo].[EventsForAccounts] ([Id], [EventId], [AccountId], [UserGender], [PurchaseDate], [TicketCost], [IsPaid]) VALUES (1, 1, 4, 0, CAST(N'2024-08-15' AS Date), 1000, 0)
INSERT [dbo].[EventsForAccounts] ([Id], [EventId], [AccountId], [UserGender], [PurchaseDate], [TicketCost], [IsPaid]) VALUES (2, 1, 2, NULL, CAST(N'2024-08-10' AS Date), 500, 1)
INSERT [dbo].[EventsForAccounts] ([Id], [EventId], [AccountId], [UserGender], [PurchaseDate], [TicketCost], [IsPaid]) VALUES (5, 1, 5, 1, CAST(N'2024-08-05' AS Date), 0, 0)
INSERT [dbo].[EventsForAccounts] ([Id], [EventId], [AccountId], [UserGender], [PurchaseDate], [TicketCost], [IsPaid]) VALUES (6, 1, 3, NULL, CAST(N'2024-08-07' AS Date), 500, 0)
INSERT [dbo].[EventsForAccounts] ([Id], [EventId], [AccountId], [UserGender], [PurchaseDate], [TicketCost], [IsPaid]) VALUES (7, 3, 1, 0, CAST(N'2024-05-05' AS Date), 750, 1)
SET IDENTITY_INSERT [dbo].[EventsForAccounts] OFF
GO
SET IDENTITY_INSERT [dbo].[Features] ON 

INSERT [dbo].[Features] ([Id], [Name]) VALUES (1, N'Баня')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (2, N'Сауна')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (3, N'Диско-80-90-00')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (4, N'Конкурсы')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (5, N'Бассейн')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (6, N'Стриптиз')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (7, N'Массаж')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (8, N'Фотосессии')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (9, N'С питанием')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (10, N'С алкоголем')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (11, N'Gang-bang')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (12, N'Go-go')
INSERT [dbo].[Features] ([Id], [Name]) VALUES (13, N'С ночёвкой')
SET IDENTITY_INSERT [dbo].[Features] OFF
GO
SET IDENTITY_INSERT [dbo].[FeaturesForSchedules] ON 

INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (1, 1, 1)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (2, 1, 3)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (3, 1, 9)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (5, 2, 4)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (6, 2, 7)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (8, 3, 8)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (10, 5, 3)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (11, 6, 3)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (12, 7, 5)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (13, 7, 6)
INSERT [dbo].[FeaturesForSchedules] ([Id], [ScheduleId], [FeatureId]) VALUES (16, 7, 4)
SET IDENTITY_INSERT [dbo].[FeaturesForSchedules] OFF
GO
SET IDENTITY_INSERT [dbo].[Hobbies] ON 

INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (1, N'Хоккей', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (2, N'Футбол', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (3, N'Шашлыки', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (4, N'Кино, театры', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (5, N'Выставки, музеи', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (6, N'Велоспорт', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (7, N'Путешествия', NULL)
INSERT [dbo].[Hobbies] ([Id], [Name], [Description]) VALUES (8, N'Сауна, баня', NULL)
SET IDENTITY_INSERT [dbo].[Hobbies] OFF
GO
SET IDENTITY_INSERT [dbo].[HobbiesForAccounts] ON 

INSERT [dbo].[HobbiesForAccounts] ([Id], [AccountId], [HobbyId]) VALUES (1, 3, 1)
INSERT [dbo].[HobbiesForAccounts] ([Id], [AccountId], [HobbyId]) VALUES (2, 3, 3)
INSERT [dbo].[HobbiesForAccounts] ([Id], [AccountId], [HobbyId]) VALUES (40, 2, 3)
INSERT [dbo].[HobbiesForAccounts] ([Id], [AccountId], [HobbyId]) VALUES (43, 2, 7)
INSERT [dbo].[HobbiesForAccounts] ([Id], [AccountId], [HobbyId]) VALUES (44, 2, 8)
SET IDENTITY_INSERT [dbo].[HobbiesForAccounts] OFF
GO
SET IDENTITY_INSERT [dbo].[Messages] ON 

INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (1, CAST(N'2024-01-01T00:00:00.000' AS DateTime), CAST(N'2024-04-24T13:36:56.310' AS DateTime), 3, 2, N'СерТан6970 - Олегу - 1', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (2, CAST(N'2024-01-02T00:00:00.000' AS DateTime), NULL, 4, 5, N'Сергей - Елене Крым', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (3, CAST(N'2024-01-03T00:00:00.000' AS DateTime), CAST(N'2024-04-24T13:36:56.310' AS DateTime), 3, 2, N'СерТан6970 - Олегу - 2', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (4, CAST(N'2024-01-04T00:00:00.000' AS DateTime), NULL, 2, 5, N'Олег - Елене Крым - 1', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (5, CAST(N'2024-01-05T00:00:00.000' AS DateTime), CAST(N'2024-06-20T17:39:39.147' AS DateTime), 2, 3, N'Олег - СерТан6970 - 1', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (6, CAST(N'2024-01-06T00:00:00.000' AS DateTime), CAST(N'2024-05-17T17:14:06.680' AS DateTime), 5, 2, N'Елена Крым - Олегу - 1', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (7, CAST(N'2024-05-14T13:17:34.810' AS DateTime), CAST(N'2024-06-20T17:39:39.147' AS DateTime), 2, 3, N'Сообщение от Олега к СерТан', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (8, CAST(N'2024-05-14T13:21:12.270' AS DateTime), CAST(N'2024-06-20T17:39:39.147' AS DateTime), 2, 3, N'Сообщение от Олега к СерТан-2', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (9, CAST(N'2024-05-14T13:22:05.400' AS DateTime), CAST(N'2024-06-20T17:39:39.147' AS DateTime), 2, 3, N'Сообщение от Олега к СерТан-3', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (10, CAST(N'2024-06-14T13:59:09.103' AS DateTime), CAST(N'2024-06-20T17:39:39.147' AS DateTime), 2, 3, N'3123123', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (11, CAST(N'2024-06-20T17:17:25.197' AS DateTime), NULL, 2, 5, N'Я - Елене', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (12, CAST(N'2024-06-20T17:19:12.097' AS DateTime), NULL, 2, 5, N'Я - Елене', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (13, CAST(N'2024-06-20T17:39:59.517' AS DateTime), CAST(N'2024-06-20T17:40:02.197' AS DateTime), 3, 2, N'55555555555555', 0)
INSERT [dbo].[Messages] ([Id], [CreateDate], [ReadDate], [SenderId], [RecipientId], [Text], [IsDeleted]) VALUES (14, CAST(N'2024-06-20T17:40:16.263' AS DateTime), CAST(N'2024-06-20T17:40:18.303' AS DateTime), 3, 2, N'Сертан - Мне', 0)
SET IDENTITY_INSERT [dbo].[Messages] OFF
GO
SET IDENTITY_INSERT [dbo].[Notifications] ON 

INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (1, CAST(N'2014-01-01T00:00:00.000' AS DateTime), CAST(N'2024-08-06T12:08:30.200' AS DateTime), 2, N'От Админа к Олегу', 1, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (2, CAST(N'2014-01-03T12:40:00.000' AS DateTime), CAST(N'2024-08-05T21:57:26.727' AS DateTime), 2, N'От СерТан6970 к Олегу', 3, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (3, CAST(N'2014-01-04T00:00:00.000' AS DateTime), NULL, 5, N'От Олега к Елене Крым', 2, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (4, CAST(N'2024-05-14T16:29:31.117' AS DateTime), CAST(N'2024-08-06T17:16:44.867' AS DateTime), 2, N'Уведомление от Олега к СерТан', 2, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (500, CAST(N'2024-07-19T16:53:12.397' AS DateTime), CAST(N'2024-08-06T12:34:57.000' AS DateTime), 2, N' подписались на Вас.', 1, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (501, CAST(N'2024-05-19T16:53:12.950' AS DateTime), CAST(N'2024-08-06T12:34:57.000' AS DateTime), 2, N' отписались от Вас.', 2, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (502, CAST(N'2024-08-05T16:53:23.923' AS DateTime), CAST(N'2024-08-06T12:34:57.000' AS DateTime), 2, N' Вас заблокировали.', 5, 0)
INSERT [dbo].[Notifications] ([Id], [CreateDate], [ReadDate], [RecipientId], [Text], [SenderId], [IsDeleted]) VALUES (503, CAST(N'2024-08-06T12:53:24.753' AS DateTime), CAST(N'2024-08-06T12:34:57.000' AS DateTime), 2, N' Вас разблокировали.', 3, 0)
SET IDENTITY_INSERT [dbo].[Notifications] OFF
GO
SET IDENTITY_INSERT [dbo].[PhotosForAccounts] ON 

INSERT [dbo].[PhotosForAccounts] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [AccountId]) VALUES (1, N'a7f7fdb2-fc35-415b-987d-73f2d14974da', N'Администратор', 1, 0, 1)
INSERT [dbo].[PhotosForAccounts] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [AccountId]) VALUES (2, N'51db5093-ff1c-4681-a9f3-ea6c47c859ac', N'Олег и Марина Мск', 1, 0, 2)
INSERT [dbo].[PhotosForAccounts] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [AccountId]) VALUES (3, N'7f4d5503-b5ac-4315-b1b9-7b8abc59b24f', N'SerTan6970', 1, 0, 3)
INSERT [dbo].[PhotosForAccounts] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [AccountId]) VALUES (4, N'a6d2ca7e-7370-4549-b8cd-25766ea5314b', N'Толя Бой', 1, 0, 4)
INSERT [dbo].[PhotosForAccounts] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [AccountId]) VALUES (5, N'6a1ddb45-7aa1-4e78-be9e-b21b75912dfc', N'Елена Крым', 1, 0, 5)
INSERT [dbo].[PhotosForAccounts] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [AccountId]) VALUES (6, N'068958e7-f06d-48d8-a3be-bdb76c540f44', N'Пара Неразлучники', 1, 0, 6)
SET IDENTITY_INSERT [dbo].[PhotosForAccounts] OFF
GO
SET IDENTITY_INSERT [dbo].[PhotosForEvents] ON 

INSERT [dbo].[PhotosForEvents] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [EventId]) VALUES (75, N'd3bc5fe1-537b-4b5f-930f-bcea7c54c2ae', N'"В гостях у сказки"', 1, 0, 1)
INSERT [dbo].[PhotosForEvents] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [EventId]) VALUES (76, N'8f55194e-049d-4dbe-9cdf-3880ca30ab13', N'Рамирас встреча в сказке', 1, 0, 2)
INSERT [dbo].[PhotosForEvents] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [EventId]) VALUES (77, N'2010d8ee-c9f8-4802-aa35-6a00e48ffa6f', N'Баня у Серёги', 1, 0, 3)
INSERT [dbo].[PhotosForEvents] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [EventId]) VALUES (78, N'46e8e154-d9cb-4641-a919-8a0055039abd', N'Минские встречи', 1, 0, 6)
INSERT [dbo].[PhotosForEvents] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [EventId]) VALUES (79, N'0010d8ee-c9f8-4802-aa35-6a00e48ffa61', N'Вторая фотка', 0, 0, 2)
INSERT [dbo].[PhotosForEvents] ([Id], [Guid], [Comment], [IsAvatar], [IsDeleted], [EventId]) VALUES (80, N'16e8e154-d9cb-4641-a919-8a0055039ab1', N'Третья фотка', 0, 0, 2)
SET IDENTITY_INSERT [dbo].[PhotosForEvents] OFF
GO
SET IDENTITY_INSERT [dbo].[Regions] ON 

INSERT [dbo].[Regions] ([Id], [Name], [Number], [CountryId], [Order]) VALUES (1, N'Москва и область', 77, 1, 1)
INSERT [dbo].[Regions] ([Id], [Name], [Number], [CountryId], [Order]) VALUES (2, N'Тверь', 69, 1, 69)
INSERT [dbo].[Regions] ([Id], [Name], [Number], [CountryId], [Order]) VALUES (3, N'Минск и область', 7, 2, 100)
INSERT [dbo].[Regions] ([Id], [Name], [Number], [CountryId], [Order]) VALUES (5, N'Санкт-Петербург и область', 78, 1, 2)
SET IDENTITY_INSERT [dbo].[Regions] OFF
GO
SET IDENTITY_INSERT [dbo].[RelationsForAccounts] ON 

INSERT [dbo].[RelationsForAccounts] ([Id], [CreateDate], [SenderId], [RecipientId], [Type], [IsConfirmed]) VALUES (243, CAST(N'2024-07-18T17:16:59.1966667' AS DateTime2), 3, 2, 20, 1)
SET IDENTITY_INSERT [dbo].[RelationsForAccounts] OFF
GO
SET IDENTITY_INSERT [dbo].[SchedulesForAccounts] ON 

INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (3, 2, 3, NULL, CAST(N'2024-08-06' AS Date), 500, 0, 0)
INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (4, 3, 3, NULL, CAST(N'2024-08-06' AS Date), 500, 0, 0)
INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (5, 3, 4, 0, CAST(N'2024-08-10' AS Date), 2000, 0, 0)
INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (6, 3, 5, 1, CAST(N'2024-08-10' AS Date), 0, 0, 0)
INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (7, 7, 4, 0, CAST(N'2024-08-25' AS Date), 0, 0, 0)
INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (9, 2, 4, 0, CAST(N'2024-10-02' AS Date), 0, 0, 0)
INSERT [dbo].[SchedulesForAccounts] ([Id], [ScheduleId], [AccountId], [AccountGender], [PurchaseDate], [TicketCost], [IsPaid], [IsDeleted]) VALUES (10, 2, 5, 0, CAST(N'2024-10-02' AS Date), 0, 0, 0)
SET IDENTITY_INSERT [dbo].[SchedulesForAccounts] OFF
GO
SET IDENTITY_INSERT [dbo].[SchedulesForEvents] ON 

INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (1, 1, N'0120a6ea-4843-4a97-8bda-05dc2f60b2e2', N'17.08. Будем купаться в чане на костре. Есть также комната общего пользования. Шашлыки!!!', CAST(N'2024-08-17T13:00:00.000' AS DateTime), CAST(N'2024-09-03T15:00:00.000' AS DateTime), 7000, 0, 3500, 0)
INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (2, 2, N'1120a6ea-4843-4a97-8bda-05dc2f60b2e2', N'Рамирас 05 августа. Квартирник!', CAST(N'2024-08-05T21:00:00.000' AS DateTime), CAST(N'2024-08-31T11:00:00.000' AS DateTime), 5000, 0, 2000, 0)
INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (3, 2, N'd05016cb-351f-4087-8918-c2c20a44a624', N'Рамирас 07 сентября. Сауна!', CAST(N'2024-09-07T10:00:00.000' AS DateTime), CAST(N'2024-09-07T23:00:00.000' AS DateTime), 5000, 0, 2000, 0)
INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (5, 3, N'97d8e571-22b5-40b1-8d6b-d8079a9f77c0', N'Встреча у Серёги с 14.09 по 16.09', CAST(N'2024-09-14T20:00:00.000' AS DateTime), CAST(N'2024-09-16T20:00:00.000' AS DateTime), 0, 0, 0, 0)
INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (6, 3, N'a4172b18-ca2d-4405-87d3-a50ea6500ebf', N'Встреча у Серёги с 21.08 по 07.09', CAST(N'2024-08-21T20:00:00.000' AS DateTime), CAST(N'2024-09-07T20:00:00.000' AS DateTime), 500, 250, 100, 0)
INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (7, 3, N'8b034738-56d5-49d3-a23b-527502321991', N'Встреча у Серёги с 01.08 по 03.08', CAST(N'2024-08-01T12:00:00.000' AS DateTime), CAST(N'2024-08-03T18:00:00.000' AS DateTime), 0, 0, 0, 0)
INSERT [dbo].[SchedulesForEvents] ([Id], [EventId], [Guid], [Description], [StartDate], [EndDate], [CostMan], [CostWoman], [CostPair], [IsDeleted]) VALUES (9, 6, N'f66cf918-7c1f-4ba0-9a1f-d9b59b847732', N'05.09. Отличная встреча для всех желающих', CAST(N'2024-09-05T00:00:00.000' AS DateTime), CAST(N'2024-09-07T00:00:00.000' AS DateTime), 1000, 0, 500, 0)
SET IDENTITY_INSERT [dbo].[SchedulesForEvents] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (1, N'50882363-7040-4212-b088-d9db0c2cc2ff', 1, CAST(N'1977-07-07' AS Date), N'Админ', 0, 170, 70, NULL, 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (4, N'778cddd5-96f5-4bef-bb2d-079f2df13887', 3, CAST(N'1969-02-01' AS Date), N'Сергей', 0, 165, 110, N'Строю дома, вожусь в огороде, работящий', 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (5, N'5a994d5f-0105-4c7c-afb6-da6fd95a52b2', 3, CAST(N'1970-02-11' AS Date), N'Татьяна', 1, 169, 78, N'Занимаюсь домохозяйством, топлю баню', 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (6, N'1d0924ce-3378-471d-9a3e-0b41949e93a6', 4, CAST(N'1999-02-01' AS Date), N'Толян', 0, 168, 100, NULL, 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (7, N'a8bfd0e2-faa1-4260-ac97-28eb864313a4', 5, CAST(N'1969-02-01' AS Date), N'Елена', 1, 171, 75, NULL, 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (8, N'87af8135-1bd5-46b9-82e2-01d64bcded21', 6, CAST(N'1978-05-05' AS Date), N'Карина', 1, 165, 83, NULL, 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (9, N'ee3381ab-1408-4fd7-99bc-2f81b86b5d8b', 6, CAST(N'1976-06-05' AS Date), N'Сергей', 0, 170, 110, NULL, 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (33, N'fa5cf194-6fc6-4a47-aa62-df8dc40a2ad8', 2, CAST(N'1977-01-29' AS Date), N'Олег', 0, 155, 95, N'Об Олеге2', 0)
INSERT [dbo].[Users] ([Id], [Guid], [AccountId], [BirthDate], [Name], [Gender], [Height], [Weight], [About], [IsDeleted]) VALUES (35, N'3e9543f0-9883-43c8-9e71-8e8c7329a01d', 2, CAST(N'1969-07-01' AS Date), N'Марина', 1, 178, 95, N'О Марине', 0)
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[VisitsForAccounts] ON 

INSERT [dbo].[VisitsForAccounts] ([Id], [CreateDate], [LastDate], [AccountId]) VALUES (1, CAST(N'2023-01-01T00:00:00.000' AS DateTime), CAST(N'2023-01-01T00:00:00.000' AS DateTime), 1)
INSERT [dbo].[VisitsForAccounts] ([Id], [CreateDate], [LastDate], [AccountId]) VALUES (2, CAST(N'2023-12-01T00:00:00.000' AS DateTime), CAST(N'2024-10-02T18:45:32.727' AS DateTime), 2)
INSERT [dbo].[VisitsForAccounts] ([Id], [CreateDate], [LastDate], [AccountId]) VALUES (3, CAST(N'2023-12-02T00:00:00.000' AS DateTime), CAST(N'2024-07-24T18:16:33.513' AS DateTime), 3)
INSERT [dbo].[VisitsForAccounts] ([Id], [CreateDate], [LastDate], [AccountId]) VALUES (4, CAST(N'2023-12-03T00:00:00.000' AS DateTime), CAST(N'2023-12-03T00:00:00.000' AS DateTime), 4)
INSERT [dbo].[VisitsForAccounts] ([Id], [CreateDate], [LastDate], [AccountId]) VALUES (5, CAST(N'2023-12-04T00:00:00.000' AS DateTime), CAST(N'2023-12-04T00:00:00.000' AS DateTime), 5)
INSERT [dbo].[VisitsForAccounts] ([Id], [CreateDate], [LastDate], [AccountId]) VALUES (6, CAST(N'2023-12-05T00:00:00.000' AS DateTime), CAST(N'2023-12-05T00:00:00.000' AS DateTime), 6)
SET IDENTITY_INSERT [dbo].[VisitsForAccounts] OFF
GO
/****** Object:  Index [IX_Accounts_RegionId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_Accounts_RegionId] ON [dbo].[Accounts]
(
	[RegionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AccountsWishLists_AccountId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_AccountsWishLists_AccountId] ON [dbo].[AccountsWishLists]
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Events_AdminId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_Events_AdminId] ON [dbo].[Events]
(
	[AdminId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Events_RegionId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_Events_RegionId] ON [dbo].[Events]
(
	[RegionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Notifications_SenderId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_Notifications_SenderId] ON [dbo].[Notifications]
(
	[SenderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AccountsPhotos_AccountId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_AccountsPhotos_AccountId] ON [dbo].[PhotosForAccounts]
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventsPhotos_EventId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_EventsPhotos_EventId] ON [dbo].[PhotosForEvents]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Regions_CountryId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE NONCLUSTERED INDEX [IX_Regions_CountryId] ON [dbo].[Regions]
(
	[CountryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_AccountsVisits_AccountId]    Script Date: 02.10.2024 18:48:55 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_AccountsVisits_AccountId] ON [dbo].[VisitsForAccounts]
(
	[AccountId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF__Accounts__Guid__02FC7413]  DEFAULT (newid()) FOR [Guid]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_IsConfirmed]  DEFAULT ((0)) FOR [IsConfirmed]
GO
ALTER TABLE [dbo].[Accounts] ADD  CONSTRAINT [DF_Accounts_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[AccountsWishLists] ADD  CONSTRAINT [DF__AccountsW__Start__07C12930]  DEFAULT (getdate()) FOR [StartDate]
GO
ALTER TABLE [dbo].[Countries] ADD  CONSTRAINT [DF_Countries_Order]  DEFAULT ((100)) FOR [Order]
GO
ALTER TABLE [dbo].[DiscussionsForEvents] ADD  CONSTRAINT [DF_Table_1_Date]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[DiscussionsForEvents] ADD  CONSTRAINT [DF_EventsDiscussions_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Events] ADD  CONSTRAINT [DF__Events__Guid__08B54D69]  DEFAULT (newid()) FOR [Guid]
GO
ALTER TABLE [dbo].[Events] ADD  CONSTRAINT [DF_Events_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[EventsForAccounts] ADD  CONSTRAINT [DF_AccountsEvents_PurchaseDate]  DEFAULT (getdate()) FOR [PurchaseDate]
GO
ALTER TABLE [dbo].[EventsForAccounts] ADD  CONSTRAINT [DF_AccountsEvents_TicketCost]  DEFAULT ((0)) FOR [TicketCost]
GO
ALTER TABLE [dbo].[EventsForAccounts] ADD  CONSTRAINT [DF_AccountsEvents_IsPaid]  DEFAULT ((0)) FOR [IsPaid]
GO
ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF__Messages__Create__0A9D95DB]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[Messages] ADD  CONSTRAINT [DF_Messages_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Notifications] ADD  CONSTRAINT [DF__Notificat__Creat__0B91BA14]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[Notifications] ADD  CONSTRAINT [DF_Notifications_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[PhotosForAccounts] ADD  CONSTRAINT [DF_AccountsPhotos_IsAvatar]  DEFAULT ((0)) FOR [IsAvatar]
GO
ALTER TABLE [dbo].[PhotosForAccounts] ADD  CONSTRAINT [DF_AccountsPhotos_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[PhotosForEvents] ADD  DEFAULT (newid()) FOR [Guid]
GO
ALTER TABLE [dbo].[Regions] ADD  CONSTRAINT [DF_Regions_Sort]  DEFAULT ((100)) FOR [Order]
GO
ALTER TABLE [dbo].[RelationsForAccounts] ADD  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[RelationsForAccounts] ADD  CONSTRAINT [DF_Relations_IsConfirmed]  DEFAULT ((0)) FOR [IsConfirmed]
GO
ALTER TABLE [dbo].[SchedulesForAccounts] ADD  CONSTRAINT [DF_SchedulesForAccounts_PurchaseDate]  DEFAULT (getdate()) FOR [PurchaseDate]
GO
ALTER TABLE [dbo].[SchedulesForAccounts] ADD  CONSTRAINT [DF_SchedulesForAccounts_TicketCost]  DEFAULT ((0)) FOR [TicketCost]
GO
ALTER TABLE [dbo].[SchedulesForAccounts] ADD  CONSTRAINT [DF_SchedulesForAccounts_IsPaid]  DEFAULT ((0)) FOR [IsPaid]
GO
ALTER TABLE [dbo].[SchedulesForAccounts] ADD  CONSTRAINT [DF_SchedulesForAccounts_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[SchedulesForEvents] ADD  CONSTRAINT [DF_EventsSchedules_Guid]  DEFAULT (newid()) FOR [Guid]
GO
ALTER TABLE [dbo].[SchedulesForEvents] ADD  CONSTRAINT [DF_EventsSchedules_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_Guid]  DEFAULT (newid()) FOR [Guid]
GO
ALTER TABLE [dbo].[Users] ADD  CONSTRAINT [DF_Users_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[VisitsForAccounts] ADD  CONSTRAINT [DF__AccountsV__Creat__05D8E0BE]  DEFAULT (getdate()) FOR [CreateDate]
GO
ALTER TABLE [dbo].[VisitsForAccounts] ADD  CONSTRAINT [DF__AccountsV__LastD__06CD04F7]  DEFAULT (getdate()) FOR [LastDate]
GO
ALTER TABLE [dbo].[AccountsWishLists]  WITH CHECK ADD  CONSTRAINT [FK_AccountsWishLists_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AccountsWishLists] CHECK CONSTRAINT [FK_AccountsWishLists_Accounts]
GO
ALTER TABLE [dbo].[DiscussionsForEvents]  WITH CHECK ADD  CONSTRAINT [FK_DiscussionsForEvents_Accounts] FOREIGN KEY([SenderId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[DiscussionsForEvents] CHECK CONSTRAINT [FK_DiscussionsForEvents_Accounts]
GO
ALTER TABLE [dbo].[EventsForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_EventsForAccounts_Events] FOREIGN KEY([EventId])
REFERENCES [dbo].[Events] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[EventsForAccounts] CHECK CONSTRAINT [FK_EventsForAccounts_Events]
GO
ALTER TABLE [dbo].[FeaturesForSchedules]  WITH CHECK ADD  CONSTRAINT [FK_FeaturesForSchedules_SchedulesForEvents] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[SchedulesForEvents] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[FeaturesForSchedules] CHECK CONSTRAINT [FK_FeaturesForSchedules_SchedulesForEvents]
GO
ALTER TABLE [dbo].[HobbiesForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_HobbiesForAccounts_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[HobbiesForAccounts] CHECK CONSTRAINT [FK_HobbiesForAccounts_Accounts]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD  CONSTRAINT [FK_Notifications_Accounts] FOREIGN KEY([RecipientId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Notifications] CHECK CONSTRAINT [FK_Notifications_Accounts]
GO
ALTER TABLE [dbo].[PhotosForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_PhotosForAccounts_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PhotosForAccounts] CHECK CONSTRAINT [FK_PhotosForAccounts_Accounts]
GO
ALTER TABLE [dbo].[PhotosForEvents]  WITH CHECK ADD  CONSTRAINT [FK_PhotosForEvents_Events] FOREIGN KEY([EventId])
REFERENCES [dbo].[Events] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[PhotosForEvents] CHECK CONSTRAINT [FK_PhotosForEvents_Events]
GO
ALTER TABLE [dbo].[RelationsForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_RelationsForAccounts_Accounts] FOREIGN KEY([RecipientId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[RelationsForAccounts] CHECK CONSTRAINT [FK_RelationsForAccounts_Accounts]
GO
ALTER TABLE [dbo].[SchedulesForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_SchedulesForAccounts_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SchedulesForAccounts] CHECK CONSTRAINT [FK_SchedulesForAccounts_Accounts]
GO
ALTER TABLE [dbo].[SchedulesForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_SchedulesForAccounts_Schedules] FOREIGN KEY([ScheduleId])
REFERENCES [dbo].[SchedulesForEvents] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SchedulesForAccounts] CHECK CONSTRAINT [FK_SchedulesForAccounts_Schedules]
GO
ALTER TABLE [dbo].[SchedulesForEvents]  WITH CHECK ADD  CONSTRAINT [FK_SchedulesForEvents_Events] FOREIGN KEY([EventId])
REFERENCES [dbo].[Events] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[SchedulesForEvents] CHECK CONSTRAINT [FK_SchedulesForEvents_Events]
GO
ALTER TABLE [dbo].[Users]  WITH CHECK ADD  CONSTRAINT [FK_Users_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Users] CHECK CONSTRAINT [FK_Users_Accounts]
GO
ALTER TABLE [dbo].[VisitsForAccounts]  WITH CHECK ADD  CONSTRAINT [FK_VisitsForAccounts_Accounts] FOREIGN KEY([AccountId])
REFERENCES [dbo].[Accounts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[VisitsForAccounts] CHECK CONSTRAINT [FK_VisitsForAccounts_Accounts]
GO
/****** Object:  StoredProcedure [dbo].[EventsFilter_sp]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[EventsFilter_sp]
	@GetEventsRequestDto VARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	-- FeaturesIds
	CREATE TABLE #FeaturesIdsTable (Id INT);
	IF ((SELECT COUNT(*) FROM OPENJSON(@GetEventsRequestDto, '$.FeaturesIds')) > 0)
		INSERT INTO #FeaturesIdsTable
			SELECT se.Id FROM SchedulesForEvents se
			JOIN FeaturesForSchedules fs ON fs.ScheduleId = se.Id
			WHERE fs.FeatureId IN (SELECT [value] FROM OPENJSON(@GetEventsRequestDto, '$.FeaturesIds'))
	ELSE
		INSERT INTO #FeaturesIdsTable SELECT Id FROM SchedulesForEvents

	-- RegionsId
	CREATE TABLE #RegionsIdsTable (Id INT)
	IF ((SELECT COUNT(*) FROM OPENJSON(@GetEventsRequestDto, '$.RegionsIds')) > 0)
		INSERT INTO #RegionsIdsTable 
			SELECT se.Id FROM SchedulesForEvents se
			JOIN Events e ON e.Id = se.EventId
			WHERE e.RegionId IN (SELECT [value] FROM OPENJSON(@GetEventsRequestDto, '$.RegionsIds'))
	ELSE
		INSERT INTO #RegionsIdsTable SELECT Id FROM SchedulesForEvents

	-- AdminId
	CREATE TABLE #AdminsIdsTable (Id INT)
	IF ((SELECT COUNT(*) FROM OPENJSON(@GetEventsRequestDto, '$.AdminsIds')) > 0)
		INSERT INTO #AdminsIdsTable 
			SELECT se.Id FROM SchedulesForEvents se
			JOIN Events e ON e.Id = se.EventId
			WHERE e.AdminId IN (SELECT [value] FROM OPENJSON(@GetEventsRequestDto, '$.AdminsIds'))
	ELSE
		INSERT INTO #AdminsIdsTable SELECT Id FROM SchedulesForEvents

	-- FreeText
	DECLARE @filterFreeText NVARCHAR(100) = (SELECT JSON_VALUE(@GetEventsRequestDto, '$.FilterFreeText'))
	CREATE TABLE #FilterFreeTextTable (Id INT)
	IF (@filterFreeText IS NOT NULL AND @filterFreeText <> '')
		INSERT INTO #FilterFreeTextTable 
			SELECT se.Id FROM SchedulesForEvents se
			JOIN Events e ON e.Id = se.EventId
			WHERE FREETEXT (e.Name, @filterFreeText) OR FREETEXT (se.Description, @filterFreeText)
	ELSE
		INSERT INTO #FilterFreeTextTable SELECT Id FROM SchedulesForEvents

	SELECT DISTINCT(Id) FROM #FeaturesIdsTable
	INTERSECT
	SELECT DISTINCT(Id) FROM #RegionsIdsTable
	INTERSECT
	SELECT DISTINCT(Id) FROM #AdminsIdsTable
	INTERSECT
	SELECT DISTINCT(Id) FROM #FilterFreeTextTable
END
GO
/****** Object:  StoredProcedure [dbo].[UpdateHobbiesForAccounts_sp]    Script Date: 02.10.2024 18:48:55 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[UpdateHobbiesForAccounts_sp]
	@AccountId INT,
	@HobbiesIds VARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	IF (@HobbiesIds = '')
		BEGIN
			DELETE FROM HobbiesForAccounts WHERE AccountId = @AccountId;
			RETURN
		END

	CREATE TABLE #hobbies (HobbyId INT);

	INSERT INTO #hobbies
	SELECT value FROM STRING_SPLIT(@HobbiesIds, ',');

	DELETE FROM HobbiesForAccounts WHERE AccountId = @AccountId AND HobbyId NOT IN (SELECT HobbyId FROM #hobbies)

	INSERT INTO HobbiesForAccounts (AccountId, HobbyId)
	SELECT @AccountId, HobbyId FROM #hobbies
	EXCEPT
	SELECT @AccountId, HobbyId FROM HobbiesForAccounts WHERE AccountId = @AccountId

	DROP TABLE #hobbies;

END
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'null - пара, 0 - муж, 1 - жен' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'SchedulesForAccounts', @level2type=N'COLUMN',@level2name=N'AccountGender'
GO
USE [master]
GO
ALTER DATABASE [SwingHouse] SET  READ_WRITE 
GO
