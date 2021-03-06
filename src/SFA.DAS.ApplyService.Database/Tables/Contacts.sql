-- create Contact table for Apply
CREATE TABLE [dbo].[Contacts](
	[Id] [uniqueidentifier] NOT NULL DEFAULT NEWID(),
   Email	[nvarchar](256)	NOT NULL,	-- Email Address used to uniquely identify Applicant
   [GivenNames] [nvarchar](250)  NOT NULL, 
   [FamilyName] [nvarchar](250)  NOT NULL, 
   [SigninId] [uniqueidentifier] NULL,-- when Known SigninID (DFE or Provider Idams Username)
   [SigninType] [nvarchar](20)  NULL, -- when Known 'DfESignin','ProvideridAMS'
   ApplyOrganisationID [uniqueidentifier] NULL, -- When Known organisation for the apply contact
   ContactDetails [nvarchar](max)	 NULL, --  Contact Details 
   Status	[nvarchar](	20)	NOT NULL,	-- 'new', 'inprogress' � signup awaiting callback from DFE Signin, 'live' is live , 'deleted' is no longer to be used
   IsApproved [bit] NOT NULL DEFAULT 0, -- set when this user is approved to represent the Organisation
   CreatedAt	Datetime2(7)	NOT NULL,	--Date / Time that the record was created
   CreatedBy	[nvarchar](256)	NOT NULL,	--Username (staff or ApplyContact)
   UpdatedAt	Datetime2(7)		NULL,	--Date / Time of the last update
   UpdatedBy	[nvarchar](256)	NULL,	--Username (staff or ApplyContact)
   DeletedAt	Datetime2(7)		NULL,	--Date / Time of the soft delete
   DeletedBy	[nvarchar](256)	NULL	--Username (staff or ApplyContact)
    CONSTRAINT [PK_Contact] PRIMARY KEY ([Id]),
) 
GO

ALTER TABLE [dbo].[Contacts] ADD CONSTRAINT [FK_Contacts_Organisations] FOREIGN KEY([ApplyOrganisationID])
REFERENCES [dbo].[Organisations] ([Id])
GO

ALTER TABLE [dbo].[Contacts] CHECK CONSTRAINT [FK_Contacts_Organisations]
GO

CREATE UNIQUE INDEX [IXU_Contacts] ON [Contacts] ([Email])
GO
CREATE INDEX [IX_Contacts_UKPRN] ON [Contacts] ([ApplyOrganisationId])
GO
CREATE INDEX [IX_Contacts_SignInId] ON [Contacts] ([SignInId])
GO
CREATE INDEX [IX_Contacts_SignInType] ON [Contacts] ([SignInType])
GO