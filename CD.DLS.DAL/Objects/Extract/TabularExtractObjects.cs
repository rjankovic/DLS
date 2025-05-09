using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Extract
{

    #region Tabular

    public class TabularItem : ExtractObject
    {
        public string DbName;
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.TabularDB;

        public override string Name => DbName;

        public string Content { get; set; }
    }

    public class TabularDB : ExtractObject
    {
        public TabularDB()
        {
            Annotations = new List<TabularAnnotation>();
        }

        public string DBName;
        public List<TabularAnnotation> Annotations;
        public string Collation;
        public string Description;
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.TabularDB;

        public override string Name => DBName;
    }

    public class TabularModel : ExtractObject
    {
        public TabularModel()
        {
            TabularDataSources = new List<TabularDataSource>();
            TabularTables = new List<TabularTable>();
            Relationships = new List<TabularRelationship>();
            Perspectives = new List<TabularPerspective>();
            Cultures = new List<TabularCulture>();
            Annotations = new List<TabularAnnotation>();
        }

        public string modelName;
        public string Id;
        public string Description;
        public string DBName;
        public string WorkspaceName;
        public string ContentProviderType;
        public List<TabularDataSource> TabularDataSources;
        public List<TabularTable> TabularTables;
        public List<TabularRelationship> Relationships;
        public List<TabularPerspective> Perspectives;
        public List<TabularCulture> Cultures;
        public override ExtractTypeEnum ExtractType => ExtractTypeEnum.TabularModel;
        public List<TabularAnnotation> Annotations;

        public override string Name => modelName;
    }

    public class TabularDataSource
    {
        public TabularDataSource()
        {
            Annotations = new List<TabularAnnotation>();
        }

        public string DSname;
        public string Description;
        public string SourceType;
        public string ServerName;
        public string DatabaseName;
        public string ConnectionString;
        public List<TabularAnnotation> Annotations;

    }

    public class TabularTable  //ha ha ha
    {
        public TabularTable()
        {
            Columns = new List<TabularTableColumn>();
            Partitions = new List<TabularTablePartition>();
            Measures = new List<TabularTableMeasure>();
            Hierarchies = new List<TabularTableHierarchy>();
            Annotations = new List<TabularAnnotation>();
        }
        public string Name;
        public List<TabularTableColumn> Columns;
        public List<TabularTablePartition> Partitions;
        public List<TabularTableMeasure> Measures;
        public List<TabularTableHierarchy> Hierarchies;
        public List<TabularAnnotation> Annotations;

    }

    public enum TabularTableColumnTypeEnum { RowNumber = 1, Data = 2, Calculated = 3, CalculatedTableColumn = 4 }
    public enum TabularRelationshipEndCardinality
    {
        None = 0,
        One = 1,
        Many = 2
    }

    public class TabularTableColumn
    {
        public TabularTableColumn()
        {
            Annotations = new List<TabularAnnotation>();
        }
        public string Name;
        public string DataType;
        public string SourceColumn;
        public string Expression;
        public TabularTableColumnTypeEnum ColumnType;
        public List<TabularAnnotation> Annotations;
        public TabularTableColumnAttributeHierarchy AttributeHierarchy;

    }

    public class TabularTableColumnAttributeHierarchy
    {
        public TabularTableColumnAttributeHierarchy()
        {
            Annotations = new List<TabularAnnotation>();
        }

        public string Name;
        public List<TabularAnnotation> Annotations;
    }

    public enum TabularPartitionSourceTypeEnum { QueryPartitionSource = 1, CalculatedPartitionSource = 2, MLanguagePartitionSource = 4 }

    public class TabularTablePartition
    {
        public TabularTablePartition()
        {
            Annotations = new List<TabularAnnotation>();
        }
        public TabularPartitionSourceTypeEnum PartitionSourceType;
        public string Query;
        public string Expression;
        public string Name;
        public string DataSourceName;
        public List<TabularAnnotation> Annotations;
    }

    public class TabularTableMeasure
    {
        public TabularTableMeasure()
        {
            Annotations = new List<TabularAnnotation>();
        }
        public string Name;
        public string Expression;
        public string ErrorMessage;
        public List<TabularAnnotation> Annotations;
    }

    public class TabularTableHierarchy
    {
        public TabularTableHierarchy()
        {
            Annotations = new List<TabularAnnotation>();
        }
        public string Name;
        public List<TabularAnnotation> Annotations;
    }

    public class TabularRelationship
    {
        public TabularRelationship()
        {
            Annotations = new List<TabularAnnotation>();
        }

        public string Name;
        public string FromTable;
        public string ToTable;
        public bool IsActive;
        public string FromColumn;
        public string ToColumn;
        public TabularRelationshipEndCardinality FromCardinality;
        public TabularRelationshipEndCardinality ToCardinality;

        public List<TabularAnnotation> Annotations;

    }

    public class TabularAnnotation
    {
        public string Name;
        public string Value;
    }

    public class TabularPerspective
    {
        public TabularPerspective()
        {
            Annotations = new List<TabularAnnotation>();
            PerspectiveTables = new List<TabularPerspectiveTable>();
        }

        public string Name;
        public List<TabularAnnotation> Annotations;
        public List<TabularPerspectiveTable> PerspectiveTables;
    }

    public class TabularPerspectiveTable
    {
        public TabularPerspectiveTable()
        {
            Annotations = new List<TabularAnnotation>();
            Columns = new List<TabularPerspectiveTableColumn>();
            Measures = new List<TabularPerspectiveMeasure>();
            Hierarchies = new List<TabularPerspectiveHierarchy>();

        }
        public string Name;
        public List<TabularAnnotation> Annotations;
        public List<TabularPerspectiveTableColumn> Columns;
        public List<TabularPerspectiveMeasure> Measures;
        public List<TabularPerspectiveHierarchy> Hierarchies;

    }

    public class TabularPerspectiveTableColumn
    {
        public TabularPerspectiveTableColumn()
        {
            Annotations = new List<TabularAnnotation>();
        }
        public string Name;
        public List<TabularAnnotation> Annotations;
    }

    public class TabularPerspectiveMeasure
    {
        public TabularPerspectiveMeasure()
        {
            Annotations = new List<TabularAnnotation>();
        }

        public string Name;
        public string Measure;
        public List<TabularAnnotation> Annotations;
    }

    public class TabularPerspectiveHierarchy
    {
        public TabularPerspectiveHierarchy()
        {
            Annotations = new List<TabularAnnotation>();
            HierarchyLevels = new List<TabularPerspectiveHierarchyLevel>();
        }

        public string Name;
        public string Hierarchy;
        public List<TabularAnnotation> Annotations;
        public List<TabularPerspectiveHierarchyLevel> HierarchyLevels;
    }

    public class TabularPerspectiveHierarchyLevel
    {
        public string Name;
    }

    public class TabularCulture
    {
        public TabularCulture()
        {
            Annotations = new List<TabularAnnotation>();
            Translations = new List<TabularTranslation>();
        }
        public string Name;
        public string LinguistincMetadata;
        public List<TabularAnnotation> Annotations;
        public List<TabularTranslation> Translations;
    }

    public class TabularTranslation
    {
        public string Value;
    }


    #endregion

}
