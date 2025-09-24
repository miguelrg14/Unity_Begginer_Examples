using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

public class OnlineCarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Competitor_Info stats;
    [SerializeField] CircuitMng circuitMng;

    [Header("Stats")]
    public float updatePositionSpeed    = 1;
    public float updateRotationSpeed    = 1;

    public Vector3 targetPosition;
    public Quaternion targetRotation;

    void Awake()
    {
        stats = GetComponent<Competitor_Info>();

        if (circuitMng == null)
            circuitMng = GameObject.Find("Manager_Circuit").GetComponent<CircuitMng>();
    }
    void Start()
    {
        //targetPosition = transform.position;
        //targetRotation = transform.rotation;

        targetPosition = SpawnPosition().position;
        targetRotation = SpawnPosition().rotation;
    }

    void Update()
    {
        transform.position = Vector3.Lerp   (transform.position, targetPosition, updatePositionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, updateRotationSpeed * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Waypoint")
        {
            if (other.gameObject.GetComponent<WaypointID>().ID == stats.Wp_Target)
            {
                Check_WaypointLogic();
            }
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

    /// <summary>
    ///     Waypoint logic & lap summation.
    /// </summary>
    void Check_WaypointLogic()
    {
        // Win => Last waypoint
        if (stats.lap == circuitMng.maxLap && stats.Wp_Target == 0 && stats.Wp_TotalSurpassed != 0)
        {
            stats.Wp_Target++;
            stats.finishedRace = true;
            stats.Wp_TotalSurpassed++;
            circuitMng.Check_Ranking();
            transform.gameObject.SetActive(false);
            return;
        }

        // Last waypoint in lap
        int circuitLastWPNumber = circuitMng.waypoints.Length - 1;
        if (stats.Wp_Target >= circuitLastWPNumber)
        {
            stats.Wp_Target = 0;
            stats.Wp_startCounting = true;
            Check_LastWp();
            stats.Wp_TotalSurpassed++;
            circuitMng.Check_Ranking();
            return;
        }
        // General waypoints in race
        else
        {
            stats.Wp_Target++;
            Check_LastWp();
            if (stats.Wp_startCounting == true)
                Add_Lap();
            stats.Wp_TotalSurpassed++;
            circuitMng.Check_Ranking();
        }
    }

    /// <summary>
    ///     Lap summation logic.
    /// </summary>
    void Add_Lap()
    {
        if (stats.lap < circuitMng.maxLap) stats.lap++;
        else stats.lap = circuitMng.maxLap;
    }
    /// <summary>
    ///     Last WayPoint summation logic.
    /// </summary>
    void Check_LastWp()
    {
        if (stats.Wp_Target > 0) stats.Wp_Previous = stats.Wp_Target - 1;
        // Last WayPoint in the list
        if (stats.Wp_Target == 0) stats.Wp_Previous++;
    }

}
