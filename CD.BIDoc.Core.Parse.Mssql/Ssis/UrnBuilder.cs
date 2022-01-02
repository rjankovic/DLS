using CD.DLS.Model.Mssql.Ssis;
using CD.DLS.Model.Mssql;
using CD.DLS.DAL.Objects.Extract;
using CD.DLS.Model.Interfaces;
using System.Linq;

namespace CD.DLS.Parse.Mssql.Ssis
{
    public class UrnBuilder
    {

        public RefPath GetExpressionFragmentUrn(int ordinal, RefPath parent)
        {
            return parent.NamedChild("Fragment", "No_" + ordinal);
        }
        

        public RefPath GetExpressionUrn(RefPath parent, string name = null)
        {
            if (name == null)
            {
                return parent.Child("Expression");
            }
            return parent.NamedChild("Expression", name);
        }

        public RefPath GetProjectUrn(FolderElement parent, string name)
        {
            return parent.RefPath.NamedChild("Project", name);
        }

        public RefPath GetPackageUrn(ProjectElement parent, string name)
        {
            return parent.RefPath.NamedChild("Package", name);
        }

        public RefPath GetParameterUrn(SsisModelElement parent, BIDoc.Core.Parse.Mssql.Ssis.SsisParameter parameter)
        {
            return parent.RefPath.NamedChild("Parameter", parameter.Name);
        }

        public RefPath GetVariableUrn(SsisModelElement parent, string qualifiedName)
        {
            return parent.RefPath.NamedChild("Variable", qualifiedName);
        }

        public RefPath GetExecutableUrn(SsisModelElement parent, string executableName, string executableId)
        {
            return parent.RefPath.NamedChild("Executable", executableName + "_" + executableId);
        }

        public RefPath GetDfInnerUrn(SsisModelElement parent)
        {
            return parent.RefPath.Child("DataFlow");
        }

        public RefPath GetDfComponentUrn(SsisModelElement parent, string componentIdString)
        {
            //TODO: equal sign?
            return new RefPath(parent.RefPath.Path + string.Format("/Component[@IdString'{0}']", componentIdString));
        }

        public RefPath GetDfSourceComponentUrn(SsisModelElement parent, string componentIdString)
        {
            //TODO: equal sign?
            return new RefPath(parent.RefPath.Path + string.Format("/SourceComponent[@IdString'{0}']", componentIdString));
        }

        //public RefPath GetDfColumnUrn(SsisModelElement parent, IDTSOutputColumn100 column)
        //{
        //    return new RefPath(parent.RefPath.Path + string.Format("/Column[@IdString'{0}']", column.IdentificationString));
        //}

        public RefPath GetConnectionManagerUrn(SsisModelElement parent, string managerName)
        {
            return parent.RefPath.NamedChild("ConnectionManager", managerName);
        }

        //public RefPath GetParameterAssignmentUrn(SsisModelElement parent, SsisXmlProvider.ParamAssignment assignment)
        //{
        //    return parent.RefPath.NamedChild("ParameterAssignment", assignment.ParamName);
        //}

        public RefPath GetParameterAssignmentUrn(SsisModelElement parent, string paramName)
        {
            return parent.RefPath.NamedChild("ParameterAssignment", paramName);
        }


        public RefPath GetPrecedenceConstraintUrn(SsisModelElement parent, string constraintName)
        {
            return parent.RefPath.NamedChild("PrecedenceConstraint", constraintName);
        }
        public RefPath GetDfOutputUrn(SsisModelElement parent, string outputName)
        {
            return parent.RefPath.NamedChild("Output", outputName);
        }
        public RefPath GetDfOutputColumnUrn(SsisModelElement parent, string outputColumnName)
        {
            return parent.RefPath.NamedChild("Column", outputColumnName + "_" + parent.Children.Count().ToString());
        }

        public RefPath DfColumnAggregationLinkElement(SsisModelElement parent, string SourceColumnName, string TargetColumnName, string SourceColumnLineageId, int index)
        {
            return parent.RefPath.NamedChild("Column", SourceColumnName + "_" + SourceColumnName + "_" + SourceColumnLineageId + "_" + index.ToString());
        }

        public RefPath GetDfInputUrn(SsisModelElement parent, string inputName)
        {
            return parent.RefPath.NamedChild("Input", inputName);
        }
        public RefPath GetDfInputColumnUrn(SsisModelElement parent, string inputColumnName)
        {
            return parent.RefPath.NamedChild("Column", inputColumnName + "_" + parent.Children.Count().ToString());
        }
        public RefPath GetDfLookupColumnUrn(SsisModelElement parent, string inputColumnName, string joinToColumn)
        {
            return new RefPath(parent.RefPath.Path + string.Format("/LookupColumn[@Name='{0}' and @JoinToColumn='{1}']", inputColumnName, joinToColumn));
        }

        public RefPath GetDfLookupOutputJoinToInputReferenceColumnUrn(SsisModelElement parent, DfColumnElement joinSource, DfColumnElement outputTarget)
        {
            return new RefPath(parent.RefPath.Path + string.Format("/LookupJoinOutputColumn[@OutputName='{0}' and @JoinToColumn='{1}']", outputTarget.Caption, joinSource.Caption));
        }

        public RefPath GetDfPathUrn(SsisModelElement parent, string pathIdString)
        {
            return parent.RefPath.NamedChild("Path", pathIdString);
        }

    }
}
