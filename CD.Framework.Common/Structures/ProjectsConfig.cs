using CD.DLS.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace CD.DLS.Common.Structures
{
    public class ProjectsConfig : ConfigurationSection
    {
        [ConfigurationProperty("", IsRequired = true, IsDefaultCollection = true)]
        public ProjectConfigCollection Instances
        {
            get { return (ProjectConfigCollection)this[""]; }
            set { this[""] = value; }
        }
    }
    public class ProjectConfigCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ProjectConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            //set to whatever Element Property you want to use for a key
            return ((ProjectConfigElement)element).Name;
        }
    }

    public class ProjectConfigElement : ConfigurationElement
    {
        //Make sure to set IsKey=true for property exposed as the GetElementKey above
        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        ProjectConfig _config;
        public ProjectConfig Config
        {
            get { return _config; }
        }

        protected override bool OnDeserializeUnrecognizedElement(string elementName, System.Xml.XmlReader reader)
        {
            if (elementName == "ProjectConfig")
            {
                //_config = (XElement)XElement.ReadFrom(reader);
                XmlSerializer serializer = new XmlSerializer(typeof(ProjectConfig));
                _config = (ProjectConfig)(serializer.Deserialize(reader));
                return true;
            }
            else
                return base.OnDeserializeUnrecognizedElement(elementName, reader);
        }
    }
}
