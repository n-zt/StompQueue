using WebSocketSharp;

string host = "";
int port = 15673; // SSL port used for the STOMP protocol over WebSocket in RabbitMQ."
string username = ""; // RabbitMQ username
string password = ""; // RabbitMQ password

using (var ws = new WebSocket($"wss://{host}:{port}/ws"))
{
    ws.SslConfiguration.EnabledSslProtocols = System.Security.Authentication.SslProtocols.Tls12;
    ws.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; // Sertifika doğrulama işlemini devre dışı bırakır (DİKKAT: Geliştirme amaçlıdır)

    ws.OnOpen += (sender, e) =>
    {
        Console.WriteLine("WebSocket opened.");
        string connectFrame = $"CONNECT\naccept-version:1.2\nhost:/\nlogin:{username}\npasscode:{password}\n\n\x00";
        ws.Send(connectFrame);
        Console.WriteLine("CONNECT frame sent.");
    };

    ws.OnMessage += (sender, e) =>
    {
        Console.WriteLine($"Received message: {e.Data}");
        if (e.Data.Contains("CONNECTED"))
        {
            Console.WriteLine("Connected to RabbitMQ.");

            // Mesaj gönderme
            string message = "Hello, RabbitMQ!";
            string sendFrame = $"SEND\ndestination:/queue/my_queue\ncontent-type:text/plain\n\n{message}\x00";
            ws.Send(sendFrame);
            Console.WriteLine("Message sent to the queue.");
        }
        else if (e.Data.Contains("ERROR"))
        {
            Console.WriteLine("Received ERROR frame: " + e.Data);
        }
    };

    ws.OnClose += (sender, e) =>
    {
        Console.WriteLine("WebSocket closed.");
    };

    ws.OnError += (sender, e) =>
    {
        Console.WriteLine($"WebSocket error: {e.Message}");
    };

    ws.Connect();

    Console.ReadKey(true);

    ws.Close();
}
