using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WPFollower : MonoBehaviour
{
    public CircuitMng circuit;
    public int currentWPIndex;
    public Vector3 currentWPPosition;

    public float speed;
    public float rotationSpeed;

    public float distanceToWPThreshold = 1f;

    private void Start()
    {
        currentWPPosition = circuit.waypoints[currentWPIndex].transform.position;
    }

    private void Update()
    {
        float distanceToWP = Vector3.Distance(transform.position, currentWPPosition);

        Vector3 targetDirection = currentWPPosition - transform.position;
        transform.rotation = Quaternion.Slerp(transform.rotation, 
            Quaternion.LookRotation(targetDirection), rotationSpeed * 
            Time.deltaTime);

        transform.Translate(0f, 0f, speed * Time.deltaTime);

        if (distanceToWP <=  distanceToWPThreshold )
        {
            currentWPIndex = (currentWPIndex + 1) % circuit.waypoints.Length;
            currentWPPosition = circuit.waypoints[currentWPIndex].transform.position;
        }
    }
}
