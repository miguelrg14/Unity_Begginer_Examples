using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerData", menuName = "ScriptableObjects/ServerConfig")]
public class SO_ServerConfig : ScriptableObject
{
    public string   serverIP;
    public int      serverPort;
    public string   nickname;

    public void Init(string server_ip, int server_port, string nick)
    {
        this.serverIP   = server_ip;
        this.serverPort = server_port;
        this.nickname   = nick;
    }
    public static SO_ServerConfig CreateInstance(string server_ip, int server_port, string nick)
    {
        var data = ScriptableObject.CreateInstance<SO_ServerConfig>();
        data.Init(server_ip, server_port, nick);
        return data;
    }
}
