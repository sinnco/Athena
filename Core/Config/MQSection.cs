using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Configuration;

namespace ViCore.Config
{
    public class MQSection : ConfigurationSection
    {
        static MQSection()
        {
            _properties.Add(_enable);
            _properties.Add(_hosts);
        }

        static readonly ConfigurationProperty _enable = new ConfigurationProperty("enable", typeof(bool), true);
        static readonly ConfigurationProperty _hosts = new ConfigurationProperty(null, typeof(MQHostElementCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public bool Enable
        {
            get { return Convert.ToBoolean(base[_enable]); }
        }

        [ConfigurationCollection(typeof(MQHostElement), AddItemName = "host", CollectionType = ConfigurationElementCollectionType.BasicMap)]
        public MQHostElementCollection Hosts
        {
            get
            {
                return base[_hosts] as MQHostElementCollection;
            }
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
