using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

var senderMessages = new List<Message>();
var sentMessages = new List<Message>();

var localAddress = IPAddress.Parse("127.0.0.1");

Console.Write("username: ");
var username = Console.ReadLine();
Console.Write("incoming messages port: ");
if (!int.TryParse(Console.ReadLine(), out var localPort)) return;
Console.Write("sending messages port: ");
if (!int.TryParse(Console.ReadLine(), out var remotePort)) return;

Task.Run(ReceiveMessageAsync);
await SendMessageAsync();

async Task SendMessageAsync()
{
    using var sender = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    Console.WriteLine("Enter message: ");
    while (true)
    {
        var message = Console.ReadLine();
        //Console.Clear();
        
        if (string.IsNullOrWhiteSpace(message)) break;

        var realMessage = new Message(username, message, DateTime.Now);
        sentMessages.Add(realMessage);
        
        var data = JsonSerializer.SerializeToUtf8Bytes(realMessage);
        Console.Clear();
        var temp = new List<Message>();
        temp.AddRange(senderMessages);
        temp.AddRange(sentMessages);

        foreach (var messageTmp in temp.OrderBy(x => x.Time))
        {
            Console.WriteLine($"{messageTmp.SenderName}: {messageTmp.Text} -- {messageTmp.Time}");
        }
        
        await sender.SendToAsync(data, SocketFlags.None, new IPEndPoint(localAddress, remotePort));
    }
}


async Task ReceiveMessageAsync()
{
    using var receiver = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

    receiver.Bind(new IPEndPoint(localAddress, localPort));
    var data = new byte[65535];
    while (true)
    {
        var result = await receiver.ReceiveFromAsync(data, SocketFlags.None, new IPEndPoint(IPAddress.Any, 0));
        var message = Encoding.UTF8.GetString(data, 0, result.ReceivedBytes);
        var realdMessage = JsonSerializer.Deserialize<Message>(message);
       
        senderMessages.Add(realdMessage);

        Console.Clear();
        var temp = new List<Message>();
        temp.AddRange(senderMessages);
        temp.AddRange(sentMessages);

        foreach (var messageTmp in temp.OrderBy(x => x.Time))
        {
            Console.WriteLine($"{messageTmp.SenderName}: {messageTmp.Text} -- {messageTmp.Time}");
        }
    }
}


public class Message
{
    public string SenderName { get; }
    public string Text { get; }
    public DateTime Time { get; }

    public Message(string SenderName, string Text, DateTime Time)
    {
        this.SenderName = SenderName;
        this.Text = Text;
        this.Time = Time;
    }
}