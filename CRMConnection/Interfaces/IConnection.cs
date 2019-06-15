using Microsoft.Xrm.Sdk;

namespace CRMConnection.Interfaces
{
    public interface IConnection
    {
        IOrganizationService Connect();

        bool IsConnected { get; }
    }
}
