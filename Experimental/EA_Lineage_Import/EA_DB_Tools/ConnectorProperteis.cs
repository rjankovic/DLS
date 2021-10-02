namespace EA_DB_Tools
{
    public enum ConnectorTypeEnum { Dependency, InformationFlow, Association, DirectedAssociation }
    public class ConnectorProperteis
    {
        public EA.Element SourceElement;
        public EA.Element TargetElement;
        public string Name;
        public ConnectorTypeEnum ConnectorType;
        public string SourceAttributeGUID;
        public string TargetAttributeGUID;
    }
}