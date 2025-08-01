using System.ComponentModel.Design;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebServer;

public static class Server
{
    private static HttpListener listener = new HttpListener();
    private static Router router = new Router();
    public static int maxSimultaneousConnections = 20;
    private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

    public static void Start()
    {
        Console.WriteLine("Starting server...");
        router.WebsitePath = GetWebsitePath();
        List<IPAddress> localhostIPs = GetLocalhostIPs();
        InitializeListener(localhostIPs);
        StartListener();
    }

    private static List<IPAddress> GetLocalhostIPs()
    {
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        List<IPAddress> ret = host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();

        return ret;
    }

    private static void InitializeListener(List<IPAddress> localhostIPs)
    {
        Console.WriteLine("Initializing Listener...");

        listener.Prefixes.Add("http://localhost/");

        localhostIPs.ForEach(ip =>
        {
            string prefix = $"http://{ip.ToString()}/";
            Console.WriteLine($"Listening on {prefix}");
            listener.Prefixes.Add(prefix);
        });

        Console.WriteLine("Listener initialized!");
    }

    //Listens to connections on a seperate worker thread
    private static void StartListener()
    {
        Console.WriteLine("Starting listener...");
        listener.Start();
        Task.Run(() => RunServer(listener));
    }

    private static async Task RunServer(HttpListener listener)
    {
        Console.WriteLine("Server Started!");
        while (true)
        {
            sem.WaitOne();
            await StartConnectionListener(listener);
        }
    }


    public static void Log(HttpListenerRequest request)
    {
        Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url!.AbsoluteUri.Substring(7));
    }

    private static async Task StartConnectionListener(HttpListener listener)
    {
        HttpListenerContext context = await listener.GetContextAsync();

        sem.Release();
        HttpListenerRequest request = context.Request;

        Log(request);

        string verb = request.HttpMethod;
        string path = request.Url!.AbsoluteUri; //gives entire link : http://192.168.40.224:8080/favicon.ico
        Dictionary<string, string> kvParams = new Dictionary<string, string>();
        foreach (string? key in request.QueryString.AllKeys)
        { kvParams.Add(key!, request.QueryString[key]); }

        router.Route(verb, path, kvParams);

        GetWebsitePath();

        string response = "<html><head><meta http-equiv='content-type' content='text/html; charset=utf-8'/></head>Hello Browser!</html>";
        byte[] encoded = Encoding.UTF8.GetBytes(response);
        context.Response.ContentLength64 = encoded.Length;
        context.Response.OutputStream.Write(encoded, 0, encoded.Length);
        context.Response.OutputStream.Close();
    }

    public static string GetWebsitePath()
    {
        string websitePath = Assembly.GetExecutingAssembly().Location;
        Console.WriteLine(websitePath);

        return websitePath;
    }
}