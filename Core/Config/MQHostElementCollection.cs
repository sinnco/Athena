using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ViCore.Config
{
    public sealed class MQHostElementCollection : ConfigurationElementCollection
    {
        public MQHostElementCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        protected override ConfigurationElement CreateNewElement()
        {
            return new MQHostElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as MQHostElement).Name;
        }

        protected override string ElementName
        {
            get
            {
                return "host";
            }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        public MQHostElement Get(string name)
        {
            return BaseGet(name) as MQHostElement;
        }

        public MQHostElement Default
        {
            get { return BaseGet("default") as MQHostElement; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }
    }
}
