CREATE TABLE dbo.Environment (
	[EnvironmentID] uniqueidentifier NOT NULL,
	[Name] nvarchar(100) NOT NULL,
	[Platform] nvarchar(50) NOT NULL,
	[Tooling] nvarchar(50) NOT NULL,
	[IsActive] bit NOT NULL,
	CONSTRAINT PK_Environment PRIMARY KEY CLUSTERED ([EnvironmentID]),
	CONSTRAINT UQ_Environment_Name UNIQUE ([Name])
);
GO

CREATE TABLE dbo.Agent (
	[AgentID] uniqueidentifier NOT NULL,
	[Name] nvarchar(100) NOT NULL,
	[ApiKey] varbinary(32) NOT NULL,
	[IsActive] bit NOT NULL,
	[EnvironmentID] uniqueidentifier NOT NULL,
	CONSTRAINT PK_Agent PRIMARY KEY CLUSTERED ([AgentID]),
	CONSTRAINT UQ_Agent_Name UNIQUE ([Name]),
	CONSTRAINT FK_Agent_Environment FOREIGN KEY ([EnvironmentID]) REFERENCES dbo.Environment ([EnvironmentID])
);
GO

CREATE TABLE dbo.Project (
	[ProjectID] uniqueidentifier NOT NULL,
	[Name] nvarchar(100) NOT NULL,
	[EnvironmentID] uniqueidentifier NOT NULL,
	[RepositoryType] varchar(10) NOT NULL,
	[RepositoryPath] nvarchar(max) NOT NULL,
	[BlameTheAuthor] bit NOT NULL,
	[PraiseTheAuthor] bit NOT NULL,
	[NotifyTheAuthor] bit NOT NULL,
	[MaxFailureCount] int NOT NULL,
	[IsActive] bit NOT NULL,
	[TriggerMethod] tinyint NOT NULL,
	-- project state
	[NextBuildNumber] int NOT NULL,

	CONSTRAINT PK_Project PRIMARY KEY CLUSTERED ([ProjectID]),
	CONSTRAINT UQ_Project_Name UNIQUE ([Name]),
	CONSTRAINT FK_Project_Environment FOREIGN KEY ([EnvironmentID]) REFERENCES dbo.Environment ([EnvironmentID])
);
GO

CREATE INDEX IX_Project_EnvironmentID ON dbo.Project ([EnvironmentID]);
GO

CREATE TABLE dbo.ProjectContact (
	[ProjectContactID] uniqueidentifier NOT NULL,
	[ProjectID] uniqueidentifier NOT NULL,
	[Name] nvarchar(100) NOT NULL,
	[RepositoryUsername] nvarchar(100) NULL,
	[Email] nvarchar(200) NULL,
	[Phone] nvarchar(50) NULL,
	[NotifyOnSuccess] bit NOT NULL,
	[NotifyOnFailure] bit NOT NULL,
	CONSTRAINT PK_ProjectContact PRIMARY KEY CLUSTERED ([ProjectContactID]),
	CONSTRAINT UQ_ProjectContact_Name UNIQUE ([ProjectID],[Name]),
	CONSTRAINT FK_ProjectContact_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project ([ProjectID])
);
GO

CREATE INDEX IX_ProjectContact_ProjectID ON dbo.Project ([ProjectID]);
GO

CREATE TABLE dbo.ProjectSetting (
	[ProjectID] uniqueidentifier NOT NULL,
	[TypeName] varchar(25) NOT NULL, -- Repository, Variable
	[Name] nvarchar(100) NOT NULL,
	[Value] nvarchar(max) NULL,
	CONSTRAINT PK_ProjectSetting PRIMARY KEY CLUSTERED ([ProjectID],[TypeName],[Name]),
	CONSTRAINT FK_ProjectSetting_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project ([ProjectID])
);
GO

CREATE INDEX IX_ProjectSetting_ProjectID ON dbo.ProjectSetting ([ProjectID]);
GO

CREATE TABLE dbo.ProjectAsset (
	[ProjectAssetID] int IDENTITY(1,1) NOT NULL,
	[ProjectID] uniqueidentifier,
	[Path] nvarchar(max) NOT NULL,
	CONSTRAINT PK_ProjectAsset PRIMARY KEY CLUSTERED ([ProjectAssetID]),
	CONSTRAINT FK_ProjectAsset_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project ([ProjectID])
);
GO

CREATE INDEX IX_ProjectAsset_ProjectID ON dbo.ProjectAsset ([ProjectID]);
GO

CREATE TABLE dbo.ProjectTask (
	[ProjectID] uniqueidentifier NOT NULL,
	[TaskNumber] int NOT NULL,
	[CategoryName] varchar(15) NOT NULL, -- Build, Test, Deploy
	[Expression] nvarchar(max) NOT NULL,
	[SuccessExitCodes] varchar(100) NOT NULL, -- CSV list of exit codes
	CONSTRAINT PK_ProjectTask PRIMARY KEY CLUSTERED ([ProjectID],[TaskNumber]),
	CONSTRAINT FK_ProjectTask FOREIGN KEY ([ProjectID]) REFERENCES dbo.[Project] ([ProjectID])
);
GO

CREATE INDEX IX_ProjectTask_ProjectID ON dbo.ProjectTask ([ProjectID]);
GO

CREATE TABLE dbo.ProjectHistory (
	[ProjectID] uniqueidentifier NOT NULL,
	[BuildNumber] int NOT NULL,
	[BuildDate] datetimeoffset NOT NULL,
	[ResultCode] int NOT NULL,
	CONSTRAINT PK_ProjectHistory PRIMARY KEY CLUSTERED ([ProjectID],[BuildNumber]),
	CONSTRAINT FK_ProjectHistory_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project ([ProjectID])
);
GO

CREATE INDEX IX_ProjectHistory_ProjectID ON dbo.ProjectHistory ([ProjectID]);
GO

CREATE TABLE dbo.BuildQueue (
	[BuildQueueID] uniqueidentifier NOT NULL,
	[ProjectID] uniqueidentifier NOT NULL,
	[RevisionIdentifier] nvarchar(256) NOT NULL,
	[BuildNumber] int NOT NULL,
	[CreateDateTime] datetimeoffset NOT NULL,
	[AssignedDateTime] datetimeoffset NULL,
	[AssignedAgentID] uniqueidentifier  NULL,
	[CompleteDateTime] datetimeoffset NULL,
	[Status] tinyint NOT NULL, --1 = queued, 2 = running, 3 = done
	[LockExpiresDateTime] datetimeoffset NULL,
	RetryAfterDateTime datetimeoffset NULL,

	[LastResultCode] int NULL,
	[LastResultDateTime] datetimeoffset NULL,
	[FailCount] int NOT NULL CONSTRAINT DF_BuildQueue_FailCount DEFAULT(0),

	CONSTRAINT PK_BuildQueue PRIMARY KEY ([BuildQueueID]),
	CONSTRAINT FK_BuildQueue_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project ([ProjectID]),
	CONSTRAINT FK_BuildQueue_Agent FOREIGN KEY ([AssignedAgentID]) REFERENCES dbo.Agent ([AgentID])
);
GO

CREATE INDEX IX_BuildQueue_ProjectID ON dbo.BuildQueue ([ProjectID]);
CREATE INDEX IX_BuildQueue_AssignedAgentID ON dbo.BuildQueue ([AssignedAgentID]);
GO

CREATE TABLE dbo.BuildLog (
	[BuildQueueID] uniqueidentifier NOT NULL,
	[MessageNumber] int NOT NULL,
	[Severity] int NOT NULL,
	[Message] nvarchar(max) NULL,
	[DateTime] datetimeoffset NOT NULL,
	CONSTRAINT PK_BuildLog PRIMARY KEY CLUSTERED ([BuildQueueID],[MessageNumber]),
	CONSTRAINT FK_BuildLog_BuildQueue FOREIGN KEY ([BuildQueueID]) REFERENCES dbo.BuildQueue(BuildQueueID),
);
GO

DECLARE @Foo datetimeoffset = SYSDATETIMEOFFSET();
DECLARE @bar datetimeoffset = SYSDATETIMEOFFSET();

SELECT @foo, @Bar;
