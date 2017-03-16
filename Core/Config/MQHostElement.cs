using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

namespace ViCore.Config
{
    public class MQHostElement : ConfigurationElement
    {
        static MQHostElement()
        {
            _properties.Add(_name);
            _properties.Add(_ip);
            _properties.Add(_path);
            _properties.Add(_acknowledgeHost);
            _properties.Add(_acknowledgeType);
            _properties.Add(_useJournal);
            _properties.Add(_useDeadLetter);
        }

        static readonly ConfigurationProperty _name = new ConfigurationProperty("name", typeof(string));
        static readonly ConfigurationProperty _ip = new ConfigurationProperty("ip", typeof(string), ".");
        static readonly ConfigurationProperty _path = new ConfigurationProperty("path", typeof(string), null);
        static readonly ConfigurationProperty _acknowledgeHost = new ConfigurationProperty("acknowledgeHost", typeof(string), null);
        static readonly ConfigurationProperty _acknowledgeType = new ConfigurationProperty("acknowledgeType", typeof(int), 0);
        static readonly ConfigurationProperty _useJournal = new ConfigurationProperty("useJournal", typeof(bool), false);
        static readonly ConfigurationProperty _useDeadLetter = new ConfigurationProperty("useDeadLetter", typeof(bool), false);
        static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();

        public string Name
        {
            get
            {
                return (string)base[_name];
            }
        }

        public string Ip
        {
            get
            {
                return (string)base[_ip];
            }
        }

        public string Path
        {
            get
            {
                return (string)base[_path];
            }
        }

        public string AcknowledgeHost
        {
            get
            {
                return (string)base[_acknowledgeHost];
            }
        }

        public int AcknowledgeType
        {
            get
            {
                return Convert.ToInt32(base[_acknowledgeType]);
            }
        }

        public bool UseJournal
        {
            get
            {
                return Convert.ToBoolean(base[_useJournal]);
            }
        }

        public bool UseDeadLetter
        {
            get
            {
                return Convert.ToBoolean(base[_useDeadLetter]);
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
