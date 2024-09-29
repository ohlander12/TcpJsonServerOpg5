using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("TCP Server Opgave 5:");

        TcpListener listener = new TcpListener(IPAddress.Any, 8080);
        listener.Start();

        while (true)
        {
            TcpClient socket = listener.AcceptTcpClient();
            IPEndPoint clientEndPoint = socket.Client.RemoteEndPoint as IPEndPoint;
            Console.WriteLine("Client connected: " + clientEndPoint.Address);

            Task.Run(() => HandleClient(socket));
        }
    }

    static void HandleClient(TcpClient socket)
    {
        using NetworkStream ns = socket.GetStream();
        using StreamReader reader = new StreamReader(ns);
        using StreamWriter writer = new StreamWriter(ns) { AutoFlush = true };

        while (socket.Connected)
        {
            try
            {
                string? jsonInput = reader.ReadLine();
                if (jsonInput == null)
                {
                    Console.WriteLine("Client disconnected.");
                    break;
                }

                Console.WriteLine("Received JSON: " + jsonInput);
                var request = JsonSerializer.Deserialize<Request>(jsonInput);

                if (request == null || string.IsNullOrEmpty(request.Method))
                {
                    writer.WriteLine(JsonSerializer.Serialize(new { Error = "Invalid request format." }));
                    continue;
                }

                if (request.Method == "Random")
                {
                    Random random = new Random();
                    int randomNumber = random.Next(request.Tal1, request.Tal2 + 1);
                    writer.WriteLine(JsonSerializer.Serialize(new { Result = randomNumber }));
                }
                else if (request.Method == "Add")
                {
                    int sum = request.Tal1 + request.Tal2;
                    writer.WriteLine(JsonSerializer.Serialize(new { Result = sum }));
                }
                else if (request.Method == "Subtract")
                {
                    int difference = request.Tal1 - request.Tal2;
                    writer.WriteLine(JsonSerializer.Serialize(new { Result = difference }));
                }
                else
                {
                    writer.WriteLine(JsonSerializer.Serialize(new { Error = "Unknown method." }));
                }
            }
            catch (JsonException ex)
            {
                writer.WriteLine(JsonSerializer.Serialize(new { Error = "Fail" + ex.Message }));
            }
            catch (Exception ex)
            {
                writer.WriteLine(JsonSerializer.Serialize(new { Error = "Error" + ex.Message }));
            }
        }
    }

    public class Request
    {
        public string? Method { get; set; }
        public int Tal1 { get; set; }
        public int Tal2 { get; set; }
    }
}
