using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.DAL.Objects.Learning
{
    //public enum OLAPFieldsMLInputItemType { MEASURE, ATTRIBUTE, FILTER }

    //public class OlapFieldsMLInputItem
    //{
    //    public Guid ProjectConfigId { get; set; }
    //    public string GroupPath { get; set; }
    //    public OLAPFieldsMLInputItemType FieldType { get; set; }
    //    public string ReferenceRefPath { get; set; }
    //    public string FieldReference { get; set; }
    //    public int OlapElementId { get; set; }
    //    public string FieldName { get; set; }

    //}

    public class OlapQueryField
    {
        public int OlapQueryFieldId { get; set; }
        public int QueryElementId { get; set; }
        public int OlapFieldId { get; set; }
    }

    // f.OlapFieldId, f.FieldElementId, f.FieldName, f.FieldReference, f.FieldType

    public enum OlapFieldType { Axis, Measure, Filter }

    public class OlapField
    {
        public int OlapFieldId { get; set; }
        public int FieldElementId { get; set; }
        public string FieldName { get; set; }
        public string FieldReference { get; set; }
        public OlapFieldType FieldType { get; set; }
        public string ServerName { get; set; }
        public string DbName { get; set; }
        public string CubeName { get; set; }
    }

    public class OlapRule
    {
        public List<OlapField> PremiseFields { get; set; }
        public List<OlapField> ConclusionFields { get; set; }
        public double Confidence { get; set; }
        public double Support { get; set; }
        public string RuleCode { get; set; }
        public int OlapRuleId { get; set; }
        public string ServerName { get; set; }
        public string DbName { get; set; }
        public string CubeName { get; set; }
    }

    public class OlapRuleSet
    {
        public List<OlapField> Fields { get; set; }
        public List<OlapRule> Rules { get; set; }
        public Guid ProjectConfigId { get; set; }
    }
}

