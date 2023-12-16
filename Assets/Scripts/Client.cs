using System;
using System.IO;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Client : MonoBehaviour
{
    public InputField IPInput;
    public InputField PortInput;
    public InputField NickInput;

    private string _clientName;

    private bool _socketReady;
    private TcpClient _socket;
    private NetworkStream _stream;
    private StreamWriter _writer;
    private StreamReader _reader;

    public void ConnectToServer()
    {
        // 이미 연결되었다면 함수 무시
        if(_socketReady)
            return;
        
        // 기본 호스트 / 포트번호
        string ip = IPInput.text == "" ? "127.0.0.1" : IPInput.text;
        int port = PortInput.text == "" ? 7777 : int.Parse(PortInput.text);
        
        // 소켓 생성
        try
        {
            _socket = new TcpClient(ip, port);
            _stream = _socket.GetStream();
            _writer = new StreamWriter(_stream);
            _reader = new StreamReader(_stream);
            _socketReady = true;
        }
        catch (Exception e)
        {
            ChattingManager.Instance.ShowMessage($"Socket error : {e.Message}");
        }
    }

    public void OnSendButton(InputField sendInput)
    {
#if (UNITY_EDITOR || UNITY_STANDALONE)
        if (!Input.GetButtonDown("Submit"))
            return;

        sendInput.ActivateInputField();
#endif
        if(sendInput.text.Trim() == "")
            return;

        string message = sendInput.text;
        sendInput.text = "";
        Send(message);
    }

    private void Update()
    {
        if (_socketReady && _stream.DataAvailable)
        {
            string data = _reader.ReadLine();
            if (data != null)
            {
                OnIncommingData(data);
            }
        }
    }

    private void OnApplicationQuit()
    {
        CloseSocket();
    }

    private void OnIncommingData(string data)
    {
        if (data == "%NAME")
        {
            _clientName = NickInput.text == "" ? "Guest" + Random.Range(1000, 10000) : NickInput.text;
            Send($"&NAME|{_clientName}");
            return;
        }
        
        ChattingManager.Instance.ShowMessage(data);
    }

    private void Send(string data)
    {
        if(!_socketReady)
            return;
        
        _writer.WriteLine(data);
        _writer.Flush();
    }

    private void CloseSocket()
    {
        if(!_socketReady)
            return;
        
        _writer.Close();
        _reader.Close();
        _socket.Close();
        _socketReady = false;
    }
    
}
