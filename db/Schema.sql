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
	[EnvironmentID] int NOT NULL,
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
	[LastBuildResultCode] int NULL,
	[LastSuccessDateTime] datetimeoffset NULL,
	[LastSuccessCommitIdentifier] nvarchar(256) NULL,
	[LastFailureDateTime] datetimeoffset NULL,
	[LastFailureCommitIdentifier] nvarchar(256) NOT NULL,
	[FailCount] int NOT NULL,

	CONSTRAINT PK_Project PRIMARY KEY CLUSTERED ([ProjectID]),
	CONSTRAINT UQ_Project_Name UNIQUE ([Name]),
	CONSTRAINT FK_Project_Environment FOREIGN KEY ([EnvironmentID]) REFERENCES dbo.Environment ([EnvironmentID])
);
GO

CREATE INDEX IX_Project_Environment ON dbo.Project ([Environment]);
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
	[ProjetAssetID] int IDENTITY(1,1) NOT NULL,
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
	[TaskType] varchar(15) NOT NULL, -- Build, Test, Deploy
	[CommandText] nvarchar(max) NOT NULL,
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
	CONSTRAINT PK_BuildQueue PRIMARY KEY ([BuildQueueID]),
	CONSTRAINT FK_BuildQueue_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project ([ProjectID]),
	CONSTRAINT FK_BuildQueue_Agent FOREIGN KEY ([AssignedAgentID]) REFERENCES dbo.Agent ([AgentID])
);
GO

CREATE INDEX IX_BuildQueue_ProjectID ON dbo.BuildQueue ([ProjectID]);
CREATE INDEX IX_BuildQueue_AssignedAgentID ON dbo.BuildQueue ([AssignedAgentID]);
GO

CREATE TABLE dbo.BuildLog (
	[ProjectID] uniqueidentifier NOT NULL,
	[BuildQueueID] int NOT NULL,
	[MessageNumber] int NOT NULL,
	[Severity] int NOT NULL,
	[Message] nvarchar(max) NULL,
	CONSTRAINT PK_BuildLog PRIMARY KEY CLUSTERED ([ProjectID],[BuildNumber],[MessageNumber]),
	CONSTRAINT FK_BuildLog_Project FOREIGN KEY ([ProjectID]) REFERENCES dbo.Project([ProjectID]),
);
GO

