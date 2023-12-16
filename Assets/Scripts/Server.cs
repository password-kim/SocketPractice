using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

public class Server : MonoBehaviour
{
    public InputField PortInput;

    private List<ServerClient> _clients;
    private List<ServerClient> _disconnectList;
    
    private TcpListener _server;
    private bool _serverStarted;

    public void ServerCreate()
    {
        _clients = new List<ServerClient>();
        _disconnectList = new List<ServerClient>();

        try
        {
            int port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);
            _server = new TcpListener(IPAddress.Any, port);
            _server.Start();

            StartListening();
            _serverStarted = true;
            ChattingManager.Instance.ShowMessage($"서버가 {port}에서 시작되었습니다.");
        }
        catch (Exception e)
        {
            ChattingManager.Instance.ShowMessage($"Socket error: {e.Message}");
        }
    }
    
    void Update()
    {
        if(!_serverStarted)
            return;

        foreach (ServerClient client in _clients)
        {
            // 클라이언트가 여전히 연결되있나?
            if (!IsConnected(client.tcp))
            {
                client.tcp.Close();
                _disconnectList.Add(client);
            }
            else // 클라이언트로부터 체크 메시지를 받는다.
            {
                NetworkStream stream = client.tcp.GetStream();
                if (stream.DataAvailable)
                {
                    string data = new StreamReader(stream, true).ReadLine();
                    if (data != null)
                    {
                        OnIncommingData(client, data);
                    }
                }
            }
        }

        for (int i = 0; i < _disconnectList.Count - 1; ++i)
        {
            BroadCast($"{_disconnectList[i].clientName} 연결이 끊어졌습니다.", _clients);
            
            _clients.Remove(_disconnectList[i]);
            _disconnectList.RemoveAt(i);
        }
    }

    private bool IsConnected(TcpClient client)
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    return client.Client.Receive(new byte[1], SocketFlags.Peek) != 0;
                }

                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception e)
        {
            return false;
        }
    }

    private void StartListening()
    {
        _server.BeginAcceptTcpClient(AcceptTcpClient, _server);
    }

    private void AcceptTcpClient(IAsyncResult ar)
    {
        TcpListener listener = (TcpListener)ar.AsyncState;
        _clients.Add(new ServerClient(listener.EndAcceptTcpClient(ar)));
        StartListening();
        
        // 메시지를 연결된 모두에게 보냄
        BroadCast("%NAME", new List<ServerClient>() {_clients[^1]});
    }

    private void OnIncommingData(ServerClient serverClient, string data)
    {
        if (data.Contains("&NAME"))
        {
            serverClient.clientName = data.Split('|')[1];
            BroadCast($"{serverClient.clientName}이 연결되었습니다.", _clients);
            return;
        }
        
        BroadCast($"{serverClient.clientName} : {data}", _clients);
    }

    private void BroadCast(string data, List<ServerClient> serverClients)
    {
        foreach (ServerClient client in serverClients)
        {
            try
            {
                StreamWriter writer = new StreamWriter(client.tcp.GetStream());
                writer.WriteLine(data);
                writer.Flush();
            }
            catch (Exception e)
            {
                ChattingManager.Instance.ShowMessage($"Write error : {e.Message}를 클라이언트에게 {client.clientName}");
            }
        }
    }
}

public class ServerClient
{
    public TcpClient tcp;
    public string clientName;

    public ServerClient(TcpClient clientSocket)
    {
        clientName = "Guest";
        tcp = clientSocket;
    }
}