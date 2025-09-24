using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AIFollowerController : MonoBehaviour
{
    private CarController car;
    public CircuitMng circuit;

    public float steeringSensitivity = 0.01f;
    public float acelerationSensitivity = 1f;
    public float brakeSensitivity = 0.1f;

    public WPFollower rabbit;
    private Transform rabbitTransform;

    public float maxDistanceToRabbit = 30f;

    public float cornerDegrees = 90f;

    private void Awake()
    {
        car = GetComponent<CarController>();
    }

    private void Start()
    {
        rabbitTransform = rabbit.transform;
    }
    private void Update()
    {
        Debug.DrawLine(transform.position, rabbitTransform.position, Color.blue);

        float distanceToRabbit=Vector3.Distance(transform.position, rabbitTransform.position);
        if (distanceToRabbit > maxDistanceToRabbit)
            rabbit.speed = 0;
        else
            rabbit.speed = Mathf.Lerp(0f,car.actualSpeed *3f, 1f - distanceToRabbit / maxDistanceToRabbit);

    }
    void FixedUpdate()
    {
        Vector3 localTarget = transform.InverseTransformPoint(rabbitTransform.position);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;

        float speedFactor = car.actualSpeed / car.maxSpeed;

        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0f, cornerDegrees);
        float cornerFactor = corner / cornerDegrees;    // Calcular cuan difícil es la curva para frenar o no


        float torque = 1f;
        float brake = 0f;
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1f, 1f) *
            Mathf.Sign(car.actualSpeed);

        // Calcular freno después d la curva
        if (speedFactor >= 0.08f && cornerFactor >= 0.2f)
        {
            brake = Mathf.Lerp(0f, 0.5f + (speedFactor * brakeSensitivity), cornerFactor); // es igual a "brake = cornerFactor;"
        }
        // Calcular freno antes d la curva
        if (speedFactor >= 0.16f && (cornerFactor >= 0.4f))
        {
            torque = Mathf.Lerp(0f, acelerationSensitivity, 1f - cornerFactor);
        }

        car.ApplyTorque(torque);
        car.ApplyBrake(brake);
        car.ApplySteering(steer);
    }
}
