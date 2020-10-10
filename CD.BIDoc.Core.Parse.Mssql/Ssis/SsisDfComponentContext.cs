using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Parse.Mssql.Db;
using CD.DLS.DAL.Objects.Extract;
using System.Collections.Generic;
using System.Xml;
using CD.DLS.Model.Interfaces;

namespace CD.DLS.Parse.Mssql.Ssis
{
    public class SsisDfComponentContext
    {
        public SsisDfComponentContext(
            SsisIndex referrables,
            ConnectionIndex connections,
            Dictionary<string, TableSourceColumnList> tempTablesAvailable,
            SsisDfComponent component,
            XmlElement componentDefinitionXml,
            ComponentAllIO componentIO,
            DfInnerElement dfElement,
            UrnBuilder urnBuilder,
            SsisXmlProvider definitionSearcher,
            AvailableDatabaseModelIndex dbIndex,
            ISqlScriptModelParser sqlScriptExtractor,
            RefPath componentRefPath
            )
        {
            Referrables = referrables;
            Connections = connections;
            TempTablesAvailable = tempTablesAvailable;
            Component = component;
            ComponentDefinitionXml = componentDefinitionXml;
            ComponentIO = componentIO;
            DfElement = dfElement;
            UrnBuilder = urnBuilder;
            DefinitionSearcher = definitionSearcher;
            DbIndex = dbIndex;
            SqlScriptExtractor = sqlScriptExtractor;
            ComponentRefPath = componentRefPath;
        }

        /// <summary>
        /// Variable and parameter index
        /// </summary>
        public SsisIndex Referrables { get; }
        /// <summary>
        /// Connections available to the package
        /// </summary>
        public ConnectionIndex Connections { get; }
        /// <summary>
        /// The DF component metadata extracted from source
        /// </summary>
        public SsisDfComponent Component { get; }
        /// <summary>
        /// XML definition of the component
        /// </summary>
        public XmlElement ComponentDefinitionXml { get; }
        /// <summary>
        /// Input and output collection
        /// </summary>
        public ComponentAllIO ComponentIO { get; }
        /// <summary>
        /// Temp tables created in the package
        /// </summary>
        public Dictionary<string, TableSourceColumnList> TempTablesAvailable { get; }
        /// <summary>
        /// The containing dataflow element, representing the pipeline inside the Dataflow task
        /// </summary>
        public DfInnerElement DfElement { get; }
        /// <summary>
        /// RefPath provider
        /// </summary>
        public UrnBuilder UrnBuilder { get; }
        /// <summary>
        /// XML helper
        /// </summary>
        public SsisXmlProvider DefinitionSearcher { get; }
        /// <summary>
        /// SQL database context
        /// </summary>
        public AvailableDatabaseModelIndex DbIndex { get; }
        /// <summary>
        /// SQL parrser
        /// </summary>
        public ISqlScriptModelParser SqlScriptExtractor { get; }
        /// <summary>
        /// Ref Path prepared to be assigned to the newly created component element
        /// </summary>
        public RefPath ComponentRefPath { get; }
    }
}
