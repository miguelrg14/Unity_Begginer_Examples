using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
[RequireComponent(typeof(SocketController))]
public class OnlineManager : MonoBehaviour
{
    private SocketController socket;

    [Header("References")]
    public PlayerOnlineController playerOnline;
    public Competitor_Info competitorInfo;
    [SerializeField] CircuitMng circuitMng;

    [Header("Logic")]
    public GameObject onlineCarPrefab;
    [SerializeField] GameObject entitiesParent;
    private string instanceName;
    private Dictionary<string,OnlineCarController> cars = new Dictionary<string,OnlineCarController>();


    void Awake()
    {
        socket = GetComponent<SocketController>();
    }
    void Start()
    {
        instanceName = (competitorInfo.nickname + playerOnline.GetHashCode());
        socket.Send("join|" + instanceName);
    }

    public void ParseMessages(byte[] iBuffer,int bytesReceived)
    {
        byte[] strBuffer=new byte[bytesReceived];
        Buffer.BlockCopy(iBuffer,0,strBuffer,0,
            bytesReceived);
        string str=Encoding.ASCII.GetString(iBuffer);
        string[] messages=str.Split('$');
        foreach(string message in messages)
        {
            ParseMessage(message);
        }
    }
    private void ParseMessage(string message)
    {
        string[] parameters = message.Split('|');
        switch (parameters[0])
        {
            case "join":
                //add a new online car
                if(cars.ContainsKey(parameters[1]))
                {
                    Debug.LogWarning($"Player {parameters[1]} is trying to enter but is already in the scene D:");
                }
                else
                {
                    GameObject newPlayer = GameObject.Instantiate(onlineCarPrefab, SpawnPosition().position, SpawnPosition().rotation, entitiesParent.transform);
                    newPlayer.name = parameters[1];
                    cars.Add(parameters[1], newPlayer.GetComponent<OnlineCarController>());
                    socket.Send("join|" + instanceName);

                    // Race integration
                    circuitMng.competing.Add(newPlayer);
                    circuitMng.Update_CompetitorsTotal();
                    circuitMng.Check_Ranking();
                }
                break;
            case "updatePosition":
                if (!cars.ContainsKey(parameters[1]))
                {
                    Debug.LogWarning($"Unable to update position of {parameters[1]} key not found");
                }
                else
                {
                    OnlineCarController car;
                    cars.TryGetValue(parameters[1], out car);
                    car.targetPosition=StringToVector3(parameters[2]);
                }
                break;
            case "updateRotation":
                if (!cars.ContainsKey(parameters[1]))
                {
                    Debug.LogWarning($"Unable to update rotation of {parameters[1]} key not found");
                }
                else
                {
                    OnlineCarController car;
                    cars.TryGetValue(parameters[1], out car);
                    car.targetRotation.eulerAngles = StringToVector3(parameters[2]);
                }
                break;
        }
    }


    Transform SpawnPosition()
    {
        Transform[] spawnpoints = circuitMng.spawnpoints;

        if (circuitMng.competitors_Total == 0) return spawnpoints[0];
        if (circuitMng.competitors_Total == 1) return spawnpoints[1];
        if (circuitMng.competitors_Total == 2) return spawnpoints[2];
        if (circuitMng.competitors_Total == 3) return spawnpoints[3];

        else
            return spawnpoints[0];
    }
    string CompetitorName()
    {


        return "New Player";
    }

    public void UpdatePosition()
    {
        socket.Send("updatePosition|" + instanceName + "|" + playerOnline.transform.position);
    }

    public void UpdateRotation()
    {
        socket.Send("updateRotation|" + instanceName + "|"+ playerOnline.transform.rotation.eulerAngles);
    }
    private static Vector3 StringToVector3(string strVector)
    {
        // Remove the parentheses
        if (strVector.StartsWith("(") && strVector.EndsWith(")"))
            strVector = strVector.Substring(1, strVector.Length - 2);

        // split the items
        string[] sArray = strVector.Split(',');

        return new Vector3(
            float.Parse(sArray[0], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[1], CultureInfo.InvariantCulture.NumberFormat),
            float.Parse(sArray[2], CultureInfo.InvariantCulture.NumberFormat)
        );
    }

}
