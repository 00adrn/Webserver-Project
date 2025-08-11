using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Clifton.Extensions;

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
            {"html", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
            {"css", new ExtensionInfo() {Loader=FileLoader, ContentType="text/css"}},
            {"js", new ExtensionInfo() {Loader=FileLoader, ContentType="text/javascript"}},
            {"", new ExtensionInfo() {Loader=PageLoader, ContentType="text/html"}},
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

    private ResponsePacket PageLoader(string fullPath, string ext, ExtensionInfo extInfo)
    {
        return FileLoader(fullPath, ext, extInfo);
    }

    public ResponsePacket Route(string verb, string path, Dictionary<string, string> kvParams)
    {
        string ext = path.RightOf('.', 1);
        ExtensionInfo extInfo;

        if (path == "/") { path = "Pages\\index.html"; ext = "html"; }
        else if (string.IsNullOrEmpty(ext)) { path = $"Pages\\{path.RightOf('/', 1)}.html"; ext = "html"; }
        else { path = path.RightOf('/', 1); }

        string fullPath = Path.Combine(WebsitePath, path);

        if (!extFolderMap.TryGetValue(ext, out extInfo!))
        {
            Console.WriteLine("Unsupported Extension");
            return new ResponsePacket()
            {
                Data = Encoding.UTF8.GetBytes("<h1>415 Error: Unsupported Extension Type"),
                ContentType = "text/html",
                Encoding = Encoding.UTF8
            };
        }
        ;

        if (!File.Exists(fullPath))
        {
            Console.WriteLine("Error 404: Item not found");
            return new ResponsePacket()
            {
                Data = Encoding.UTF8.GetBytes("<h1>Error 404: Page not found"),
                ContentType = "text/html",
                Encoding = Encoding.UTF8
            };
        }

        return extInfo.Loader!(fullPath, ext, extInfo);
    }
}

public class ResponsePacket
{
    public string? Redirect { get; set; }
    public byte[]? Data { get; set; }
    public string? ContentType { get; set; }
    public Encoding? Encoding { get; set; }
}

internal class ExtensionInfo
{
    public Func< string, string, ExtensionInfo, ResponsePacket>? Loader { get; set; }
    public string? ContentType { get; set; }
}
