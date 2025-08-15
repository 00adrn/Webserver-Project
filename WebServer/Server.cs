using System.Net;
using System.Net.Sockets;
using System.Reflection;
using WebServer.Website;

namespace WebServer;

public static class Server
{
    private static HttpListener listener = new HttpListener();
    private static Router router = new Router();
    public static int maxSimultaneousConnections = 20;
    private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);
    private static int port = 8080;


    public enum ServerError
    {
        OK,
        ExpiredSession,
        NotAuthorized,
        FileNotFound,
        PageNotFound,
        ServerError,
        UnknownType
    }

    public static string? ErrorHandler(Server.ServerError error)
    {
        string? errorRoute = null;

        switch (error)
        {
            case Server.ServerError.ExpiredSession:
                errorRoute = "/ErrorPages/expiredSession.html";
                break;
            case Server.ServerError.FileNotFound:
                errorRoute = "/ErrorPages/fileNotFound.html";
                break;
            case Server.ServerError.NotAuthorized:
                errorRoute = "/ErrorPages/notAuthorized.html";
                break;
            case Server.ServerError.PageNotFound:
                errorRoute = "/ErrorPages/pageNotFound.html";
                break;
            case Server.ServerError.ServerError:
                errorRoute = "/ErrorPages/serverError.html";
                break;
            case Server.ServerError.UnknownType:
                errorRoute = "/ErrorPages/unknownType.html";
                break;
        }

        return errorRoute;
    }

    public static void Start()
    
    {
        Console.WriteLine("Starting server...");
        router.WebsitePath = GetWebsitePath();
        List<IPAddress> localhostIPs = GetLocalhostIPs();
        InitializeListener(localhostIPs);
        StartListener();
    }

    public static void AddRoute(Route route)
    {
        router.AddRoute(route);
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

        listener.Prefixes.Add($"http://localhost:{port}/");

        foreach (IPAddress ip in localhostIPs)
        {
            string address = $"http://{ip.ToString()}:{port}/";
            Console.WriteLine($"Listening on {address}");
            listener.Prefixes.Add(address);
        }
    }

    //Listens to connections on a seperate worker thread
    private static void StartListener()
    {
        listener.Start();
        Task.Run(() => RunServer(listener));
    }

    private static async Task RunServer(HttpListener listener)
    {
        while (true)
        {
            sem.WaitOne();
            await StartConnectionListener(listener);
        }
    }


    public static void Log(HttpListenerRequest request)
    {
        Console.WriteLine($"{request.HttpMethod} /{request.Url!.AbsoluteUri.AfterXthChar('/',3)}");
    }

    private static async Task StartConnectionListener(HttpListener listener)
    {
        HttpListenerContext context = await listener.GetContextAsync();
        sem.Release();
        HttpListenerRequest request = null;
        ResponsePacket responsePacket;
        try
        {
            request = context.Request;
            Log(request);
            string verb = request.HttpMethod;
            string path = request.RawUrl!;
            string parms = request.RawUrl!.After('?');
            Dictionary<string, string> kvParams = GetKeyValues(parms);
            responsePacket = router.Route(verb, path, kvParams);

            if (responsePacket.Error != ServerError.OK)
            {
                responsePacket.Redirect = ErrorHandler(responsePacket.Error);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ex.Message}\n{ex.StackTrace}");
            responsePacket = new ResponsePacket() { Redirect = ErrorHandler(Server.ServerError.ServerError) };
        }

        Respond(request, context.Response, responsePacket);
    }

    private static Dictionary<string, string> GetKeyValues(string data, Dictionary<string, string> kv = null!)
    {
        if (kv.IsNull()) { kv = new Dictionary<string, string>(); }

        if (data.Length > 0)
        {
            string[] dataList = data.Split('&');
            foreach (string keyAndValue in dataList) { kv[keyAndValue.Before('=')] = keyAndValue.After('='); }
        }


        return kv;
    }

    public static string GetWebsitePath()
    {
        string assemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string projectRoot = Path.GetFullPath(Path.Combine(assemblyDirectory, @"..\..\..\"));
        string websitePath = Path.Combine(projectRoot, "Website");
        return websitePath;
    }

    private static void Respond(HttpListenerRequest request, HttpListenerResponse response, ResponsePacket resp)
    {
        if (resp.Redirect == String.Empty || resp.Redirect == null)
        {
            response.ContentType = resp.ContentType;
            response.ContentLength64 = resp.Data!.Length;
            response.OutputStream.Write(resp.Data, 0, resp.Data.Length);
            response.ContentEncoding = resp.Encoding;
            response.StatusCode = (int)HttpStatusCode.OK;
        }
        else
        {
            response.StatusCode = (int)HttpStatusCode.Redirect;
            response.Redirect($"http://{request.Url!.Authority}{resp.Redirect}");
        }
        response.OutputStream.Close();
    }

}
