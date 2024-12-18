/****** Object:  Database [MonitFlota]    Script Date: 11/5/2024 1:45:14 AM ******/
CREATE DATABASE [MonitFlota]  (EDITION = 'GeneralPurpose', SERVICE_OBJECTIVE = 'GP_S_Gen5_1', MAXSIZE = 32 GB) WITH CATALOG_COLLATION = SQL_Latin1_General_CP1_CI_AS, LEDGER = OFF;
GO
ALTER DATABASE [MonitFlota] SET COMPATIBILITY_LEVEL = 160
GO
ALTER DATABASE [MonitFlota] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [MonitFlota] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [MonitFlota] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [MonitFlota] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [MonitFlota] SET ARITHABORT OFF 
GO
ALTER DATABASE [MonitFlota] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [MonitFlota] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [MonitFlota] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [MonitFlota] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [MonitFlota] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [MonitFlota] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [MonitFlota] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [MonitFlota] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [MonitFlota] SET ALLOW_SNAPSHOT_ISOLATION ON 
GO
ALTER DATABASE [MonitFlota] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [MonitFlota] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [MonitFlota] SET  MULTI_USER 
GO
ALTER DATABASE [MonitFlota] SET QUERY_STORE = ON
GO
ALTER DATABASE [MonitFlota] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 100, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
/*** The scripts of database scoped configurations in Azure should be executed inside the target database connection. ***/
GO
-- ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 8;
GO
/****** Object:  Table [dbo].[currentloc]    Script Date: 11/5/2024 1:45:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[currentloc](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[vehicle_id] [int] NOT NULL,
	[coordinates] [nvarchar](50) NOT NULL,
	[ctimestamp] [datetime] NULL,
	[speed] [real] NULL,
	[potholes] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Jobs]    Script Date: 11/5/2024 1:45:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Jobs](
	[j_Id] [int] IDENTITY(1,1) NOT NULL,
	[j_Service] [int] NOT NULL,
	[j_Delivery] [nvarchar](100) NULL,
	[j_Location] [nvarchar](100) NULL,
	[j_Skills] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[j_Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 11/5/2024 1:45:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[u_id] [int] IDENTITY(1,1) NOT NULL,
	[u_username] [varchar](255) NULL,
	[u_password] [varchar](255) NULL,
	[u_type] [varchar](50) NULL,
	[u_type_id] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[u_id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserTrafficReports]    Script Date: 11/5/2024 1:45:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserTrafficReports](
	[ReportID] [int] IDENTITY(1,1) NOT NULL,
	[Latitude] [decimal](10, 6) NOT NULL,
	[Longitude] [decimal](10, 6) NOT NULL,
	[Lanes] [tinyint] NOT NULL,
	[Potholes] [tinyint] NOT NULL,
	[RoadCondition] [varchar](50) NOT NULL,
	[SurfaceType] [varchar](50) NOT NULL,
	[TrafficLevel] [varchar](20) NOT NULL,
	[Hazards] [text] NULL,
	[ReportDate] [datetime] NOT NULL,
	[ReporterID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ReportID] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vehicles]    Script Date: 11/5/2024 1:45:15 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vehicles](
	[vh_Id] [int] IDENTITY(1,1) NOT NULL,
	[vh_Profile] [nvarchar](50) NOT NULL,
	[vh_Start] [nvarchar](100) NULL,
	[vh_End] [nvarchar](100) NULL,
	[vh_Capacity] [nvarchar](100) NULL,
	[vh_Skills] [nvarchar](100) NULL,
	[vh_TimeWindow] [nvarchar](100) NULL,
PRIMARY KEY CLUSTERED 
(
	[vh_Id] ASC
)WITH (STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[currentloc] ON 

INSERT [dbo].[currentloc] ([id], [vehicle_id], [coordinates], [ctimestamp], [speed], [potholes]) VALUES (1, 1, N'26.034555, 44.913811', CAST(N'2024-11-04T23:26:18.780' AS DateTime), 2.5, 0)
SET IDENTITY_INSERT [dbo].[currentloc] OFF
GO
SET IDENTITY_INSERT [dbo].[Jobs] ON 

INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (1, 300, N'1', N'26.0268044471741, 44.9533228467913', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (2, 300, N'1', N'26.0262143611908, 44.94181888887002', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (3, 300, N'1', N'26.0262143611908, 44.94181888887002', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (4, 1, N'1', N'25.9898328781128, 44.9509804786043', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (6, 1, N'1', N'26.0393786430359,44.9350483205907', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (7, 1, N'1', N'26.0393786430359,44.9350483205907', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (8, 1, N'1', N'26.0319328308105,44.9341217197001', N'1')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (9, 1, N'1', N'25.9884488582611,44.9407670918876', N'6')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (10, 1, N'1', N'25.9946823120117,44.9669233968106', N'16')
INSERT [dbo].[Jobs] ([j_Id], [j_Service], [j_Delivery], [j_Location], [j_Skills]) VALUES (11, 1, N'1', N'26.0358810424805,44.9437819385537', N'1')
SET IDENTITY_INSERT [dbo].[Jobs] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([u_id], [u_username], [u_password], [u_type], [u_type_id]) VALUES (1, N'admin', N'admin', N'administrator', N'1')
INSERT [dbo].[Users] ([u_id], [u_username], [u_password], [u_type], [u_type_id]) VALUES (2, N'vehicle1', N'vehicle1', N'vehicle', N'1')
INSERT [dbo].[Users] ([u_id], [u_username], [u_password], [u_type], [u_type_id]) VALUES (3, N'vehicle2', N'vehicle2', N'vehicle', N'2')
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET IDENTITY_INSERT [dbo].[UserTrafficReports] ON 

INSERT [dbo].[UserTrafficReports] ([ReportID], [Latitude], [Longitude], [Lanes], [Potholes], [RoadCondition], [SurfaceType], [TrafficLevel], [Hazards], [ReportDate], [ReporterID]) VALUES (1, CAST(44.922658 AS Decimal(10, 6)), CAST(26.015903 AS Decimal(10, 6)), 3, 2, N'Poor', N'Asphalt', N'Heavy', N'Construction work', CAST(N'2024-10-25T08:30:00.000' AS DateTime), 101)
INSERT [dbo].[UserTrafficReports] ([ReportID], [Latitude], [Longitude], [Lanes], [Potholes], [RoadCondition], [SurfaceType], [TrafficLevel], [Hazards], [ReportDate], [ReporterID]) VALUES (2, CAST(44.925441 AS Decimal(10, 6)), CAST(25.994475 AS Decimal(10, 6)), 2, 1, N'Moderate', N'Concrete', N'Moderate', N'Minor cracks', CAST(N'2024-10-25T09:00:00.000' AS DateTime), 102)
INSERT [dbo].[UserTrafficReports] ([ReportID], [Latitude], [Longitude], [Lanes], [Potholes], [RoadCondition], [SurfaceType], [TrafficLevel], [Hazards], [ReportDate], [ReporterID]) VALUES (3, CAST(44.935298 AS Decimal(10, 6)), CAST(26.019022 AS Decimal(10, 6)), 4, 0, N'Good', N'Asphalt', N'Light', NULL, CAST(N'2024-10-25T09:30:00.000' AS DateTime), 103)
INSERT [dbo].[UserTrafficReports] ([ReportID], [Latitude], [Longitude], [Lanes], [Potholes], [RoadCondition], [SurfaceType], [TrafficLevel], [Hazards], [ReportDate], [ReporterID]) VALUES (4, CAST(44.935298 AS Decimal(10, 6)), CAST(26.019022 AS Decimal(10, 6)), 3, 3, N'Poor', N'Gravel', N'Heavy', N'Roadwork and potholes', CAST(N'2024-10-25T10:00:00.000' AS DateTime), 104)
SET IDENTITY_INSERT [dbo].[UserTrafficReports] OFF
GO
SET IDENTITY_INSERT [dbo].[Vehicles] ON 

INSERT [dbo].[Vehicles] ([vh_Id], [vh_Profile], [vh_Start], [vh_End], [vh_Capacity], [vh_Skills], [vh_TimeWindow]) VALUES (1, N'driving-car', N'26.036540865898136,44.91442221794393', N'26.036540865898136,44.91442221794393', N'6', N'1,14,2', N'28800,43200')
INSERT [dbo].[Vehicles] ([vh_Id], [vh_Profile], [vh_Start], [vh_End], [vh_Capacity], [vh_Skills], [vh_TimeWindow]) VALUES (2, N'driving-car', N'26.036540865898136,44.91442221794393', N'26.036540865898136,44.91442221794393', N'2', N'6,16', N'28800,43200')
SET IDENTITY_INSERT [dbo].[Vehicles] OFF
GO
ALTER TABLE [dbo].[currentloc] ADD  DEFAULT (getdate()) FOR [ctimestamp]
GO
ALTER DATABASE [MonitFlota] SET  READ_WRITE 
GO
