using System;
using System.Net.Http;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        var client = new HttpClient();
        // Since we don't have a valid cookie, it will redirect to Login
        var response = await client.PostAsync(""http://localhost:5242/Admin/ToggleStatus?userId=2"", null);
        Console.WriteLine(response.StatusCode);
        Console.WriteLine(response.RequestMessage.RequestUri);
    }
}
