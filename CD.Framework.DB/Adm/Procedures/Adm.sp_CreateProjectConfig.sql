CREATE PROCEDURE [Adm].[sp_CreateProjectConfig]
	@projectconfigid UNIQUEIDENTIFIER,
	@name NVARCHAR(200)
AS
INSERT INTO Adm.ProjectConfigs(ProjectConfigId, Name) VALUES(@projectconfigid, @name)



INSERT INTO BIDoc.ModelElements(ExtendedProperties, RefPath, Definition, Caption, Type, RefPathPrefix, RefPathIntervalStart, RefPathIntervalEnd, ProjectConfigId)
VALUES(N'{}', N'', NULL, N'', N'CD.DLS.Model.SolutionModelElement', N'', 0, 0, @projectconfigid)

DECLARE @solutionElementId INT = (SELECT ModelElementId FROM BIDoc.ModelElements WHERE RefPath = N'' AND ProjectConfigId = @projectconfigid)

INSERT INTO BIDoc.ModelElements(ExtendedProperties, RefPath, Definition, Caption, Type, RefPathPrefix, RefPathIntervalStart, RefPathIntervalEnd, ProjectConfigId)
VALUES(N'{}', N'Business', NULL, N'Business Objects', N'CD.DLS.Model.Business.Organization.BusinessRootElement', N'Business', 0, 0, @projectconfigid)

DECLARE @businessElementId INT = (SELECT ModelElementId FROM BIDoc.ModelElements WHERE RefPath = N'Business' AND ProjectConfigId = @projectconfigid)

INSERT INTO BIDoc.ModelLinks(ElementFromId, ElementToId, [Type], ExtendedProperties)
VALUES (@businessElementId, @solutionElementId, N'parent', N'{}')

INSERT INTO BIDoc.ModelElements(ExtendedProperties, RefPath, Definition, Caption, Type, RefPathPrefix, RefPathIntervalStart, RefPathIntervalEnd, ProjectConfigId)
VALUES(N'{}', N'Business/SharedFolder', NULL, N'Shared Artefacts', N'CD.DLS.Model.Business.Organization.BusinessFolderElement', N'Business/SharedFolder', 0, 0, @projectconfigid)

DECLARE @sharedFolderId INT = (SELECT ModelElementId FROM BIDoc.ModelElements WHERE RefPath = N'Business/SharedFolder' AND ProjectConfigId = @projectconfigid)

INSERT INTO BIDoc.ModelLinks(ElementFromId, ElementToId, [Type], ExtendedProperties)
VALUES (@sharedFolderId, @businessElementId, N'parent', N'{}')
