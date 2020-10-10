using CD.DLS.Common.Structures;
using CD.DLS.Model.Interfaces.DependencyGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Serialization
{
    class BIDocGraphInfoConverter
    {
        public BIDocGraphBulk ConvertToBIDocGraphInfo(IDependencyGraph dependecyGraph)
        {
            throw new NotImplementedException();
        }


        public BasicGraphInfo ConvertToBasicGraphInfo(BIDocGraphStored bidocGraphInfo)
        {
            BasicGraphInfo bgi = new BasicGraphInfo();

            Dictionary<int, int> parent_ids = new Dictionary<int, int>();

            foreach (var link in bidocGraphInfo.Links)
            {
                if (link.LinkType == LinkTypeEnum.Parent)
                {
                    parent_ids[link.NodeFromId] = link.NodeToId;
                }
                else
                {
                    bgi.Links.Add(new BasicGraphInfoLink
                    {
                        Id = link.Id,
                        LinkType = link.LinkType,
                        NodeFromId = link.NodeFromId,
                        NodeToId = link.NodeToId,
                    });
                }
            }


            foreach (var node in bidocGraphInfo.Nodes)
            {
                int? parent;
                int parent_id;

                if(parent_ids.TryGetValue(node.Id, out parent_id))
                {
                    parent = parent_id;
                }
                else
                {
                    parent = null;
                }


                bgi.Nodes.Add(
                    new BasicGraphInfoNode
                {
                    Description = node.Description,
                    //DocumentRelativePath = node.DocumentRelativePath,
                    Id = node.Id,
                    Name = node.Name,
                    NodeType = node.NodeType,
                    ProjectConfigId = node.ProjectConfigId,
                    //RefPath = node.RefPath,
                    ParentId = parent                        
                }
                );
            }

            return bgi;
        }
    }
}
