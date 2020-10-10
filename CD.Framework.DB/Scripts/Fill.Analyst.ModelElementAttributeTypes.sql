IF NOT EXISTS (SELECT TOP 1 1 FROM Analyst.ModelElementAttributeTypes WHERE Code = N'String')
INSERT INTO Analyst.ModelElementAttributeTypes([Name], [Code]) VALUES
(N'String', N'String')

IF NOT EXISTS (SELECT TOP 1 1 FROM Analyst.ModelElementAttributeTypes WHERE Code = N'AnalystLink')
INSERT INTO Analyst.ModelElementAttributeTypes([Name], [Code]) VALUES
(N'AnalystLink', N'Link')

IF NOT EXISTS (SELECT TOP 1 1 FROM Analyst.ModelElementAttributeTypes WHERE Code = N'BIDocLink')
INSERT INTO Analyst.ModelElementAttributeTypes([Name], [Code]) VALUES
(N'BIDocLink', N'BI Doc Link')

IF NOT EXISTS (SELECT TOP 1 1 FROM Analyst.ModelElementAttributeTypes WHERE Code = N'Numeric')
INSERT INTO Analyst.ModelElementAttributeTypes([Name], [Code]) VALUES
(N'Numeric', N'Numeric')

IF NOT EXISTS (SELECT TOP 1 1 FROM Analyst.ModelElementAttributeTypes WHERE Code = N'DateTime')
INSERT INTO Analyst.ModelElementAttributeTypes([Name], [Code]) VALUES
(N'DateTime', N'Datetime')

IF NOT EXISTS (SELECT TOP 1 1 FROM Analyst.ModelElementAttributeTypes WHERE Code = N'Enumeration')
INSERT INTO Analyst.ModelElementAttributeTypes([Name], [Code]) VALUES
(N'Enum', N'Enumeration')



