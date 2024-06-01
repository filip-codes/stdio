using System.Net;

namespace Studio;

public class Request
{
    public string Url { get; set; }
    
    protected HttpListenerRequest HttpListenerRequest { get; set; }
    
    public Request(HttpListenerRequest request)
    {
        this.HttpListenerRequest = request;
    }
    
    public string? Input(string key)
    {
        return this.HttpListenerRequest.QueryString.Get(key);
    }
}