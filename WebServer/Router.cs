using System.Text;

namespace WebServer;

public class Router
{
    public string WebsitePath { get; set; }
    private Dictionary<string, ExtensionInfo> extFolderMap;
    public const string POST = "post";
    public const string GET = "get";
    public const string PUT = "put";
    public const string DELETE = "delete";
    



    public Router()
    {
        WebsitePath = Server.GetWebsitePath();

        extFolderMap = new Dictionary<string, ExtensionInfo>()
        {
            {"ico", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/ico"}},
            {"png", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/png"}},
            {"jpg", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/jpg"}},
            {"gif", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/gif"}},
            {"bmp", new ExtensionInfo() {Loader=ImageLoader, ContentType="image/bmp"}},
            {"html", new ExtensionInfo() {Loader=FileLoader, ContentType="text/html"}},
            {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
            {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
            {"", new ExtensionInfo() {Loader=FileLoader, ContentType="text/html"}},
        };
    }

    private ResponsePacket ImageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
        FileStream fStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
        BinaryReader br = new BinaryReader(fStream);
        ResponsePacket ret = new ResponsePacket() { Data = br.ReadBytes((int)fStream.Length), ContentType = extInfo.ContentType };
        br.Close();
        fStream.Close();

        return ret;
    }

    private ResponsePacket FileLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
        string text = File.ReadAllText(fullPath);
        ResponsePacket ret = new ResponsePacket() { Data = Encoding.UTF8.GetBytes(text), ContentType = extInfo.ContentType, Encoding = Encoding.UTF8 };

        return ret;
    }

    public ResponsePacket Route(string verb, string path, Dictionary<string, string>? kvParams)
    {
        path = path[1..];
        string ext = path[(1+path.IndexOf('.'))..];

        ExtensionInfo extInfo;

        if (path == "")
            { path = "Pages/index.html"; ext = "html"; }

        else if (string.IsNullOrEmpty(ext))
        { path = $"Pages/{path}.html"; ext = "html"; }

        string fullPath = Path.Combine(WebsitePath, path);

        if (extFolderMap.TryGetValue(ext, out extInfo!))
        {
            if (!File.Exists(fullPath))
                return new ResponsePacket() { Error = Server.ServerError.FileNotFound };

            return extInfo.Loader!(fullPath, ext, extInfo);
        }
        else
        {
            return new ResponsePacket() { Error = Server.ServerError.UnknownType };
        }

    }
}

internal class ExtensionInfo
{
    public Func< string, string, ExtensionInfo, ResponsePacket>? Loader { get; set; }
    public string? ContentType { get; set; }
}
