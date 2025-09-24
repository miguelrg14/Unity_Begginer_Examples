using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using static CResources;

public class UnitHarvester : UnitBase
{

    public enum State
    {
        None,
        GoingToMine,
        GoingToChop,
        Waiting,
        Harvesting,
        ReturningToBase
    }
    public State state = State.None;

    [Header("Resources")]
    public int    storage_total = 0;
    public int    storage_stone = 0;
    public int    storage_wood = 0;
    public int    harvestAmountPerChop = 1;
    public int    storage_max = 10;
    public  float harvestRate = 1f;
    private float harvestRateAux = 0f;

    // last mine
    private CResources currentMine;
    private Vector3 lastHarvestPosition;
    private int lastHarvestIndex;

    [SerializeField] bool onBase = false;

    public override void Start()
    {
        base.Start();
    }

    protected override void Update_Idle()
    {
        base.Update_Idle();

        switch (state)
        {
            case State.Harvesting:

                if (storage_total >= storage_max) 
                {
                    Leave_CurrentMine();
                    GoTo(armyBase_gameobject.transform.position);
                    state = State.ReturningToBase;

                    return;
                }

                // Timer para recoger materiales
                if (Time.time > harvestRateAux + harvestRate)
                {
                    harvestRateAux = Time.time;

                    currentMine.Get_Resources(harvestAmountPerChop);

                    if (currentMine.Get_ResourceType() == Resource.Stone)
                        storage_stone += harvestAmountPerChop;
                    if (currentMine.Get_ResourceType() == Resource.Wood)
                        storage_wood += harvestAmountPerChop;

                    storage_total += harvestAmountPerChop;
                }

                break;
        }
    }
    protected override void Update_GoingTo()
    {
        base.Update_GoingTo();

        switch (state)
        {
            case State.GoingToMine:
                // check if the unit is close enough to the mine
                float distToMineSqr = (currentMine.transform.position - transform.position).sqrMagnitude;
                if (distToMineSqr < currentMine.waitDistanceSqr)
                {
                    if (currentMine.Get_HarvestPosition(ref lastHarvestPosition, ref lastHarvestIndex, this))
                    {
                        // there is a vacant -> go to chop
                        state = State.GoingToChop;
                        GoTo(lastHarvestPosition);
                    }
                    else
                    {
                        // there is no vacant -> wait on the queue
                        Set_Idle();
                        state = State.Waiting;
                    }
                }
                break;
            case State.GoingToChop:
                if (currentState == State_base.Idle)
                {
                    // start chopping
                    state = State.Harvesting;
                }
                break;
            case State.ReturningToBase:
                /// TODO
                // check if arrived to the armyBase
                if (onBase)
                {
                    //  let the resources to the army
                    if (storage_stone > 0)
                    {
                        armyBase.Introduce_Resources(storage_stone, Resource.Stone);
                        storage_stone = 0;
                    }
                    if (storage_wood > 0)
                    {
                        armyBase.Introduce_Resources(storage_wood, Resource.Wood);
                        storage_wood = 0;
                    }
                    storage_total = 0;


                    if (currentMine != null)
                    {
                        GoTo(currentMine.transform.position);
                        state = State.GoingToMine;
                    }
                    else
                    {
                        state = State.None;
                    }
                }
                break;
        }
    }

    /// <summary>
    ///     Gestión del click derecho (dirección de movimiento de las unidades) en el suelo del mapa.
    /// </summary>
    /// <param name="position"></param>
    public override void RightClick_OnFloor(Vector3 position)
    {
        base.RightClick_OnFloor(position);

        Leave_CurrentMine();
        state = State.None;
    }
    /// <summary>
    ///     Gestión de interacción con edificios
    /// </summary>
    /// <param name="transform"></param>
    public override void RightClick_OnTransform(Transform transform)
    {
        base.RightClick_OnTransform(transform);

        // TODO manage this click having into account the current currentHarvesterState

        if (transform.TryGetComponent(out CResources mine))
        {
            if (state == State.None || state == State.GoingToMine)
            {
                currentMine = mine;

                if (storage_stone >= storage_max)
                {
                    // TODO go to the base to release the actual resources
                }
                else
                {
                    state = State.GoingToMine;
                    GoTo(mine.transform.position);
                }
            }
            else if (state == State.ReturningToBase)
            {
                // TODO
                // if max resources -> do nothing
                // else goto mine
            }
            else if (state == State.Harvesting || state == State.Waiting || state == State.GoingToChop)
            {
                // TODO
                // if same mine do nothing
                // else goto mine
            }
        }

        if (transform.TryGetComponent(out ArmyBaseController armyBase))
        {
            state = State.ReturningToBase;
            currentMine = null; 
        }
    }

    /// <summary>
    ///     After waiting for a space to harvest in actual resource, this leads to a new harvest position
    /// </summary>
    /// <param name="harvestPosition"></param>
    /// <param name="index"></param>
    public void Finish_Waiting(Vector3 harvestPosition, int index)
    {
        lastHarvestPosition = harvestPosition;
        lastHarvestIndex = index;

        state = State.GoingToChop;
        GoTo(lastHarvestPosition);
    }
    /// <summary>
    ///     Unlinks entity completely from current mine he is working on
    /// </summary>
    private void Leave_CurrentMine()
    {
        if (state == State.Harvesting || state == State.GoingToChop)
        {
            currentMine.Leave_HarvestPosition(lastHarvestIndex);
        }
        else if (state == State.Waiting)
        {
            currentMine.Leave_Queue(this);
        }
    }

    protected override void UnitDied()
    {
        // exit the queues
        Leave_CurrentMine();

        base.UnitDied();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<ArmyBaseController>() != null)
            onBase = true;

        else
            onBase = false;
    }
    private void OnTriggerExit(Collider other)
    {
        onBase = false;
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        GUI.Label(new Rect(screenPosition.x - 20f, Screen.height - screenPosition.y - 45f, 200f, 20f), state.ToString());
    }

}
