using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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

    public static void Start()
    {
        List<IPAddress> localhostIPs = GetLocalhostIPs();
        HttpListener listener = InitializeListener(localhostIPs);
        Start(listener);
    }

    private static HttpListener InitializeListener(List<IPAddress> localhostIPs)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");

        //listen to the IP address as well
        localhostIPs.ForEach(ip =>
        {
            Console.WriteLine("Listening on IP " + "http://" + ip.ToString() + ":8080/");
            listener.Prefixes.Add("http://" + ip.ToString() + ":8080/");
        });

        return listener;
    }

    //Listens to connections on a seperate worker thread
    private static void Start(HttpListener listener)
    {
        listener.Start();
        Task.Run(() => RunServer(listener));
    }

    // Running in a separate thread, starts listening to connections not exceeding the max number of connections set
    private static async Task RunServer(HttpListener listener)
    {
        while (true)
        {
            sem.WaitOne();
            await StartConnectionListener(listener);
        }
    }

    //log requests
    public static void Log(HttpListenerRequest request)
    {
        Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri);
    }

    //Asynchronously listen and wait for connectiosn
    private static async Task StartConnectionListener(HttpListener listener)
    {
        //Wait for connections, return to caller while we wait
        HttpListenerContext context = await listener.GetContextAsync();

        //release the sempahore so that another listener can immediately start up
        sem.Release();

        //log the request
        Log(context.Request);
        


        string response = "<html><head><meta http-equiv='content-type' content='text/html; charset=utf-8'/></head>Hello Browser!</html>";
        byte[] encoded = Encoding.UTF8.GetBytes(response);
        context.Response.ContentLength64 = encoded.Length;
        context.Response.OutputStream.Write(encoded, 0, encoded.Length);
        context.Response.OutputStream.Close();
    }
}