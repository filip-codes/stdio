using System.Net;
using System.Text;

namespace Studio.Http;

public class Response
{
    private HttpListenerResponse _response { get; set; }
    
    public Response(HttpListenerResponse response)
    {
        this._response = response;
    }
    
    public void Redirect(string path)
    {
        this._response.Redirect(path);
    }

    public Response Handle(string content)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        
        this._response.ContentLength64 = buffer.Length;
        this._response.OutputStream.Write(buffer, 0, buffer.Length);
        this._response.OutputStream.Close();

        return this;
    }
}