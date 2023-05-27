using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snobal.Library.Scopes
{
    /// <summary>
    /// Instantiate one of these and use the constructor to imprint a tenant url and rest client on scopes that the factory produces.
    /// </summary>
    public class ScopeFactory
    {
        string tenantURL;
        IRestClient restClient;

        public ScopeFactory(string TenantURL, IRestClient RestClient)
        {
            tenantURL = TenantURL;
            restClient = RestClient;
        }

        public void SetRestclient(IRestClient RestClient)
        {
            restClient = RestClient;
        }

        public void Configure(ScopeTemplate scope)
        {
            scope.tenantUrl = tenantURL;
            scope.restClient = restClient;
        }

        public Login BuildLoginScope(string DeviceToken)
        {
            Logger.Log("Creating new login scope.");
            var login = new Login(DeviceToken);
            Configure(login);
            return login;
        }

    }
}
