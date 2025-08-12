using System.Text;

namespace WebServer;

public class ResponsePacket
{
    public string? Redirect { get; set; }
    public byte[]? Data { get; set; }
    public string? ContentType { get; set; }
    public Encoding? Encoding { get; set; }
    public Server.ServerError Error = Server.ServerError.OK;
}
