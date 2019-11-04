CREATE TABLE [dbo].[ApplicationReviews](
	[Id] [uniqueidentifier] NOT NULL,
	[ApplicationId] [uniqueidentifier] NOT NULL,
	[IsActive] [bit] NOT NULL,
	[GatewayReviewStatus] [nvarchar](20) NOT NULL,
	[PmoReviewStatus] [nvarchar](20) NOT NULL,
	[AssessorReview1Status] [nvarchar](20) NOT NULL,
	[AssessorReview2Status] [nvarchar](20) NOT NULL,
	[AssessorModerationStatus] [nvarchar](20) NOT NULL,
	[AssessorReview1Comments] [nvarchar](max) NULL,
	[AssessorReview2Comments] [nvarchar](max) NULL,
    [LegalChecks] NVARCHAR(MAX) NULL, 
    [AddressChecks] NVARCHAR(MAX) NULL, 
    CONSTRAINT [PK_ApplicationReviews] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY], 
    CONSTRAINT [AK_ApplicationReviews_ApplicationId] UNIQUE ([ApplicationId])
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE [dbo].[ApplicationReviews]  WITH CHECK ADD  CONSTRAINT [FK_ApplicationReviews_Applications] FOREIGN KEY([ApplicationId])
REFERENCES [dbo].[Applications] ([Id])
GO

ALTER TABLE [dbo].[ApplicationReviews] CHECK CONSTRAINT [FK_ApplicationReviews_Applications]
GO