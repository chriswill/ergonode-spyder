CREATE TABLE [dbo].[Nodes](	
	[Address] [varchar](50) NOT NULL,
	[Port] [int] NOT NULL,
    [PublicIP] [bit] NOT NULL,	
	[AgentName] [varchar](100) NOT NULL,
	[PeerName] [varchar](100) NOT NULL,
	[Version] [varchar](20) NOT NULL,
	[BlocksToKeep] [int] NULL,
	[NiPoPoWBootstrapped] [bit] NULL,
	[StateType] [varchar](10) NULL,
	[VerifyingTransactions] [bit] NULL,
	[PeerCount] [smallint] NULL,
	[DateAdded] [smalldatetime] NOT NULL,
	[DateUpdated] [smalldatetime] NULL,
	[DateHandshake] [smalldatetime] NULL,
	[DatePeersQueried] [smalldatetime] NULL,
	[DateContactAttempted] [smalldatetime] NULL,
	[MissingDate] [smalldatetime] NULL,
	[ContactAttempts] [tinyint] NULL,
	[ContinentCode] [varchar](2) NULL,
	[ContinentName] [varchar](50) NULL,
	[CountryCode] [varchar](2) NULL,
	[CountryName] [varchar](50) NULL,
	[RegionCode] [varchar](5) NULL,
	[RegionName] [varchar](50) NULL,
	[City] [varchar](100) NULL,
	[ZipOrPostalCode] [varchar](25) NULL,
	[Latitude] [numeric](18, 14) NULL,
	[Longitude] [numeric](18, 14) NULL,
	[ISP] [varchar](150) NULL,
	[GeoDateUpdated] [smalldatetime] NULL,
 CONSTRAINT [PK_Nodes] PRIMARY KEY CLUSTERED 
(
	[Address] ASC,
	[Port] ASC
))
GO

CREATE TYPE [dbo].[NodeTableType] AS TABLE(
	[Address] [varchar](50) NOT NULL,
	[Port] [int] NOT NULL,
	[PublicIP] [bit] NOT NULL,
	[AgentName] [varchar](100) NOT NULL,
	[PeerName] [varchar](100) NOT NULL,
	[Version] [varchar](20) NOT NULL,
	[BlocksToKeep] [int] NULL,
	[NiPoPoWBootstrapped] [bit] NULL,
	[StateType] [varchar](10) NULL,
	[VerifyingTransactions] [bit] NULL,
	PRIMARY KEY CLUSTERED 
(
	[Address] ASC,
	[Port] ASC
))

GO


CREATE TABLE [dbo].[NodesByDay](
	[Day] [date] NOT NULL,
	[NodeCount] [INT] NOT NULL,
 CONSTRAINT [PK_NodesByDay] PRIMARY KEY CLUSTERED 
(
	[Day] ASC	
)) 
GO

CREATE TABLE [dbo].[NodesByWeek](
	[Year] [INT] NOT NULL,
	[Week] [TINYINT] NOT NULL,	
	[NodeCount] [INT] NOT NULL,
 CONSTRAINT [PK_NodesByWeek] PRIMARY KEY CLUSTERED 
(
	[Year] ASC,
	[Week] ASC	
)) 
GO

CREATE TABLE [dbo].[NodesByMonth](
	[Year] [INT] NOT NULL,
	[Month] [TINYINT] NOT NULL,	
	[NodeCount] [INT] NOT NULL,
 CONSTRAINT [PK_NodesByMonth] PRIMARY KEY CLUSTERED 
(
	[Year] ASC,
	[Month] ASC	
)) 
GO

CREATE VIEW [dbo].[ActiveNodes]
AS
SELECT        [Address], [Port], PublicIP, AgentName, PeerName, [Version], BlocksToKeep, NiPoPoWBootstrapped, 
			  StateType, VerifyingTransactions, PeerCount, DateAdded, DateUpdated, ContinentCode, ContinentName, CountryCode, CountryName, 
			  RegionCode, RegionName, City, ZipOrPostalCode, Latitude, Longitude, ISP
FROM          dbo.Nodes
WHERE        (DateHandshake > DATEADD(Day, - 1, GetUTCDate()))

GO

CREATE PROCEDURE [dbo].[AddUpdateDiscoveredNodes] 
	@tvp dbo.NodeTableType READONLY,
	@address [nvarchar](50),
	@port [int]

AS
BEGIN	
	SET NOCOUNT ON;

	UPDATE dbo.Nodes
	SET DatePeersQueried = GETUTCDATE(),
	    PeerCount = (SELECT COUNT(*) FROM @tvp)
	WHERE [Address] = @address AND [Port] = @port

	MERGE dbo.Nodes as [Target]
	USING (
		SELECT 
		[Address], 
		[Port],
		[PublicIP],
		[AgentName],
		[PeerName],
		[Version],
		[BlocksToKeep],
		[NiPoPoWBootstrapped],
		[StateType],
		[VerifyingTransactions]	
		FROM @tvp) AS [Source]
	( 
		[Address], 
		[Port],
		[PublicIP],
		[AgentName],
		[PeerName],
		[Version],
		[BlocksToKeep],
		[NiPoPoWBootstrapped],
		[StateType],
		[VerifyingTransactions]	
	) 
	ON ([Target].[Address] = [Source].[Address] AND [Target].[Port] = [Source].[Port])
	WHEN MATCHED THEN
	UPDATE 
		SET [AgentName] = [Source].[AgentName],	
			[PeerName] = [Source].[PeerName],
			[Version] = [Source].[Version],
			[BlocksToKeep] = [Source].[BlocksToKeep],
			[NiPoPoWBootstrapped] = [Source].[NiPoPoWBootstrapped],
			[StateType] = [Source].[StateType],
			[VerifyingTransactions] = [Source].[VerifyingTransactions],
			[DateUpdated] = GETUTCDATE(),
			[DateContactAttempted] = null,
			[MissingDate] = null,
			[ContactAttempts] = null
	WHEN NOT MATCHED THEN
	INSERT (
		[Address], 
		[Port],
		[PublicIP],
		[AgentName],
		[PeerName],
		[Version],
		[BlocksToKeep],
		[NiPoPoWBootstrapped],
		[StateType],
		[VerifyingTransactions],
		[DateAdded],
		[DateUpdated]      
	)
	VALUES (
		[Source].[Address], 
		[Source].[Port],
		[Source].[PublicIP],
		[Source].[AgentName],
		[Source].[PeerName],
		[Source].[Version],
		[Source].[BlocksToKeep],
		[Source].[NiPoPoWBootstrapped],
		[Source].[StateType],
		[Source].[VerifyingTransactions],
		GetUTCDate(),
		GetUTCDate()
	);
END
GO

CREATE PROCEDURE [dbo].[RecordNodeHandshake] 
	@tvp dbo.NodeTableType READONLY	

AS
BEGIN	
	SET NOCOUNT ON;
		
	MERGE dbo.Nodes AS [Target]
	USING (
		SELECT 
		[Address], 
		[Port],
		[PublicIP],
		[AgentName],
		[PeerName],
		[Version],
		[BlocksToKeep],
		[NiPoPoWBootstrapped],
		[StateType],
		[VerifyingTransactions]	
		FROM @tvp) AS [Source]
	( 
		[Address], 
		[Port],
		[PublicIP],
		[AgentName],
		[PeerName],
		[Version],
		[BlocksToKeep],
		[NiPoPoWBootstrapped],
		[StateType],
		[VerifyingTransactions]	
	) 
	ON ([Target].[Address] = [Source].[Address] AND [Target].[Port] = [Source].[Port])
	WHEN MATCHED THEN
	UPDATE 
		SET [AgentName] = [Source].[AgentName],	
			[PeerName] = [Source].[PeerName],
			[Version] = [Source].[Version],
			[BlocksToKeep] = [Source].[BlocksToKeep],
			[NiPoPoWBootstrapped] = [Source].[NiPoPoWBootstrapped],
			[StateType] = [Source].[StateType],
			[VerifyingTransactions] = [Source].[VerifyingTransactions],
			[DateUpdated] = GETUTCDATE(),
			[DateHandshake] = GETUTCDATE(),
			[DateContactAttempted] = NULL,
			[MissingDate] = NULL,
			[ContactAttempts] = NULL
	WHEN NOT MATCHED THEN
	INSERT (
		[Address], 
		[Port],
		[PublicIP],
		[AgentName],
		[PeerName],
		[Version],
		[BlocksToKeep],
		[NiPoPoWBootstrapped],
		[StateType],
		[VerifyingTransactions],
		[DateAdded],
		[DateUpdated],
		[DateHandshake]
	)
	VALUES (
		[Source].[Address], 
		[Source].[Port],
		[Source].[PublicIP],
		[Source].[AgentName],
		[Source].[PeerName],
		[Source].[Version],
		[Source].[BlocksToKeep],
		[Source].[NiPoPoWBootstrapped],
		[Source].[StateType],
		[Source].[VerifyingTransactions],
		GETUTCDATE(),
		GETUTCDATE(),
		GETUTCDATE()
	);
END
GO


CREATE PROCEDURE RecordFailedConnection 	
	@address [varchar](50), 
	@port [int]
AS
BEGIN
	
	SET NOCOUNT ON;

	DECLARE @attempts [tinyint] = (SELECT ISNULL(ContactAttempts, 0) FROM dbo.Nodes
		WHERE [Address] = @address AND [Port] = @port)

	UPDATE dbo.Nodes
	SET DateContactAttempted = GETUTCDATE(), ContactAttempts = @attempts + 1
	WHERE
		[Address] = @address AND [Port] = @port
    
END
GO

CREATE PROCEDURE DailyMaintenanceAndAnalytics 	
AS
BEGIN	

	SET NOCOUNT ON;

    DELETE 
	FROM dbo.Nodes
	WHERE (DateUpdated is NULL OR DateUpdated < DATEADD(DAY, -4, GETUTCDATE())) AND ContactAttempts >= 5

	MERGE dbo.NodesByDay as [Target]
	USING (
		SELECT 
		CAST(GETUTCDATE() as date),
		(Select COUNT(*) FROM dbo.ActiveNodes)
	) AS [Source]
	( 
		[Day], 
		[NodeCount]		
	) 
	ON ([Target].[Day] = [Source].[Day])
	WHEN MATCHED THEN
	UPDATE 
		SET [NodeCount] = [Source].[NodeCount]				
	WHEN NOT MATCHED THEN
	INSERT (
		[Day], 
		[NodeCount]		
	)
	VALUES (
		[Source].[Day], 
		[Source].[NodeCount]		
	);

	MERGE dbo.NodesByWeek as [Target]
	USING (
		SELECT 
		DATEPART(YEAR, [Day]),
		DATEPART(WEEK, [Day]),	
		CAST(ROUND(AVG(NodeCount), 0) AS INT)
		FROM dbo.NodesByDay
		WHERE [Day] >= DATEADD(wk, 0, DATEADD(DAY, 1-DATEPART(WEEKDAY, GETUTCDATE()), DATEDIFF(dd, 0, GETUTCDATE())))
		GROUP BY DATEPART(YEAR, [Day]), DATEPART(WEEK, [Day])
	) AS [Source]
	( 
		[Year],
		[Week],	
		[NodeCount]		
	) 
	ON ([Target].[Year] = [Source].[Year] AND [Target].[Week] = [Source].[Week])
	WHEN MATCHED THEN
	UPDATE 
		SET [NodeCount] = [Source].[NodeCount]				
	WHEN NOT MATCHED THEN
	INSERT (
		[Year],
		[Week],	
		[NodeCount]		
	)
	VALUES (
		[Source].[Year], 
		[Source].[Week], 	
		[Source].[NodeCount]		
	);	

	MERGE dbo.NodesByMonth as [Target]
	USING (
		SELECT 
		DATEPART(YEAR, [Day]),
		DATEPART(MONTH, [Day]),	
		CAST(ROUND(AVG(NodeCount), 0) AS INT)
		FROM dbo.NodesByDay
		WHERE [Day] >= DATEFROMPARTS(YEAR(GETUTCDATE()),MONTH(GETUTCDATE()),1)
		GROUP BY DATEPART(YEAR, [Day]), DATEPART(MONTH, [Day])
	) AS [Source]
	( 
		[Year],
		[Month],	
		[NodeCount]		
	) 
	ON ([Target].[Year] = [Source].[Year] AND [Target].[Month] = [Source].[Month])
	WHEN MATCHED THEN
	UPDATE 
		SET [NodeCount] = [Source].[NodeCount]				
	WHEN NOT MATCHED THEN
	INSERT (
		[Year],
		[Month],	
		[NodeCount]		
	)
	VALUES (
		[Source].[Year], 
		[Source].[Month], 	
		[Source].[NodeCount]		
	);
	
END
GO