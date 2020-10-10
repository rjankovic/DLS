using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CD.DLS.Clients.Controls.Interfaces
{
    public interface ICloseable
    {
        event EventHandler Closing;
    }
}
