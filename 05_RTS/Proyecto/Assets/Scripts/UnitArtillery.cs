using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class UnitArtillery : UnitBase
{
    private LineRenderer gunLineRenderer;

    public enum State
    {
        None,
        Alert,
        Melee,
        Shooting,
        Chasing
    }
    public State currentArtilleryState = State.None;

    public  float shotAttackRate = 1f;
    private float shotAttackRateAux = 0f;

    public float hitDamage = 2f;

    private Vector3 eyesPosition = new Vector3(0f, 1.2f, 0f);

    private float alertHitTimer = 1f;
    private float alertHitTimerAux = 0f;

    private int currentEnemyIndex = 0;

    public override void Start()
    {
        base.Start();

        Transform gunTr = transform.Find("Gun");
        if (gunTr)
        {
            gunLineRenderer = gunTr.GetComponent<LineRenderer>();
            gunLineRenderer.enabled = false;
        }
        else
            Debug.LogWarning("This artillery unit does not have a gun D:");
    }

    protected override void Update_Idle()
    {
        base.Update_Idle();

        switch (currentArtilleryState)
        {
            case State.None:
                if (enemiesOnVisionSphere.Count > 0)
                    currentArtilleryState = State.Alert;
                break;
            case State.Alert:
                Search_AllEnemies();
                break;
            case State.Shooting:
                Update_Shooting();
                break;
        }
    }
    protected override void Update_GoingTo()
    {
        base.Update_GoingTo();

        if (currentArtilleryState == State.Alert)
        {
            if (alertHitTimerAux <= 0f && currentEnemy)
            {
                Search_Enemy(currentEnemy);
                alertHitTimerAux = alertHitTimer;
            }
            else
                alertHitTimerAux -= Time.deltaTime;
        }
        else if (currentArtilleryState == State.Chasing)
        {
            currentArtilleryState = State.None;
        }
    }
    protected void Update_Shooting()
    {
        if (shotAttackRateAux >= shotAttackRate)
        {
            shotAttackRateAux = 0f;

            gunLineRenderer.enabled = true;
            gunLineRenderer.SetPosition(0, gunLineRenderer.transform.position);
            gunLineRenderer.SetPosition(1, currentEnemy.transform.position + new Vector3(0, 0.85f, 0));

            transform.LookAt(currentEnemy.transform);

            Invoke("HideGun", 0.1f);

            // attack!
            bool enemyKilled = currentEnemy.life.Damage(hitDamage);
            if (enemyKilled)
            {
                if (enemiesOnVisionSphere.Count > 0)
                    currentArtilleryState = State.Alert;
                else
                {
                    currentArtilleryState = State.None;
                    Set_Idle();
                    currentEnemy = null;
                }
            }
        }
        else
        {
            shotAttackRateAux += Time.deltaTime;
        }
    }

    private void HideGun()
    {
        gunLineRenderer.enabled = false;
    }

    /// <summary>
    ///     Gestión del click derecho (dirección de movimiento de las unidades) en el suelo del mapa.
    /// </summary>
    /// <param name="position"></param>
    public override void RightClick_OnFloor(Vector3 position)
    {
        base.RightClick_OnFloor(position);
        currentArtilleryState = State.None;
    }

    /// <summary>
    ///     Adds enemy to "enemiesOnVisionSphere" list
    /// </summary>
    /// <param name="enemy"></param>
    public override void Enemy_Enters_VisionSphere(UnitBase enemy)
    {
        base.Enemy_Enters_VisionSphere(enemy);

        if (currentArtilleryState == State.None ||
            currentArtilleryState == State.Chasing)
        {
            currentArtilleryState = State.Alert;
            alertHitTimerAux = 0f;
        }
    }
    /// <summary>
    ///     Removes enemy to "enemiesOnVisionSphere" list
    /// </summary>
    /// <param name="enemy"></param>
    public override void Enemy_Leaves_VisionSphere(UnitBase enemy)
    {
        base.Enemy_Leaves_VisionSphere(enemy);

        if (enemy == currentEnemy)
        {
            // GoTo to the last known position of currentEnemy
            RightClick_OnTransform(enemy.transform);
            if (currentArtilleryState == State.Shooting)
            {
                currentArtilleryState = State.Chasing;
            }
        }

        if (currentEnemyIndex >= 1)
            currentEnemyIndex--;
    }

    /// <summary>
    ///     Searchs for an enemy in the radious sphere around him
    /// </summary>
    /// <param name="enemyToSearch"></param>
    private void Search_Enemy(UnitBase enemyToSearch)
    {
        Debug.DrawLine(transform.position + eyesPosition, enemyToSearch.transform.position, Color.yellow, 0.25f);

        Vector3 rayDirection = (enemyToSearch.transform.position + new Vector3(0f, 1f, 0f)) - (transform.position + eyesPosition);

        if (Physics.Raycast(transform.position + eyesPosition, rayDirection, out RaycastHit hit, visionSphereRadius))
        {
            if (hit.transform.TryGetComponent(out UnitBase enemy))
            {
                if (enemy == enemyToSearch)
                {
                    // start shooting enemy
                    currentArtilleryState = State.Shooting;
                    currentEnemy = enemy;
                    shotAttackRateAux = 0f;

                    // this is the enemy where are looking for
                    if (currentState == State_base.GoingTo)
                    {
                        Set_Idle();
                    }
                }
            }
        }
    }
    /// <summary>
    ///     Searchs for all the enemies in the radious sphere around him
    /// </summary>
    private void Search_AllEnemies()
    {
        Search_Enemy(enemiesOnVisionSphere[currentEnemyIndex]);

        currentEnemyIndex = (currentEnemyIndex + 1) % enemiesOnVisionSphere.Count;
    }

    protected override void OnGUI()
    {
        base.OnGUI();

        GUI.Label(new Rect(screenPosition.x - 20f, Screen.height - screenPosition.y - 45f, 200f, 20f), currentArtilleryState.ToString());
    }
}
