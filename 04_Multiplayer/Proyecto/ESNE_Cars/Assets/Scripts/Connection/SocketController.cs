using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(OnlineManager))]
public class SocketController : MonoBehaviour
{
    private Socket socket;

    public static int players;

    public SO_ServerConfig serverConfig;
    public string serverIP  = "127.0.0.1";
    public int serverPort   = 666;

    private byte[] ibuffer  = new byte[2048];
    private byte[] obuffer  = new byte[2048];

    private OnlineManager onlineManager;

    void Awake()
    {
        serverIP        = serverConfig.serverIP;
        serverPort      = serverConfig.serverPort;

        onlineManager   = GetComponent<OnlineManager>();
    }
    void Start()
    {
        Debug.Log(serverIP);
        Debug.Log(serverPort);

        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        IPEndPoint remoteEp = new IPEndPoint(IPAddress.Parse(serverIP), serverPort);
        socket.Connect(remoteEp); // Empiezo a escuchar

        StartCoroutine(OnDataReceived());
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Send("WAAAAT");
        }
    }

    public void Send(string str)
    {
        byte[] strBuffer = Encoding.ASCII.GetBytes(str+"$");
        strBuffer.CopyTo(obuffer, 0);

        // Para ver visualmente q se está haciendo todo bien, sino no pasaría d byte a string ni viceversa
        socket.Send(obuffer, strBuffer.Length, SocketFlags.None);
    }

    private IEnumerator OnDataReceived()
    {
        while (true)
        {
            if (socket.Available > 0)
            {
                int bytesReceived = socket.Available;
                socket.Receive(ibuffer, bytesReceived, SocketFlags.None);

                onlineManager.ParseMessages(ibuffer,
                    bytesReceived);
            }            
            yield return null;
        }
    }
}
