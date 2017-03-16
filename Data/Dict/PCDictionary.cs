using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data.OracleClient;

namespace ViData.Dict
{
    public class PCDictionary 
    {
        public PCDictionary()
        {
            ObjectDictionary = new Dictionary<string, string>();
            OraParaDictionary = new Dictionary<string, OracleParameter>();
        }

        public IDictionary<string, OracleParameter> OraParaDictionary { get; set; }
        public IDictionary<string, string> ObjectDictionary { get; set; }
        public string TableName { get; set; }
        public string IdName { get; set; }
        public bool IdIdentity { get; set; }
        public string SeqName { get; set; }

        public string GetColumn(object property)
        {
            string proName = property.ToString().ToLower();
            if (ObjectDictionary.ContainsKey(proName))
            {
                return ObjectDictionary[proName];
            }
            return null;
        }

        public OracleParameter GetOraParameter(object property)
        {
            string proName = property.ToString().ToLower();
            if (OraParaDictionary.ContainsKey(proName))
            {
                return OraParaDictionary[proName];
            }
            return null;
        }
    }
}
