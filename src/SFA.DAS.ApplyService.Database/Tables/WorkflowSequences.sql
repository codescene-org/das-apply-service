CREATE TABLE [dbo].[WorkflowSequences](
	[Id] [uniqueidentifier] NOT NULL,
	[WorkflowId] [uniqueidentifier] NOT NULL,
	[SequenceId] [int] NOT NULL,
	[Status] [nvarchar](50) NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WorkflowSequences] ADD  CONSTRAINT [DF_WorkflowSequences_Id]  DEFAULT (newid()) FOR [Id]
GO
