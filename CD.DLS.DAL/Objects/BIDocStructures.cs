using System;
using CD.DLS.Common.Structures;

namespace CD.DLS.DAL.Objects.BIDoc
{
    public class BIDocModelLink
    {
        public int Id { get; set; }

        public int ElementFromId { get; set; }
        public int ElementToId { get; set; }
        public string Type { get; set; }

        public string ExtendedProperties { get; set; }

        public Guid ProjectConfigId { get; set; }
    }

    public class BIDocModelElement
    {
        public int Id { get; set; }

        public string ExtendedProperties { get; set; }
        public string RefPath { get; set; }
        public string Definition { get; set; }
        public string Caption { get; set; }
        public string Type { get; set; }

        public Guid ProjectConfigId { get; set; }

        public override string ToString()
        {
            return RefPath;
        }

        public string RefPathSuffix
        {
            get
            {
                if (RefPath.LastIndexOf("]/") > -1)
                {
                    return RefPath.Substring(RefPath.LastIndexOf("]/") + 2).PadRight(300).Substring(0, 300);
                }
                else
                {
                    return RefPath.PadRight(300).Substring(0, 300);
                }
            }
        }
    }
    public class BIDocGraphInfoNode
    {
        public int Id { get; set; }
        //public string RefPath { get; set; }
        public string Name { get; set; }
        public string NodeType { get; set; }
        //public string DocumentRelativePath { get; set; }
        public string Description { get; set; }

        public int? ParentId { get; set; }

        public DependencyGraphKind GraphKind { get; set; }

        public Guid ProjectConfigId { get; set; }

        public int SourceElementId { get; set; }

        public int TopologicalOrder { get; set; }
    }

    public class BIDocGraphInfoNodeExtended : BIDocGraphInfoNode
    {
        public string RefPath { get; set; }
        public string TypeDescription { get; set; }
        public string ElementType { get; set; }
        public string DescriptivePath { get; set; }
    }

    //public class BIDocDataFlowGraphInfoNode : BIDocGraphInfoNode
    //{
    //    public int TopologicalOrder { get; set; }
    //}

    public class BIDocGraphInfoLink
    {
        public int Id { get; set; }
        public LinkTypeEnum LinkType { get; set; }

        public int NodeFromId { get; set; }
        public int NodeToId { get; set; }
        public string ExtendedProperties { get; set; }

        public Guid ProjectConfigId { get; set; }
    }

    public class ReportElementListItem
    {
        public string ItemPath { get; set; }
        public string Caption { get; set; }
        public int ModelElementId { get; set; }
        public string RefPath { get; set; }

    }

    public class BIDocGraphDocument : GraphDocument
    {
        //public virtual BIDocGraphInfoNode GraphNode { get; set; }
        public Guid ProjectConfigId { get; set; }
    }

    public class BIDocWarningMessages
    {
        public string SourceName { get; set; }
        public string SourcePath { get; set; }
        public string TargetName { get; set; }
        public string TargetPath { get; set; }
        public string DataMessageType { get; set; }
        public string Message { get; set; }
    }

    /*
     
SELECT
h.SourceElementType, h.TargetElementType,
h.SourceRootElementId, h.TargetRootElementId,

MAX(sdp.DescriptivePath) SourceRootDescriptivePath, MAX(tdp.DescriptivePath) TargetRootDescriptivePath,
MAX(st.TypeDescription) SourceTypeDescription, MAX(tt.TypeDescription) TargetTypeDescription

FROM BIDoc.LineageGridHistory h
INNER JOIN BIDoc.ModelElementDescriptivePaths sdp ON sdp.ModelElementId = h.SourceRootElementId
INNER JOIN BIDoc.ModelElementDescriptivePaths tdp ON tdp.ModelElementId = h.TargetRootElementId
INNER JOIN BIDoc.ModelElementTypeDescriptions st ON st.ElementType = h.SourceElementType
INNER JOIN BIDoc.ModelElementTypeDescriptions tt ON tt.ElementType = h.TargetElementType
WHERE h.UserId = @userId
GROUP BY h.SourceRootElementId, h.TargetRootElementId, 
h.SourceElementType, h.TargetElementType

ORDER BY COUNT(*) * 10 * MAX(DATEDIFF(DAY, h.CreatedDateTime, GETDATE()) + 1) DESC
     */

    public class LineageGridFavorite
    {
        public string SourceElementType { get; set; }
        public string TargetElementType { get; set; }
        public int SourceRootElementId { get; set; }
        public int TargetRootElementId { get; set; }
        public string SourceRootDescriptivePath { get; set; }
        public string TargetRootDescriptivePath { get; set; }
        public string SourceTypeDescription { get; set; }
        public string TargetTypeDescription { get; set; }
        public string SourceRootElementPath { get; set; }
        public string TargetRootElementPath { get; set; }
    }
}
