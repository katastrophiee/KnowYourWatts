using KnowYourWatts.Server.Interfaces;
using System.Net;
using System.Net.Sockets;

namespace KnowYourWatts.Server;

public sealed class Server(
    IConnectionHandler connectionHandler,
    IPAddress ipAddress,
    IPEndPoint localEndPoint)
{
    private readonly IConnectionHandler _connectionHandler = connectionHandler;
    private readonly IPAddress _ipAddress = ipAddress;
    private readonly bool _runServer = true;

    public void Start()
    {
        try
        {
            var listener = new Socket(_ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(localEndPoint);
            listener.Listen(30);
            Console.WriteLine($"Server started at {localEndPoint.Address}:{localEndPoint.Port}");

            while (_runServer)
            {
                if (listener.Poll(1000, SelectMode.SelectRead))
                {
                    var handler = listener.Accept();
                    ThreadPool.QueueUserWorkItem(state => _connectionHandler.HandleConnection(handler));
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occured when starting the server: " + ex.ToString());
        }
    }
}

    // create name.database proj
    // add interfaces
    // make db interface
    // add db stub class, will contain get current meter reading, old meter, stuff from a db
    // instead of storing from a db will need classes
    // make domain models (a db table) then in db stub, initialise objects and store everything in a list
    // MPAN - meter point access number, every electric meter has one that is unique, 9-13 digits
    // MPRM - for gas
    // list of customers, against each customer we'd have the MPAN, with each request we'd send it so we'd know who to assign the meter reading to
    // people spoofs MPAN so other people pay for your electricity
    // MPAN can be known publicly as meters are on outside of houses so we can see it there, so we gotta send something secret
    // can send a client secret key stored on client also, its what we hash with on server then compare the hash with

    // Got MPAN, send MPAN to server and send a hashed version of the MPAN
    // When we hash the MPAN, we wanna salt it with a 'secret key' needs to be the same on the client and the server
    // the secret key will be the same in client and sevrer for all MPANs

    // MPAN is like username, hashed MPAN is like the password as they dont know
    // public and private keys
    // if blockchain was good for utilities then they'd using it, they are not

    // people are on different tarrifs - normally send readings every 30 minutes
    // then it sends back the kw usage and the total price, kw is so much pence per second
    // must keep a rolling total of the reading

    // implausable meter read, when you give a manual reading (tell someone the reading), it looks at historical data to check you're not lying
    // the meters them selves keep a rolling number of the electricity running through them, then send the data off using 2G
    // the server side keeps the record
    // it rolls through the nnumbers with a clicker, that is the db

    // not unit testing stop and start server
    // we'll be testing the servermathlogic as it is the business logic, might not need and interface
    // need db interface that we will mock

    // an interafce is like a contract, everything in the interface will have these things
    // might have more but confirmed has the things in the interface
    // when you create a mock, you're saying make a new instance of the member repo which is difference to the class
    // we tell it what to return

    // add ependency injestion

    //try and reconnect
    // create editor config file and run it