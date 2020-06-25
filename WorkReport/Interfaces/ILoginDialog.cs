using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeClock.Interfaces
{
    public interface ILoginDialog
    {
        string UserName { get; set; }

        string Password { get; }

        string Server { get; set; }

        string AccessToken { get; }

        bool RememberPassword { get; }

        void HideRememberPasswordControl();

        bool? ShowDialog();
    }
}
