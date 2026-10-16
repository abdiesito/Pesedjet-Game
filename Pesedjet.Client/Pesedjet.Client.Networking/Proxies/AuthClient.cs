using System.ServiceModel;
using Pesedjet.Server.Contracts.Interfaces;
using Pesedjet.Server.Contracts.DTOs;

namespace Pesedjet.Client.Networking.Proxies
{
    public class AuthClient
    {
        public bool Register(UserRegisterDto userData)
        {
            var binding = new NetTcpBinding(SecurityMode.None);
            var endpoint = new EndpointAddress("net.tcp://localhost:8081/AuthService");
            
            var factory = new ChannelFactory<IAuthManager>(binding, endpoint);
            IAuthManager channel = factory.CreateChannel();
    
            bool isSuccess = false;

            try
            {
                isSuccess = channel.RegisterAccount(userData);
                
                if (channel is ICommunicationObject commObj)
                {
                    commObj.Close();
                }
                factory.Close();
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine($"Error de red: {ex.Message}");
                if (channel is ICommunicationObject commObj) commObj.Abort();
                factory.Abort();
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Timeout: {ex.Message}");
                if (channel is ICommunicationObject commObj) commObj.Abort();
                factory.Abort();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                if (channel is ICommunicationObject commObj) commObj.Abort();
                factory.Abort();
            }

            return isSuccess;
        }
    }
}