using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Snobal.Utilities;

namespace Snobal.Library
{
    public interface IRestClient
    {
        void Initialize();
        void Shutdown();
        Snobal.Utilities.Networking.WebResponse Get(string url);
        Snobal.Utilities.Networking.WebResponse Post(string url, string body);
        Snobal.Utilities.Networking.WebResponse Post(string url, string body, IEnumerable<Snobal.Utilities.Networking.Header> headers);
        Snobal.Utilities.Networking.ContentHeaderData Head(string url);
    }
}
