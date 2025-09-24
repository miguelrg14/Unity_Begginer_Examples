using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(CSelectable))]
public class UnitBase : MonoBehaviour
{
    [HideInInspector] public NavMeshAgent nmAgent;
    [HideInInspector] public CSelectable selectable;
    [HideInInspector] public CTeam team;
    [HideInInspector] public CLife life;

    public enum State_base
    {
        Idle,
        GoingTo,
        Attacking,
        Dying
    }
    public State_base currentState  = State_base.Idle;
    public State_base prevState     = State_base.Idle;

    public float destinyThreshold = 0.5f;
    [HideInInspector] public float destinyThreshold2;

    public float visionSphereRadius = 10;
    [HideInInspector] public float visionSphereRadius2;

    public float boundingRadius = 1f;
    [HideInInspector] public float boundingRadius2;

    [Header("Attack")]
    public float attackRate = 2f;
    protected float attackRateAux = 0f;
    public float meleeAttack = 5f;

    [HideInInspector] public Vector3 screenPosition;

    Vector3 currentDestination;

    public ArmyBaseController armyBase;
    public GameObject armyBase_gameobject;

    protected UnitBase currentEnemy = null;
    protected List<UnitBase> enemiesOnVisionSphere = new List<UnitBase>();

    Texture2D progressBarEmpty, progressBarFull;

    public UnityEvent<UnitBase> unitDiedEvent;

    void Awake()
    {
        nmAgent = GetComponent<NavMeshAgent>();
        selectable = GetComponent<CSelectable>();
        team = GetComponent<CTeam>();
        life = GetComponent<CLife>();
    }
    public virtual void Start()
    {
        visionSphereRadius2 = visionSphereRadius * visionSphereRadius;
        boundingRadius2 = boundingRadius * boundingRadius;
        destinyThreshold2 = destinyThreshold * destinyThreshold;

        progressBarEmpty = new Texture2D(1, 1);
        progressBarEmpty.SetPixel(0, 0, Color.red);
        progressBarEmpty.Apply();
        progressBarFull = new Texture2D(1, 1);
        progressBarFull.SetPixel(0, 0, Color.green);
        progressBarFull.Apply();
    }
    void Update()
    {
        screenPosition = Camera.main.WorldToScreenPoint(transform.position);

        switch (currentState)
        {
            case State_base.Idle:      Update_Idle();      break;
            case State_base.GoingTo:   Update_GoingTo();   break;
            case State_base.Attacking: Update_Attacking(); break;
            case State_base.Dying:     Update_Dying();     break;
        }

        foreach(UnitBase enemy in enemiesOnVisionSphere)
        {
            Debug.DrawLine(transform.position, enemy.transform.position, Color.blue);
        }
    }

    protected virtual void Update_Idle()
    {

    }
    protected virtual void Update_GoingTo()
    {
        Vector3 offset = currentDestination - transform.position;
        float offsetSqrMagnitude = offset.sqrMagnitude;

        if (currentEnemy)
        {
            // going to attack enemy
            Vector3 offsetToEnemy = currentEnemy.transform.position - transform.position;
            float offsetSqrMagnitudeToEnemy = offsetToEnemy.sqrMagnitude;

            if (offsetSqrMagnitudeToEnemy <= visionSphereRadius2)
            {
                // check if the enemy is on sight
                currentDestination = currentEnemy.transform.position;
                nmAgent.SetDestination(currentDestination);

                if (offsetSqrMagnitudeToEnemy <= boundingRadius2 + currentEnemy.boundingRadius2)
                {
                    nmAgent.isStopped = true;

                    prevState = currentState;
                    currentState = State_base.Attacking;
                }
            }
            else if (offsetSqrMagnitude <= destinyThreshold2)
            {
                // check if the Unit has arrived but can't see the enemy
                Set_Idle();

                currentEnemy = null;
            }
        }
        else if (offsetSqrMagnitude <= destinyThreshold2)
        {
            // Unit has arrived at its destination
            Set_Idle();
        }
    }
    protected virtual void Update_Attacking()
    {
        if (currentEnemy)
        {
            attackRateAux -= Time.deltaTime;

            // Check if the enemy is on range
            Vector3 offset = currentEnemy.transform.position - transform.position;
            float offsetSqrMagnitude = offset.sqrMagnitude;

            if (offsetSqrMagnitude <= boundingRadius2 + currentEnemy.boundingRadius2)
            {
                if (attackRateAux <= 0f)
                {
                    // attack!
                    bool enemyKilled = currentEnemy.life.Damage(meleeAttack);
                    if (enemyKilled)
                    {
                        Set_Idle();
                        currentEnemy = null;
                    }

                    attackRateAux = attackRate;
                }
            }
            else if (offsetSqrMagnitude <= visionSphereRadius2)
            {
                // update the destination towards the current enemy
                GoTo(currentEnemy.transform.position);
            }
            else
            {
                // the enemy is no longer on sight
                Set_Idle();

                currentEnemy = null;
            }
        }
        else
        {
            Set_Idle();
        }
    }
    protected virtual void Update_Dying()
    {

    }

    /// <summary>
    ///     Cambia el color al del color del equipo
    /// </summary>
    /// <param name="color"></param>
    public void Set_TeamColor(Color color)
    {
        team.color = color;
        selectable.Set_Color(color);
    }
    /// <summary>
    ///     Cambio del estado actual de la unidad a un estado de idle
    /// </summary>
    protected void Set_Idle()
    {
        nmAgent.isStopped = true;

        prevState = currentState;
        currentState = State_base.Idle;
    }

    /// <summary>
    ///     Gestión del click derecho (dirección de movimiento de las unidades) en el suelo del mapa.
    /// </summary>
    /// <param name="position"></param>
    public virtual void RightClick_OnFloor(Vector3 position)
    {
        currentEnemy = null;
        GoTo(position);
    }
    /// <summary>
    ///     Gestión de interacción con edificios
    /// </summary>
    /// <param name="transform"></param>
    public virtual void RightClick_OnTransform(Transform transform)
    {
        CTeam otherTeam = transform.GetComponent<CTeam>();
        if (otherTeam)
        {
            if (otherTeam.teamNumber !=  team.teamNumber)
            {
                // right click on enemy -> Attack!!!!
                GoTo(transform.position);
                currentEnemy = transform.GetComponent<UnitBase>();
            }
            else
            {
                currentEnemy = null;

                GoTo(transform.position);
            }
        }
    }

    public void GoTo(Vector3 destination)
    {
        if (currentState != State_base.Dying)
        {
            currentDestination = destination;
            nmAgent.isStopped = false;
            nmAgent.SetDestination(destination);

            prevState = currentState;
            currentState = State_base.GoingTo;
        }
    }

    /// <summary>
    ///     Adds enemy to "enemiesOnVisionSphere" list
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void Enemy_Enters_VisionSphere(UnitBase enemy)   => enemiesOnVisionSphere.Add    (enemy);
    /// <summary>
    ///     Removes enemy to "enemiesOnVisionSphere" list
    /// </summary>
    /// <param name="enemy"></param>
    public virtual void Enemy_Leaves_VisionSphere  (UnitBase enemy)   => enemiesOnVisionSphere.Remove (enemy);

    protected virtual void UnitDied()
    {
        nmAgent.isStopped = true;

        prevState = currentState;
        currentState = State_base.Dying;

        DistanceMatrix.Remove_Unit(this);

        unitDiedEvent.Invoke(this);
    }

    protected virtual void OnGUI()
    {
        GUI.Label(new Rect(screenPosition.x - 20f, Screen.height - screenPosition.y - 30f, 200f, 20f), currentState.ToString());

        if (currentState != State_base.Dying)
        {
            Rect rect1 = new Rect(screenPosition.x - 10.0f, Screen.height - screenPosition.y - 30.0f, 20.0f, 3.0f);
            GUI.DrawTexture(rect1, progressBarEmpty);
            Rect rect2 = new Rect(screenPosition.x - 10.0f, Screen.height - screenPosition.y - 30.0f, 20.0f * life.currentNormalized, 3.0f);
            GUI.DrawTexture(rect2, progressBarFull);
        }
    }

}
