using System;
using System.Collections.Generic;
using System.Linq;
using CD.DLS.Model.Mssql.Ssas;
using CD.DLS.Model.Mssql;
using System.Data;
using Irony.Parsing;
using CD.DLS.DAL.Configuration;
using CD.DLS.DAL.Objects.Extract;

namespace CD.DLS.Parse.Mssql.Ssas
{
    /// <summary>
    /// Extracts a model of MDX scripts - mainly cube calculations and reporting datasets.
    /// </summary>
    public class MdxScriptModelExtractor
    {
        private Parser _parser;
        private UrnBuilder _urnBuilder;
        
        public MdxScriptModelExtractor()
        {
            _parser = new Parser(new MdxGrammar());
            _urnBuilder = new UrnBuilder();
        }

        public IEnumerable<ParseTreeNode> DFTraverse(ParseTree parseTree)
        {
            return DFTraverseInner(parseTree.Root);
        }

        public IEnumerable<ParseTreeNode> DFTraverseInner(ParseTreeNode node)
        {
            yield return node;
            foreach (var child in node.ChildNodes)
            {
                foreach (var childTraverseItem in DFTraverseInner(child))
                {
                    yield return childTraverseItem;
                }
            }
        }
        
        public void ExtractCubeCalculations(MultidimensionalCube cube, CubeElement cubeElement, SsasMultidimensionalDatabaseIndex environment)
        {
            ExtractCubeCalculationDeclarations(cube, cubeElement, environment);
            ExtractReferencesFromCalculations(cube, cubeElement, environment);
            environment.ClearLocalIndexes();
        }



        public MdxStatementElement ExtractMdxStatement(string statement, SsasDatabaseIndex environment, MssqlModelElement parent, out MdxStatementIndex resultingLocalEnvironment)
        {
            var multidimensionalEnvironment = environment as SsasMultidimensionalDatabaseIndex;
            var tabularEnvirionment = environment as SsasTabularDatabaseIndex;
            var isMultidimensional = multidimensionalEnvironment != null;

            resultingLocalEnvironment = new MdxStatementIndex();
            //if (statement.Contains("MEMBER [Measures].[GP LY 12 month] AS ([OGM Segment].[OGM Segment].[All],[Measures].[OGMT Prev 12 M GP])"))
            //{

            //}

            var statementUrn = _urnBuilder.GetStatementUrn(parent);
            var statementElement = new MdxStatementElement(statementUrn, statementUrn.RefId, statement, parent);
            
            var parsed = _parser.Parse(statement);
            if (parsed.Root == null)
            {
                // TODO log errors
                foreach (var msg in parsed.ParserMessages)
                {
                    ConfigManager.Log.Warning(string.Format("Error parsing MDX: {0} | at {1}:{2} | {3} | {4}", 
                        statement, msg.Location.Line, msg.Location.Column,  msg.Message, parent.RefPath.Path));
                }
                return null;
            }

            var navigator = new MdxParseTreeNavigator(parsed);
            var parts = navigator.GetTermsAndContent(parsed.Root, statement);

            string originalCubeInUse = null;
            if (isMultidimensional)
            {
                originalCubeInUse = multidimensionalEnvironment.CubeInUse;
            }
            
            if (isMultidimensional)
            {
                var cubeIdentifier = navigator.GetCubeId(parsed.Root);
                var cubeIdText = cubeIdentifier.GetText(statement);
                if (cubeIdText.StartsWith("["))
                {
                    cubeIdText = cubeIdText.TrimStart('[').TrimEnd(']');
                }
                multidimensionalEnvironment.CubeInUse = cubeIdText;

                var cubeIndex = multidimensionalEnvironment.GetCubeIndex(cubeIdText);
                resultingLocalEnvironment.CubeIndex = cubeIndex;
            }
            
            // WITH ... measures
            var localMeasures = navigator.GetCalculatedMembers();
            foreach (var calculatedMember in localMeasures)
            {
                var nameParts = navigator.GetCalculatedMemberNameParts(calculatedMember);
                var definitionText = calculatedMember.GetText(statement);
                var measureName = nameParts.Last();
                var calculationUrn = _urnBuilder.GetCalculatedMeasureUrn(measureName, statementElement.RefPath);

                var calculatedMeasureElement = new ReportCalculatedMeasureElement(_urnBuilder.GetCalculatedMeasureUrn(measureName, statementElement.RefPath), measureName, definitionText, statementElement);
                calculatedMeasureElement.OffsetFrom = calculatedMember.Span.EndPosition - calculatedMember.Span.Length;
                calculatedMeasureElement.Length = calculatedMember.Span.Length;
                statementElement.AddChild(calculatedMeasureElement);
                if (isMultidimensional)
                {
                    multidimensionalEnvironment.AddLocalMeasure(calculatedMeasureElement);
                }
                else
                {
                    tabularEnvirionment.AddLocalMeasure("Measures", calculatedMeasureElement.Caption, calculatedMeasureElement);
                }
            }

            var axes = navigator.GetTopLevelAxisSpecifications();

            Dictionary<Tuple<int, int>, MdxFragmentElement> indexedFragmentElements = new Dictionary<Tuple<int, int>, MdxFragmentElement>();

            int fragmentOrdinal = 0;
            foreach (var axis in axes)
            {
                var axisId = navigator.GetAxisId(axis, statement);
                var axisItems = navigator.GetAxisItemSelection(axis);
                foreach (var axisItem in axisItems)
                {
                    var itemName = axisItem.GetText(statement);
                    var termsAndContent = navigator.GetTermsAndContent(axisItem, statement);
                    var axisOffsetFrom = axisItem.Span.EndPosition - axisItem.Span.Length;
                    var axisLength = axisItem.Span.Length;
                    var closestParent = FindFragmentParent(statementElement, axisOffsetFrom, axisLength);
                    var fragmentUrn = _urnBuilder.GetScriptSegmentUrn(fragmentOrdinal++, closestParent.RefPath);

                    var fragmentElement = new MdxFragmentElement(fragmentUrn, itemName, itemName, closestParent);
                    fragmentElement.OffsetFrom = axisOffsetFrom;
                    fragmentElement.Length = axisLength;
                    indexedFragmentElements.Add(new Tuple<int, int>(axisItem.Span.EndPosition, axisItem.Span.Length), fragmentElement);

                    closestParent.AddChild(fragmentElement);
                    resultingLocalEnvironment.AddElementOnAxis(fragmentElement, axisId);
                }
            }


            var potRefs = navigator.GetPotentialReferences(parsed.Root).Select(x => x.GetText(statement)).ToList();

            //if (statement.Contains("[Client].[Client SID].[Client Number]"))
            //{
            //}
            
            foreach (var potentialReference in navigator.GetPotentialReferences(parsed.Root))
            {
                var identifier = potentialReference.GetText(statement);
                int resolutionLength;
                var refTokenType = potentialReference.Term.Name;
                // "expression_property": standard
                // "property": member property
                //if (identifier.Contains("[Client].[Client SID].[Client Number]"))
                //{

                //}

                SsasModelElement resolution = null;
                if (isMultidimensional)
                {
                    resolution = multidimensionalEnvironment.TryResolveIdentifier(identifier, out resolutionLength);
                }
                else
                {
                    tabularEnvirionment.QueryMode = SsasQueryMode.MDX;
                    // cannot be table...
                    resolution = tabularEnvirionment.TryResolveIdentifier(identifier, TabularReferenceType.Column);
                    if (resolution != null)
                    {
                        resolutionLength = identifier.Length;
                    }
                    
                    //resolution = tabularEnvirionment.TryResolveIdentifier()
                    tabularEnvirionment.QueryMode = SsasQueryMode.DAX;
                }
                if (resolution != null)
                {
                    MdxFragmentElement refSegmentElement;
                    var spanTuple = new Tuple<int, int>(potentialReference.Span.EndPosition, potentialReference.Span.Length);
                    if (indexedFragmentElements.ContainsKey(spanTuple))
                    {
                        refSegmentElement = indexedFragmentElements[spanTuple];
                    }
                    else
                    {
                        var referenceOffsetFrom = potentialReference.Span.EndPosition - potentialReference.Span.Length;
                        var referenceLength = potentialReference.Span.Length;
                        var closestParent = FindFragmentParent(statementElement, referenceOffsetFrom, referenceLength);
                        var segmentUrn = _urnBuilder.GetScriptSegmentUrn(fragmentOrdinal++, closestParent.RefPath);
                        refSegmentElement = new MdxFragmentElement(segmentUrn, "Segment_" + fragmentOrdinal, identifier, closestParent);
                        closestParent.AddChild(refSegmentElement);
                    }
                    refSegmentElement.OffsetFrom = potentialReference.Span.EndPosition - potentialReference.Span.Length;
                    refSegmentElement.Length = potentialReference.Span.Length;
                    refSegmentElement.Reference = resolution;
                }
            }

            if (isMultidimensional)
            {
                if (originalCubeInUse != null)
                {
                    multidimensionalEnvironment.CubeInUse = originalCubeInUse;
                }
            }

            environment.ClearLocalIndexes();

            return statementElement;
        }

        private MdxFragmentElement FindFragmentParent(MdxFragmentElement root, int offset, int length)
        {
            var container = root.Children.OfType<MdxFragmentElement>().FirstOrDefault(x => x.OffsetFrom <= offset && x.OffsetFrom + x.Length >= offset + length);
            if (container != null)
            {
                return FindFragmentParent(container, offset, length);
            }
            return root;
        }

        private void ExtractCubeCalculationDeclarations(MultidimensionalCube cube, CubeElement cubeElement, SsasMultidimensionalDatabaseIndex environment)
        {
            if (cube == null)
            {
                return;
            }
            if (cube.MdxScripts == null)
            {
                return;
            }
            foreach (CubeMdxScript mdxScript in cube.MdxScripts)
            {
                ConfigManager.Log.Important("Extracting MDX script" + mdxScript.Name);
                
                var scriptUrn = _urnBuilder.GetMdxScriptUrn(mdxScript.Name, cubeElement.RefPath);
                var scriptElement = new MdxScriptElement(scriptUrn, mdxScript.Name, mdxScript.Command, cubeElement);
                cubeElement.AddChild(scriptElement);
                environment.AddScript(scriptElement);

                //if (mdxScript.Commands.Count > 1)
                //{
                //    throw new Exception();
                //}
                //foreach (Command command in mdxScript.Commands)
                //{
                    ConfigManager.Log.Info("Parsing MDX:" + Environment.NewLine + mdxScript.Command);
                    var commandText = mdxScript.Command;
                    var parsed = _parser.Parse(commandText);
                if (parsed.HasErrors())
                {
                    ConfigManager.Log.Error("Failed to parse '" + commandText + "'");

                    foreach (var message in parsed.ParserMessages)
                    {
                        ConfigManager.Log.Error("MDX parser message: " + message.Message + " at " + message.Location.Line + ":" + message.Location.Position);
                    }
                    continue;
                }
                    
                      //      continue;
                    //}

                    //if (parsed.Root == null)
                    //{
                    //    continue;
                    //}

                    //continue;

                    var navigator = new MdxParseTreeNavigator(parsed);
                    scriptElement.OffsetFrom = parsed.Root.Span.EndPosition - parsed.Root.Span.Length;
                    scriptElement.Length = parsed.Root.Span.Length;
                    var calculatedMembers = navigator.GetCalculatedMembers();


                    foreach (var calculatedMember in calculatedMembers)
                    {
                        var nameParts = navigator.GetCalculatedMemberNameParts(calculatedMember);
                        var definitionText = calculatedMember.GetText(commandText);

                        bool isMeasure = nameParts.Count == 1 || nameParts[nameParts.Count - 2].ToLower() == "measures";
                        if (isMeasure)
                        {
                            var measureName = nameParts.Last();
                            var calculatedMeasureElement = new CubeCalculatedMeasureElement(_urnBuilder.GetCalculatedMeasureUrn(measureName, scriptElement.RefPath), measureName, definitionText, scriptElement);
                            calculatedMeasureElement.OffsetFrom = calculatedMember.Span.EndPosition - calculatedMember.Span.Length;
                            calculatedMeasureElement.Length = calculatedMember.Span.Length;
                            scriptElement.AddChild(calculatedMeasureElement);
                            environment.AddMeasure(calculatedMeasureElement);

                            //var tokensAndContent = navigator.GetTermsAndContent(calculatedMember, commandText);
                        }
                    }
                }
            }

        private void ExtractReferencesFromCalculations(MultidimensionalCube cube, CubeElement cubeElement, SsasMultidimensionalDatabaseIndex environment)
        {
            foreach (CubeMdxScript mdxScript in cube.MdxScripts)
            {
                var scriptElement = environment.FindScript(mdxScript.Name);
                //foreach (Command command in mdxScript.Commands)
                //{
                    var commandText = mdxScript.Command;
                    var parsed = _parser.Parse(commandText);

                if (parsed.HasErrors())
                {
                    ConfigManager.Log.Error("Failed to parse '" + commandText + "'");

                    foreach (var message in parsed.ParserMessages)
                    {
                        ConfigManager.Log.Error("MDX parser message: " + message.Message + " at " + message.Location.Line + ":" + message.Location.Position);
                    }
                    continue;
                }
                    //    continue;
                    //}

                    var navigator = new MdxParseTreeNavigator(parsed);

                    // calculated measures
                    var calculatedMembers = navigator.GetCalculatedMembers();
                    foreach (var calculatedMember in calculatedMembers)
                    {
                        var nameParts = navigator.GetCalculatedMemberNameParts(calculatedMember);
                        var definitionText = calculatedMember.GetText(commandText);

                        bool isMeasure = nameParts.Count == 1 || nameParts[nameParts.Count - 2].ToLower() == "measures";
                        if (isMeasure)
                        {
                            var measureName = nameParts.Last();
                            var fullMeasureIdentifier = string.Format("[Measures].[{0}]", measureName);
                            var measureElement = environment.TryResolveIdentifier(fullMeasureIdentifier);
                            if (measureElement == null)
                            {
                                throw new Exception();
                            }

                            var refOrdinal = 0;
                            foreach (var potentialReference in navigator.GetPotentialReferences(calculatedMember))
                            {
                                refOrdinal++;
                                var identifier = potentialReference.GetText(commandText);
                                int resolutionLength;
                                var resolution = environment.TryResolveIdentifier(identifier, out resolutionLength);
                                if (resolution != null)
                                {
                                    var segmentUrn = _urnBuilder.GetScriptSegmentUrn(refOrdinal, measureElement.RefPath);
                                    var refSegmentElement = new MdxFragmentElement(segmentUrn, "Segment_" + refOrdinal, identifier, measureElement);
                                    refSegmentElement.OffsetFrom = potentialReference.Span.EndPosition - potentialReference.Span.Length /*- (calculatedMember.Span.EndPosition - calculatedMember.Span.Length)*/;
                                    refSegmentElement.Length = potentialReference.Span.Length;
                                    refSegmentElement.Reference = resolution;
                                    measureElement.AddChild(refSegmentElement);
                                }
                            }

                            if (measureElement != null && measureElement.RefPath.Path == "SSASServer[@Name='RJ-THINK']/Db[@Name='Manpower_SSAS']/Cube[@Name='Manpower']/MdxScript[@Name='MdxScript']/CalculatedMeasure[@Name='BLC']")
                            {

                            }
                        }
                    }

                    // scopes
                    var scopes = navigator.GetScopeStatements();

                    var scopeOrdinal = 0;
                    foreach (var scope in scopes)
                    {
                        scopeOrdinal++;
                        var definitionText = scope.GetText(commandText);
                        var scopeUrn = _urnBuilder.GetScopeStatementUrn(scopeOrdinal, scriptElement.RefPath);
                        ScopeStatementElement scopeElement = new ScopeStatementElement(scopeUrn, "SCOPE_" + scopeOrdinal, definitionText, scriptElement);

                        var termsAndContext = navigator.GetTermsAndContent(scope, commandText);

                        var refOrdinal = 0;
                        foreach (var potentialReference in navigator.GetPotentialReferences(scope))
                        {
                            refOrdinal++;
                            var identifier = potentialReference.GetText(commandText);
                            int resolutionLength;
                            var resolution = environment.TryResolveIdentifier(identifier, out resolutionLength);
                            if (resolution != null)
                            {
                                var segmentUrn = _urnBuilder.GetScriptSegmentUrn(refOrdinal, scopeElement.RefPath);
                                var refSegmentElement = new MdxFragmentElement(segmentUrn, "Segment_" + refOrdinal, identifier, scopeElement);
                                refSegmentElement.OffsetFrom = potentialReference.Span.EndPosition - potentialReference.Span.Length - (scope.Span.EndPosition - scope.Span.Length);
                                refSegmentElement.Length = resolutionLength; // potentialReference.Span.Length;
                                refSegmentElement.Reference = resolution;
                                scopeElement.AddChild(refSegmentElement);
                            }
                        }

                    }
                }
            }
        }

}