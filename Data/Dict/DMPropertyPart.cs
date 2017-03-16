using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OracleClient;

namespace ViData.Dict
{
    public class DMPropertyPart
    {
        internal string TypeName { get; set; }
        internal string ColumnName { get; set; }
        PCDictionary _pcDict;
        PCDictionary PcDict
        {
            get
            {
                if (_pcDict == null)
                {
                    _pcDict = DMClassMap.PFCDictionary[TypeName];
                }
                return _pcDict;
            }
        }

        public DMPropertyPart Identity()
        {
            PcDict.IdIdentity = true;
            return this;
        }

        public DMPropertyPart Identity(string seqName)
        {
            PcDict.IdIdentity = true;
            PcDict.SeqName = seqName;
            return this;
        }

        public DMPropertyPart OracleParameter(OracleParameter para)
        {
            if (string.IsNullOrEmpty(para.ParameterName))
            {
                para.ParameterName = "p_" + ColumnName;
            }
            if (!PcDict.OraParaDictionary.ContainsKey(ColumnName))
            {
                PcDict.OraParaDictionary.Add(ColumnName, para);
            }
            else
            {
                PcDict.OraParaDictionary[ColumnName] = para;
            }
            return this;
        }

        public DMPropertyPart OracleType(OracleType type)
        {
            OracleParameter op;
            if (PcDict.OraParaDictionary.ContainsKey(ColumnName))
            {
                op = PcDict.GetOraParameter(ColumnName);
                op.OracleType = type;
            }
            else
            {
                op = new OracleParameter();
                op.ParameterName = "p_" + ColumnName;
                op.OracleType = type;
                op.SourceColumn = ColumnName;
                PcDict.OraParaDictionary.Add(ColumnName, op);
            }
            return this;
        }
    }
}
