/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

-- ON-1917 Clean up double spaces on Organisation Name
UPDATE [dbo].[Organisations]
SET	   [Name] = REPLACE([Name], '  ', ' ')
      ,[OrganisationDetails] = JSON_MODIFY([OrganisationDetails], '$.LegalName', REPLACE(JSON_VALUE([OrganisationDetails], '$.LegalName'), '  ', ' '))
WHERE CHARINDEX('  ', [Name]) > 0 OR CHARINDEX('  ', JSON_VALUE([OrganisationDetails], '$.LegalName')) > 0;
-- END OF: ON-1917 


