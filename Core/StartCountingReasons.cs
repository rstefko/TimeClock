using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TimeClock.Core
{
    public enum StartCountingReasons
    {
        /// <summary>
        /// When counting is stopped by user, auto continue is requested
        /// </summary>
        AutoContinued,

        /// <summary>
        /// Occures when system has been unlocked
        /// </summary>
        SystemUnlocked
    }
}
