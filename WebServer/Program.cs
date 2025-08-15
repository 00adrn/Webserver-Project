using WebServer;

Server.Start();
Server.AddRoute(new Route() { Verb = Router.POST, Path = "/redirect/about", Action = RedirectMe });
Console.ReadLine();


static string RedirectMe(Dictionary<string, string> parms) 
{
    return "/Pages/about";
}