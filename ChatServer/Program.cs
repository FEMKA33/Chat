using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class ChatServer
{
    private TcpListener server;
    private List<TcpClient> clients = new List<TcpClient>();

    public ChatServer()
    {
        try
        {
            server = new TcpListener(IPAddress.Any, 12345);
            server.Start();

            Console.WriteLine("Сервер запущен. Ожидание подключений...");

            ListenForClients();
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"SocketException: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }
    }

    private async void ListenForClients()
    {
        try
        {
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                clients.Add(client);

                Console.WriteLine("Клиент подключен.");

                HandleClient(client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in ListenForClients: {ex.Message}");
        }
    }

    private async void HandleClient(TcpClient client)
    {
        try
        {
            NetworkStream stream = client.GetStream();

            byte[] buffer = new byte[1024];
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{message}");
                BroadcastMessage(message, client);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception in HandleClient: {ex.Message}");
        }
        finally
        {
            clients.Remove(client);
            client.Close();
            Console.WriteLine("Клиент отключен.");
        }
    }

    private void BroadcastMessage(string message, TcpClient sender)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);

        foreach (var client in clients)
        {
            if (client != sender)
            {
                try
                {
                    NetworkStream stream = client.GetStream();
                    stream.WriteAsync(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception in BroadcastMessage: {ex.Message}");
                }
            }
        }
    }

    static void Main()
    {
        new ChatServer();
        Console.ReadLine();
    }
}