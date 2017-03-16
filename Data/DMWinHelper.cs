using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ViData.Dict;

namespace ViData
{
    public class DMWinHelper
    {
        DMSession _session;
        DMSession _readSession;

        internal DMSession GetSession(DbConntionType type = DbConntionType.WriteRead, string dbString = null)
        {
            if (type == DbConntionType.WriteRead)
            {
                if (_session == null)
                {
                    _session = DMHelper.Instance._sessionfactory.BuildSession(type, dbString);
                }
                return _session;
            }
            if (_readSession == null)
            {
                _readSession = DMHelper.Instance._sessionfactory.BuildSession(type, dbString);
            }
            return _readSession;
        }

        public void CloseSession()
        {
            if (_session != null)
            {
                _session.Close();
            }
            if (_readSession != null)
            {
                _readSession.Close();
            }
        }
    }
}
