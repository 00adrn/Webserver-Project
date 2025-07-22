using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace WebServer;

public static class Server
{
    private static HttpListener listener;
    public static int maxSimultaneousConnections = 20;
    private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

    /// returns a list of ip addresses that are on the localhost network

    private static List<IPAddress> GetLocalhostIPs()
    {
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

        return ret;
    }

    private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
    {
        HttpListener listener = new HttpListener();

        //listen to the IP adress as well
        localhostIPs.ForEach(ip =>
        {
            Console.WriteLine("Listening on IP" + "http://" + ip.ToString() + "/");
            listener.Prefixes.Add("http://" + ip.ToString() + "/");
        });

        return listener;
    }
}