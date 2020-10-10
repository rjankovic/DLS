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
--:r .\Scripts\Fill.ProjectsConfig.Manpower.sql
--:r .\Scripts\Fill.ProjectsConfig.SO.sql
--:r .\Scripts\Fill.ProjectsConfig.Provident.sql
--:r .\Scripts\Fill.Analyst.ModelElementAttributeTypes.sql
--:r .\Scripts\Fill.Analyst.Objecst.sql

--:r .\Scripts\EnableServiceBroker.sql

--:r .\Scripts\Fill.ProjectsConfig.Test.sql
--:r .\Scripts\Fill.ProjectsConfig.NRWH.sql
:r .\Scripts\Fill.ModelElementTypeDescriptions.sql
:r .\Scripts\Fill.ModelElementTypeDetailLevels.sql
:r .\Scripts\Fill.ModelElementTypeClasses.sql
:r .\Scripts\Fill.GlobalConfig.sql
:r .\Scripts\Fill.DataTypesAndSourceDataTypes.sql
:r .\Scripts\Fill.DataMessagesType.sql
:r .\Scripts\Fill.Search.TypeChildTypes.sql
:r .\Scripts\Fill.Search.BusinessDictionarySupportedType.sql
--:r .\Scripts\Fill.CreateDataFlowSequences.sql
:r .\Scripts\Fill.Adm.ServerRoles.sql
:r .\Scripts\Fill.Type.HighLevelTypeDescendants.sql
:r .\Scripts\Fill.SequenceEndpointTypes.sql
:r .\Scripts\Fill.AnnotationElementTypeDescriptions.sql