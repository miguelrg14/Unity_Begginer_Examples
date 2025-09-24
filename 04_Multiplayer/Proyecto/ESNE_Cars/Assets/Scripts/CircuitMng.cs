using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CircuitMng : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CanvasManager canvasManager;

    [Header("Cars")]
    [SerializeField] GameObject entitiesParent;
    public Transform[] spawnpoints;
    public List<GameObject> competing = new List<GameObject>();
    public GameObject[] waypoints;

    [Header("Race Values")]
    public int maxLap                   = 2;
    public int competitors_Total        = 0;
    public int maxCompetitorsCeiling    = 4;

    void Awake()
    {
        List<GameObject> waypointList = new List<GameObject>();

        foreach (Transform tr in transform)
            waypointList.Add(tr.gameObject);

        // Set waypoints race info
        waypoints = waypointList.ToArray();

        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i].GetComponent<WaypointID>().ID = i;
        }
        Fill_CompetitorsList();
        Update_CompetitorsTotal();
    }
    void Start()
    {
        Check_Ranking();
    }

    void Fill_CompetitorsList()
    {
        for (int i = 0; i < entitiesParent.transform.childCount; i++)
        {
            competing.Add(entitiesParent.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    ///     When Player connects server
    /// </summary>
    public void Add_OnlineCompetitor(ref GameObject newPlayer)
    {
        competing.Add(newPlayer);
        Update_CompetitorsTotal();
        Check_Ranking();
    }

    public void Update_CompetitorsTotal()
    {
        competitors_Total = competing.Count;

        for (int i = 0; i < competing.Count; i++)
        {
            competitors_Total++;
            competing[i].GetComponent<Competitor_Info>().id = i;
        }

        canvasManager.Check_MaxCompetitors();
    }

    /// <summary>
    ///     Calculates race players ranking list and sorts it.
    /// </summary>
    public void Check_Ranking()
    {
        // Orders player list
        competing = competing.OrderByDescending(obj => obj.GetComponent<Competitor_Info>().Wp_TotalSurpassed).ToList();

        // Set position variable to each player
        for (int i = 0; i <= competing.Count - 1; i++)
        {
            competing[i].GetComponent<Competitor_Info>().position = i + 1;

            // Set canvas info with position variable updated
            if (competing[i].GetComponent<PlayerController>() != null)
            {
                PlayerController playerController = competing[i].GetComponent<PlayerController>();
                playerController.Set_Canvas_Position();
                playerController.Set_Canvas_Lap();
            }
        }

        // Check number of Competitors that ended race to end up the race and show stats
        int endedRace = 0; 
        for (int i = 0; i <= competing.Count - 1; i++)
        {
            if (competing[i].GetComponent<Competitor_Info>().finishedRace)
                endedRace++;

            if (endedRace == competing.Count)
                StopRace();
        }

        canvasManager.SetPlayerPositionPanel();
    }

    /// <summary>
    ///     Finishes race
    /// </summary>
    void StopRace()
    {
        canvasManager.SetPlayerPositionPanel();
    }

    void OnDrawGizmos()
    {
        if (waypoints.Length > 0)
        {
            Vector3 prev = waypoints[0].transform.position;
            for (int i = 1; i < waypoints.Length; i++)
            {
                Vector3 next = waypoints[i].transform.position;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
            Gizmos.DrawLine(prev, waypoints[0].transform.position);
        }
    }
}
