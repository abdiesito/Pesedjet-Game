using System.ServiceModel;
using Pesedjet.Server.Contracts.Callbacks;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Server.Contracts.Callbacks
{
    public interface ILobbyCallback
    {
        [OperationContract(IsOneWay = true)]
        void OnLobbyStateChanged(LobbyChangeEvent changeEvent);
    }
}