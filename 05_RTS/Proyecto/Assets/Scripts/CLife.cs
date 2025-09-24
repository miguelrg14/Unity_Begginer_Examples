using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLife : MonoBehaviour
{
    public float maxLife = 100f;
    public float currentLife;
    private float _currentNormalized;
    public float currentNormalized
    {
        private set { _currentNormalized = value; }
        get { return _currentNormalized; }
    }

    public bool invencible = false;

    private bool hasDied = false;

    private void Start()
    {
        ResetLife();
    }

    /// <summary>
    ///     Resets Entity life
    /// </summary>
    public void ResetLife()
    {
        currentLife = maxLife;
        _currentNormalized = currentLife / maxLife;
    }
    /// <summary>
    ///     Checks if health is above 0, so entity is alive
    /// </summary>
    /// <returns></returns>
    public bool IsAlive() => currentLife > 0f;

    /// <summary>
    ///     Inflicts damage to Entity 
    /// </summary>
    /// <param name="meleeAttack"></param>
    /// <returns></returns>
    public bool Damage(float meleeAttack)
    {
        if (hasDied)
            return true;

        if (!invencible)
        {
            currentLife -= meleeAttack;

            if (currentLife < 0f)
                currentLife = 0f;

            _currentNormalized = currentLife / maxLife;
        }

        if (currentLife <= 0f && !hasDied)
        {
            // unit died :(
            SendMessage("UnitDied");
            hasDied = true;

            return true;
        }
        else
            return false;
    }
    /// <summary>
    ///     Heals Entity
    /// </summary>
    /// <param name="healAmount"></param>
    public void Heal(float healAmount)
    {
        currentLife += healAmount;
        if (currentLife > maxLife)
            currentLife = maxLife;

        _currentNormalized = currentLife / maxLife;
    }
}
