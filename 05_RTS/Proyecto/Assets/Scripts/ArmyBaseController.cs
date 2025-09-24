using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CResources;

[RequireComponent(typeof(CTeam))]
[RequireComponent(typeof(CSelectable))]
public class ArmyBaseController : MonoBehaviour
{

    [HideInInspector]
    public CTeam team;
    [HideInInspector]
    public CSelectable selectable;

    private Transform spawnPoint;

    [Header("Unit Prefabs")]
    public GameObject baseUnitPrefab;
    public GameObject artilleryUnitPrefab;
    public GameObject harvesterUnitPrefab;

    [Header("Resources")]
    public int actualResources;
    public int Resources_max = 1000;
    public int stone_resource;
    public int wood_resource;


    void Awake()
    {
        team = GetComponent<CTeam>();
        selectable = GetComponent<CSelectable>();
    }
    void Start()
    {
        spawnPoint = transform.Find("SpawnPoint");
        if (!spawnPoint)
            Debug.LogWarning("Spawn Point not found in this army base.");
    }

    /// <summary>
    ///     Set oveline material color of base
    /// </summary>
    /// <param name="color"></param>
    public void Set_TeamColor(Color color)
    {
        team.color = color;
        selectable.Set_Color(color);
    }

    /// <summary>
    ///     Base actions: Spawn entites
    /// </summary>
    /// <param name="actionType"></param>
    /// <returns></returns>
    public GameObject Action(int actionType)
    {
        GameObject unit = null;

        switch(actionType)
        {
            case 0:
                unit = GameObject.Instantiate(baseUnitPrefab, spawnPoint.position + Random.insideUnitSphere, spawnPoint.rotation);
                break;
            case 1:
                unit = GameObject.Instantiate(artilleryUnitPrefab, spawnPoint.position + Random.insideUnitSphere, spawnPoint.rotation);
                break;
            case 2:
                unit = GameObject.Instantiate(harvesterUnitPrefab, spawnPoint.position + Random.insideUnitSphere, spawnPoint.rotation);
                unit.GetComponent<UnitBase>().armyBase_gameobject = gameObject;
                break;
        }

        // set the reference to this base in the new unit
        unit.GetComponent<UnitBase>().armyBase = this;

        return unit;
    }

    /// <summary>
    ///     Resource introduction in base for harvesters
    /// </summary>
    /// <param name="cant"></param>
    /// <param name="resourceType"></param>
    /// <returns></returns>
    public int Introduce_Resources(int cant, Resource resourceType)
    {
        if (resourceType == Resource.Stone)
            stone_resource += cant;

        if (resourceType == Resource.Wood)
            wood_resource += cant;

        actualResources += cant;

        return cant;
    }
}
