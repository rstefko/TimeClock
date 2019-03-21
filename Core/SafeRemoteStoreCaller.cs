using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

namespace TimeClock.Core
{
    /// <summary>
    /// Shows try again dialog when there is a problem with remote store method call.
    /// </summary>
    public static class SafeRemoteStoreCaller
    {
        public static bool TryLogIn(string server, string userName, string password, string version)
        {
            bool result = false;

            TryAgainProvider.Try(delegate()
            {
                result = RemoteStore.ItemStore.Instance.LogIn(server, userName, password, version);
            });

            return result;
        }
    }
}
