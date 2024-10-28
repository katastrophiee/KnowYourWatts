using System.Net.Sockets;

namespace KnowYourWatts.Server.Interfaces;

public interface IConnectionHandler
{
    public void HandleConnection(Socket handler);
}
