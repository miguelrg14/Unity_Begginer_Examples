using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Competitor_Info : MonoBehaviour
{
    [Header("Race Values")]
    public int      id                      = 0;
    public string   nickname                = "Player";
    public int      Wp_Target               = 0;
    public int      Wp_Previous             = 0;
    public int      Wp_TotalSurpassed       = 0;
    public int      position                = 0;
    public int      lap                     = 1;
    public bool     Wp_startCounting        = false;
    public bool     finishedRace            = false;

    [Header("Online Info")]
    public SO_ServerConfig serverData;
    public SO_ServerConfig serverData_default;

    public Competitor_Info(string name)
    {
        nickname = name;
    } 
}
