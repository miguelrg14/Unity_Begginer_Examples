using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ServerConfig : MonoBehaviour
{
    [Header("Scriptable Objects")]
    public SO_ServerConfig serverConfig;
    public SO_ServerConfig serverConfig_default;

    [Header("Canvas - InputFields")]
    [SerializeField] TMP_InputField serverIP_InputField;
    [SerializeField] TMP_InputField serverPort_InputField;
    [SerializeField] TMP_InputField nickname_InputField;

    void Start()
    {
        serverIP_InputField.text    = serverConfig_default.serverIP;
        serverPort_InputField.text  = serverConfig_default.serverPort.ToString();
        nickname_InputField.text    = serverConfig_default.nickname;

        serverConfig.serverIP       = serverConfig_default.serverIP;
        serverConfig.serverPort     = serverConfig_default.serverPort;
        serverConfig.nickname       = serverConfig_default.nickname;
    }

    public void Set_ServerIP(string ip)
    {
        serverConfig.serverIP = ip;
        Debug.Log(serverConfig.serverIP);
    }
    public void Set_ServerPort(string port)
    {
        serverConfig.serverPort = int.Parse(port);
        Debug.Log(serverConfig.serverPort);
    }
    public void Set_Nickname(string nick)
    {
        serverConfig.nickname = nick;
        Debug.Log(serverConfig.nickname);
    }
}
