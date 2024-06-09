using System.Net;

namespace Studio.Http;

public class Request
{
    public string Url { get; set; }
    
    protected HttpListenerRequest HttpListenerRequest { get; set; }
    
    public Request(HttpListenerRequest request)
    {
        this.HttpListenerRequest = request;
        this.Url = request.Url.AbsolutePath;
    }
    
    public string? Input(string key)
    {
        return this.HttpListenerRequest.QueryString.Get(key);
    }
}