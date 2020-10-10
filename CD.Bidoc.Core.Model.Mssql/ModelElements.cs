using CD.DLS.DAL.Configuration;
using CD.DLS.Model.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Model.Mssql
{
    public class DefinitionSegment
    {
        public int PositionFrom { get; set; }
        public int Length { get; set; }
        public MssqlModelElement DefinedElement { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ModelLinkAttribute : Attribute
    {
        public string LinkType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ModelLinkCollectionAttribute : Attribute
    {
        public string LinkType { get; set; }
    }

    [DataContract]
    public abstract class MssqlModelElement : IModelElement
    {
        private readonly string _caption;
        private RefPath _refPath;

        public RefPath RefPath { get { return _refPath; } }
        public string Caption { get { return _caption; } }
        public string Definition { get; set; }
        public MssqlModelElement Parent { get; set; }

        protected List<MssqlModelElement> _children = new List<MssqlModelElement>();
        public IEnumerable<MssqlModelElement> Children { get { return _children; } }

        [ModelLink]
        public MssqlModelElement Reference { get; set; }

        public int Id { get; set; }

        IModelElement IModelElement.Parent
        {
            get
            {
                return Parent;
            }
        }

        IEnumerable<IModelElement> IModelElement.Children
        {
            get
            {
                return Children;
            }
        }

        protected MssqlModelElement(RefPath refPath, string caption)
        {
            _refPath = refPath;
            _caption = caption;
            //if (RefPath.Path ==
            //    "IntegrationServices[@Name='FSCZPRCT0041']/Catalog[@Name='SSISDB']/CatalogFolder[@Name='NDWH_SSIS']/ProjectInfo[@Name='NDWH_SSIS']/PackageInfo[@Name='ExtractAS400.dtsx']/Executable[@Name='ExtractAll']/Executable[@Name='ExctractAS400_ICALBT']/ParameterAssignment[@Name='JobRunID']"
            //    //"SSRSServer[@Name='SSRS']/Folder[@Name='/']/Folder[@Name='Brand Management']/Folder[@Name='2 - MONTHLY REPORTS']/Report[@Name='ALL BRANDS']/DataSet[@Name='ds_management_overview']/Field[@Name='sda_vozy']"
            //    )
            //{

            //}
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}; {2}", GetType().Name, _caption, _refPath.Path);
        }

        public virtual void AddChild(MssqlModelElement child)
        {
            _children.Add(child);
            //if(child.RefPath.Path == "SSRSServer[@Name='SSRS']/Folder[@Name='/']/Folder[@Name='Reports']/Report[@Name='08 - Bonuses']")
            //{

            //}
            if (child == null)
            {
                throw new Exception();
            }
        }

        public virtual void RemoveChild(MssqlModelElement child)
        {
            _children.Remove(child);
        }

        public virtual void SaveLinks<TTarget>(ModelConversion<MssqlModelElement, TTarget> conversion, IReflectionHelper reflectionHelper)
        {
            
            foreach(var property in this.GetType().GetProperties().Where(p=>Attribute.IsDefined(p, typeof(ModelLinkAttribute)))){
                var at = (ModelLinkAttribute) Attribute.GetCustomAttribute(property, typeof(ModelLinkAttribute));

                var propertyType = property.GetType();
                var linkType = at.LinkType ?? property.Name;
                if (typeof(IEnumerable<MssqlModelElement>).IsAssignableFrom(propertyType))
                {
                    conversion.LinkBatch(this, (IEnumerable<MssqlModelElement>)property.GetValue(this), linkType);
                }
                else
                {
                    var linkedElement = (MssqlModelElement)property.GetValue(this);
                    try
                    {
                        if (linkedElement != null)
                            conversion.Link(this, linkedElement, linkType);
                    }
                    catch
                    {
                        ConfigManager.Log.Error("Failed to add link from " + this.RefPath.Path + " to " + linkedElement.RefPath.Path);
                        if (this.RefPath.Path.StartsWith("SSRS") && linkedElement.RefPath.Path.StartsWith("SSRS"))
                        {
                            continue;
                        }
                       // throw;
                    }
                }
            }
        }

        public virtual void AddLink(MssqlModelElement target, string type, IReflectionHelper reflectionHelper)
        {

            foreach (var property in this.GetType().GetProperties().Where(p => Attribute.IsDefined(p, typeof(ModelLinkAttribute))))
            {
                var at = (ModelLinkAttribute)Attribute.GetCustomAttribute(property, typeof(ModelLinkAttribute));

                if ((at.LinkType ?? property.Name) == type)
                {
                    var propertyType = property.GetType();
                    if (typeof(IEnumerable<MssqlModelElement>).IsAssignableFrom(propertyType))
                    {
                        object listValue = property.GetValue(this);
                        var addMethod = propertyType.GetMethod("Add");
                        addMethod.Invoke(listValue, new object[] { target });
                    }
                    else
                    {
                        property.SetValue(this, target);
                    }
                }
            }

        }
        protected IEnumerable<NodeType> ChildrenOfType<NodeType>()
            where NodeType : MssqlModelElement
        {
            return _children.Where(c => c is NodeType).Cast<NodeType>();
        }
        public IEnumerable<NodeType> DescendantsOfType<NodeType>()
            where NodeType : MssqlModelElement
        {
            return ChildrenOfType<NodeType>().Union(Children.SelectMany(c => c.DescendantsOfType<NodeType>()));
        }
        protected NodeType ChildOfTypeByCaption<NodeType>(string caption)
            where NodeType : MssqlModelElement
        {
            return (NodeType) _children.Where(c => c is NodeType && c.Caption == caption).FirstOrDefault();
        }

        protected NodeType ChildOfTypeByCaption<NodeType>(string caption, StringComparer comparer)
    where NodeType : MssqlModelElement
        {
            return (NodeType)_children.Where(c => c is NodeType && comparer.Compare(c.Caption, caption) == 0).FirstOrDefault();
        }

        /// <summary>
        /// Fix RefPaths after a new node has been inserted in the hierarchy between this node and its parent
        /// </summary>
        /// <param name="newParent"></param>
        public void AdjustRefPathUnderNewParentOneLevel(MssqlModelElement newParent)
        {
            _refPath = newParent.RefPath.AddRefIdSuffix(this.RefPath.RefId);
            foreach (var child in Children)
            {
                child.AdjustRefPathUnderNewParentOneLevel(this);
            }
        }
    }

    
}
