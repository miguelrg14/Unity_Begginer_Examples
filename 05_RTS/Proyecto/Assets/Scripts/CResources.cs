using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CResources : MonoBehaviour
{
    public enum Resource
    {
        Stone,
        Wood
    }
    public Resource resource = Resource.Stone;

    public int totalResources = 10000;
    public int actualResources;

    public int totalHarvestingSpots = 8;
    private Vector3[] harvestingSpots;
    private bool[] harvestingSpotsTaken;

    float despPosition = 1f;

    private List<UnitHarvester> harvestersQueue = new List<UnitHarvester>();

    private GameObject[] cubes;

    [HideInInspector]
    public float waitDistanceSqr;

    void Start()
    {
        actualResources = totalResources;

        harvestingSpots = new Vector3[totalHarvestingSpots];
        harvestingSpotsTaken = new bool[totalHarvestingSpots];

        cubes = new GameObject[totalHarvestingSpots];

        float radius = transform.GetComponent<SphereCollider>().radius + despPosition;
        float PI2 = Mathf.PI * 2;

        for (int i = 0; i < totalHarvestingSpots; i++)
        {
            Vector3 pos = new Vector3(
                transform.position.x + radius * Mathf.Sin(i * (PI2 / totalHarvestingSpots)),
                transform.position.y,
                transform.position.z + radius * Mathf.Cos(i * (PI2 / totalHarvestingSpots))
            );
            harvestingSpots[i] = pos;
            harvestingSpotsTaken[i] = false;

            cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cubes[i].transform.position = pos;
            Destroy(cubes[i].GetComponent<BoxCollider>());
            cubes[i].transform.parent = this.transform;
        }

        waitDistanceSqr = (radius + despPosition * 2) * (radius + despPosition * 2);
    }
    void Update()
    {
        
    }

    /// <summary>
    ///     Substracts resource quantity from prop attached and give it to harvest entity interacting with it
    /// </summary>
    /// <param name="cant"></param>
    /// <returns></returns>
    public int Get_Resources(int cant)
    {
        if (cant <= actualResources)
        {
            actualResources -= cant;
        }
        else
        {
            cant = actualResources;
            actualResources = 0;
        }

        return cant;
    }
    public Resource Get_ResourceType() => resource;
    /// <summary>
    ///     Calculates harvester position depending on free spaces
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="index"></param>
    /// <param name="harvester"></param>
    /// <returns></returns>
    public bool Get_HarvestPosition(ref Vector3 pos, ref int index, UnitHarvester harvester)
    {
        int i = 0; bool vacant = false;
        while (!vacant && i < totalHarvestingSpots)
        {
            if (!harvestingSpotsTaken[i])
            {
                // position i is vacant
                pos = harvestingSpots[i];
                index = i;
                OccupySpot(i);
                vacant = true;
            }
            else
                i++;
        }

        if (!vacant)
        {
            harvestersQueue.Add(harvester);
        }

        return vacant;
    }
    /// <summary>
    ///     Unataches harvester from mine
    /// </summary>
    /// <param name="index"></param>
    public void Leave_HarvestPosition(int index)
    {
        FreeSpot(index);

        if (harvestersQueue.Count > 0)
        {
            // get the first harvester of the queue and send it to chop
            UnitHarvester harvester = harvestersQueue[0];
            harvestersQueue.RemoveAt(0);
            harvester.Finish_Waiting(harvestingSpots[index], index);
            OccupySpot(index);
        }
    }
    /// <summary>
    ///     Removes harvester from harvesters waiting free space to recolect queue
    /// </summary>
    /// <param name="harvester"></param>
    public void Leave_Queue(UnitHarvester harvester)
    {
        harvestersQueue.Remove(harvester);
    }

    /// <summary>
    ///     Harvester ==> Occupy spot on mine to collect
    /// </summary>
    /// <param name="index"></param>
    private void OccupySpot(int index)
    {
        harvestingSpotsTaken[index] = true;
        cubes[index].GetComponent<Renderer>().material.color = Color.red;
    }
    /// <summary>
    ///     Harvester ==> Free spot in mine to let another harvester collect in it
    /// </summary>
    /// <param name="index"></param>
    private void FreeSpot(int index)
    {
        harvestingSpotsTaken[index] = false;
        cubes[index].GetComponent<Renderer>().material.color = Color.white;
    }

}
