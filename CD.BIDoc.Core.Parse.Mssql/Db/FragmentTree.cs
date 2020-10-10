using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.TransactSql.ScriptDom;

namespace CD.DLS.Parse.Mssql.Db
{
    /// <summary>
    /// A node of a tree of SQL fragments
    /// </summary>
    public class FragmentTreeNode
    {
        private List<FragmentTreeNode> _children = new List<FragmentTreeNode>();

        public TSqlFragment Fragment { get; set; }
        public List<FragmentTreeNode> Children { get { return _children; } }

        public FragmentTreeNode(TSqlFragment fragment)
        {
            this.Fragment = fragment;
        }
    }

    public class FragmentTreeBuilder
    {
        /// <summary>
        /// Builds a tree of fragments that are of intereset.
        /// </summary>
        public FragmentTreeNode BuildFragmentTree(TSqlFragment root, List<TSqlFragment> fragmentsOfInterest)
        {
            FragmentTreeNode fragmentTreeRoot = new FragmentTreeNode(root);
            List<TSqlFragment> fragmentsOfInterestByPosition = fragmentsOfInterest.OrderBy(x => x.FirstTokenIndex).ThenByDescending(x => x.LastTokenIndex).ToList();
            fragmentsOfInterestByPosition.Insert(0, root);
            Dictionary<TSqlFragment, FragmentTreeNode> nodeDictionary = new Dictionary<TSqlFragment, FragmentTreeNode>() { { root, fragmentTreeRoot } };
            for (int i = 1; i < fragmentsOfInterestByPosition.Count; i++)
            {
                var parentIdx = i - 1;
                while (!new ScriptSpan(fragmentsOfInterestByPosition[parentIdx]).Contains(new ScriptSpan(fragmentsOfInterestByPosition[i])))
                {
                    parentIdx--;
                }
                var parent = nodeDictionary[fragmentsOfInterestByPosition[parentIdx]];
                var child = new FragmentTreeNode(fragmentsOfInterestByPosition[i]);
                /*
                 can break BuildNodeFromDependencyTree() ?
                */
                /*
                if (parent.Fragment.FragmentLength == child.Fragment.FragmentLength)
                {
                    // SelectStatement -> QuerySpecification when only one query spec. exists => redundant level
                    nodeDictionary.Add(child.Fragment, parent);
                    continue;
                }
                */
                nodeDictionary.Add(child.Fragment, child);
                parent.Children.Add(child);
            }

            return fragmentTreeRoot;
        }
    }
}
