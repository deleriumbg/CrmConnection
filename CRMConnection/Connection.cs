using log4net;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System;
using System.Net;
using System.Reflection;
using System.ServiceModel.Description;
using CRMConnection.Interfaces;


namespace CRMConnection
{
    class Connection : IConnection
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IOrganizationService _organizationService;

        public Connection()
        {
            try
            {
                // For Dynamics 365 Customer Engagement V9.X, set Security Protocol as TLS12
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                ClientCredentials clientCredentials = new ClientCredentials();
                clientCredentials.UserName.UserName = AppSettings.Username;
                clientCredentials.UserName.Password = AppSettings.Password;

                OrganizationServiceProxy proxy = new OrganizationServiceProxy(new Uri(AppSettings.Url), null, clientCredentials, null);
                proxy.EnableProxyTypes();

                if (string.IsNullOrWhiteSpace(AppSettings.Username) || string.IsNullOrWhiteSpace(AppSettings.Password) || string.IsNullOrWhiteSpace(AppSettings.Url))
                {
                    _log.Error($"Logging attempt with invalid credentials: Username {AppSettings.Username}, Password {AppSettings.Password}, Url {AppSettings.Url}");
                }
                else
                {
                    _log.Info($"Logging with credentials: Username {AppSettings.Username}, Password {AppSettings.Password}, Url {AppSettings.Url}");
                    _organizationService = (IOrganizationService)proxy;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception caught during connection initialization - {ex.Message}");
                _log.Error($"Exception caught during connection initialization - {ex.Message}");
            }
        }

        public IOrganizationService Connect() => _organizationService;

        public bool IsConnected => _organizationService != null;
    }
}
