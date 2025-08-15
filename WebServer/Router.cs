using System.Text;
using WebServer.Website;

namespace WebServer;

public class Router
{
    public string WebsitePath { get; set; }
    private Dictionary<string, ExtensionInfo> extFolderMap;
    private List<Route> routes;
    public const string POST = "post";
    public const string GET = "get";
    public const string PUT = "put";
    public const string DELETE = "delete";





    public Router()
    {
        WebsitePath = Server.GetWebsitePath();
        routes = new List<Route>();

        extFolderMap = new Dictionary<string, ExtensionInfo>()
        {
            {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
            {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
            {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
            {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
            {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
            {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
            {"css", new ExtensionInfo() {Loader=PageLoader, ContentType="text/css"}},
            {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
            {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
        };
    }

    public void AddRoute(Route route)
    {
        routes.Add(route);
    }

    private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
        if (!File.Exists(fullPath))
            return new ResponsePacket() { Error = Server.ServerError.FileNotFound };
        FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fStream);
        ResponsePacket ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
        br.Close();
        fStream.Close();

        return ret;
    }

    private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
        if (!File.Exists(fullPath))
            return new ResponsePacket() { Error = Server.ServerError.FileNotFound };
        string text = File.ReadAllText(fullPath);
        ResponsePacket ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

        return ret;
    }

    private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
        if (fullPath == WebsitePath)
        {
            fullPath += "\\Pages\\index.html";
        }
        else if (ext == "")
            fullPath += ".html";

        if (!File.Exists(fullPath))
            return new ResponsePacket() { Error = Server.ServerError.FileNotFound };

        string text = File.ReadAllText(fullPath);
        ResponsePacket ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

        return ret;
    }

    public ResponsePacket Route(string verb, string path, Dictionary<string, string>? kvParams)
    {
        string ext = path.AfterFinal('.');
        path = path[1..];
        ExtensionInfo extInfo;
        ResponsePacket? responsePacket;
        verb = verb.ToLower();

        if (extFolderMap.TryGetValue(ext, out extInfo!))
        {
            string fullPath = Path.Combine(WebsitePath, path);
            Route? route = routes.SingleOrDefault(route => verb == route.Verb.ToLower() && path == route.Path);

            if (route != null)
            {
                string redirect = route.Action(kvParams!);

                if (redirect == String.Empty || redirect == null)
                    responsePacket = extInfo.Loader!(fullPath, ext, extInfo); //This would reply with the default content loader
                else
                    responsePacket = new ResponsePacket() { Redirect = redirect };
            }
            else
            {
                return extInfo.Loader!(fullPath, ext, extInfo);
            }
        }
        else
        {
            responsePacket = new ResponsePacket() { Error = Server.ServerError.UnknownType };
        }
        return responsePacket;
    }
}


internal class ExtensionInfo
{
    public Func<string, string, ExtensionInfo, ResponsePacket>? Loader { get; set; }
    public string? ContentType { get; set; }
}
