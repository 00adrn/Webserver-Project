using System;
using System.Text;

namespace WebServer;

public class Router
{
    public string WebsitePath { get; set; }
    private Dictionary<string, ExtensionInfo> extFolderMap;

    public Router()
    {
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
}

public class ResponsePacket
{
    public string Redirect { get; set; }
    public byte[] Data { get; set; }
    public string ContentType { get; set; }
    public Encoding Enconding { get; set; }
}

internal class ExtensionInfo
{
    public Func<string, string, string, ExtensionInfo, ResponsePacket> Loader { get; set; }
    public string ContentType { get; set; }
}
