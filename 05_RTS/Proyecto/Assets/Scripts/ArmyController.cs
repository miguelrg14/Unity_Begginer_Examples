using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.CanvasScaler;

[RequireComponent(typeof(CTeam))]
public class ArmyController : MonoBehaviour
{

    private CTeam team;

    private int selectableLayerMask;
    private int floorLayerMask;

    public List<UnitBase> units = new List<UnitBase>();
    public List<ArmyBaseController> armyBases = new List<ArmyBaseController>();

    public List<UnitBase> selectedUnits = new List<UnitBase>();
    public CSelectable selectedBuilding = null;

    // mouse control
    [HideInInspector]
    public bool selecting; // rectangle selection
    private Vector3 lastClick = new Vector3();
    [HideInInspector]
    public Rect selectionRect = new Rect();

    private void Awake()
    {
        team = GetComponent<CTeam>();
    }
    void Start()
    {
        selectableLayerMask = LayerMask.GetMask("Selectable");
        floorLayerMask = LayerMask.GetMask("Floor");

        // find all the units in the scene that belongs to this army-team
        // add them to the armys units array and set their color
        UnitBase[] unitsInScene = GameObject.FindObjectsOfType<UnitBase>();
        foreach (UnitBase unit in unitsInScene)
        {
            if (unit.team.teamNumber == team.teamNumber)
            {
                Add_Unit_ToArmy(unit);
            }
        }

        // set the color of the army bases
        foreach (ArmyBaseController armyBase in armyBases)
        {
            armyBase.Set_TeamColor(team.color);
        }
    }
    void Update()
    {
        // players control
        // left click down
        if (Input.GetMouseButtonDown(0))
        {
            lastClick = Input.mousePosition;

            selectionRect.x = lastClick.x;
            selectionRect.y = lastClick.y;

            if (selectedUnits.Count > 0 || selectedBuilding)
            {
                // deselect the units if left-click on the floor
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, floorLayerMask))
                {
                    Transform objectHit = hit.transform;
                    if (objectHit)
                    {
                        Deselect_All();
                    }
                }
            }
        }

        // left click up (unit selection)
        if (Input.GetMouseButtonUp(0))
        {
            selecting = false;

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // unit left-click selection
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, selectableLayerMask))
            {
                Transform objectHit = hit.transform;

                CTeam hitTeam = objectHit.GetComponent<CTeam>();

                if (hitTeam && hitTeam.teamNumber == team.teamNumber)
                {
                    CSelectable hitSelectable = objectHit.GetComponent<CSelectable>();
                    if (hitSelectable)
                    {
                        hitSelectable.Set_Selected();

                        UnitBase unit = hitSelectable.GetComponent<UnitBase>();
                        if (unit)
                        {
                            // unit selection
                            selectedUnits.Add(unit);
                        }
                        else
                        {
                            // building selection
                            ArmyBaseController armyBase = hitSelectable.GetComponent<ArmyBaseController>();
                            if (armyBase)
                            {
                                selectedBuilding = armyBase.selectable;
                            }
                        }
                    }
                }
            }
        }

        // right click
        if (Input.GetMouseButtonDown(1))
        {
            if (selectedUnits.Count > 0)
            {
                // deselect the units if left-click on the floor
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                int rightClickLayerMask = floorLayerMask | selectableLayerMask;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, rightClickLayerMask))
                {
                    Transform objectHit = hit.transform;
                    if (objectHit)
                    {
                        if (1 << objectHit.gameObject.layer == floorLayerMask)
                        {
                            // right click on the floor
                            foreach (UnitBase unit in selectedUnits)
                            {
                                unit.RightClick_OnFloor(hit.point);
                            }
                        }
                        else if (1 << objectHit.gameObject.layer == selectableLayerMask)
                        {
                            // right click on a CSelectable
                            foreach (UnitBase unit in selectedUnits)
                            {
                                unit.RightClick_OnTransform(objectHit);
                            }
                        }
                    }
                }
            }
        }

        // mouse move while clicking
        if (Input.GetMouseButton(0) && (Input.mousePosition != lastClick))
        {
            if (!selecting)
            {
                Deselect_All();

                selecting = true;
                selectionRect.width = selectionRect.height = 0;
            }
            else
            {
                // update the selection rectangle point values
                selectionRect.width = Input.mousePosition.x - selectionRect.x;
                selectionRect.height = Input.mousePosition.y - selectionRect.y;

                if (lastClick.x > Input.mousePosition.x)
                {
                    // drag towards left - flip x
                    selectionRect.width = lastClick.x - Input.mousePosition.x;
                    selectionRect.x = Input.mousePosition.x;
                }

                if (lastClick.y > Input.mousePosition.y)
                {
                    // drag towards down - flip y
                    selectionRect.height = lastClick.y - Input.mousePosition.y;
                    selectionRect.y = Input.mousePosition.y;
                }

                // multiselection
                Deselect_All();
                foreach (UnitBase unit in units)
                {
                    if (selectionRect.Contains(unit.screenPosition))
                    {
                        selectedUnits.Add(unit);
                        unit.selectable.Set_Selected();
                    }
                }

                /*foreach (UnitBase unit in units)
                { 
                    bool unitIsSelected = selectedUnits.Contains(unit);

                    if (selectionRect.Contains(unit.screenPosition))
                    {
                        if (!unitIsSelected)
                        {
                            selectedUnits.Add(unit);
                            unit.selectable.SetSelected();
                        }
                    }
                    else if (unitIsSelected)
                    {
                        selectedUnits.Remove(unit);
                        unit.selectable.SetDeselected();
                    }
                }*/
            }
        }

        // key actions
        if (selectedBuilding)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                // spawn a BaseUnit
                Spawn_Unit(0);
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                // spawn an ArtilleryUnit
                Spawn_Unit(1);
            }
            if (Input.GetKeyDown(KeyCode.D))
            {
                // spawn a HarvesterUnit
                Spawn_Unit(2);
            }
        }

        if (Input.GetKeyDown(KeyCode.Delete))
        {
            // kill the selected units
            for (int i = selectedUnits.Count - 1; i >= 0; i--)
            {
                CLife life = selectedUnits[i].GetComponent<CLife>();
                life.Damage(life.maxLife);
            }
        }
    }

    private void Spawn_Unit(int armyBaseAction)
    {
        ArmyBaseController armyBase = selectedBuilding.GetComponent<ArmyBaseController>();
        if (armyBase)
        {
            // spawn unit
            GameObject newUnitGO = armyBase.Action(armyBaseAction);

            Add_Unit_ToArmy(newUnitGO.GetComponent<UnitBase>());
        }
    }
    private void Add_Unit_ToArmy(UnitBase unit)
    {
        unit.team.teamNumber = team.teamNumber;
        units.Add(unit);
        unit.Set_TeamColor(team.color);
        unit.unitDiedEvent.AddListener(UnitDied);
        DistanceMatrix.Insert_Unit(unit);
    }

    private void Deselect_All()
    {
        foreach(UnitBase unit in selectedUnits)
        {
            unit.selectable.Set_Deselected();
        }
        selectedUnits.Clear();

        Deselect_SelectedBuilding();
    }
    private void Deselect_SelectedBuilding()
    {
        if (selectedBuilding)
            selectedBuilding.Set_Deselected();
        selectedBuilding = null;
    }

    private void UnitDied(UnitBase unit)
    {
        if (selectedUnits.Contains(unit))
        {
            selectedUnits.Remove(unit);
        }
        units.Remove(unit);

        unit.unitDiedEvent.RemoveAllListeners();

        StartCoroutine(Destroy_Unit(unit.gameObject));
    }
    private IEnumerator Destroy_Unit(GameObject unit)
    {
        yield return new WaitForSeconds(1f);

        Destroy(unit);
    }

}
