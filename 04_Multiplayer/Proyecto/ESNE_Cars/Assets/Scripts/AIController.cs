using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CarController))]
public class AIController : MonoBehaviour
{
    [Header("References")]
    public CircuitMng circuit;
    private CarController car;
    Competitor_Info stats;

    [Header("Vehicle Config")]
    public float steeringSensitivity = 0.01f;
    public float acelerationSensitivity = 1f;
    public float brakeSensitivity = 0.1f;

    private Vector3 targetWP_V3, targetNextWP_V3;

    public float distanceToWPThreshold = 4;

    public float cornerDegrees = 90f;


    [Header("Avoid Obstacles System")]
    [SerializeField] float frontSensorsLenght = 6f;
    [SerializeField] float sideSensorsLenght = 2f;
    [SerializeField] Vector3 frontSensorPosition = new Vector3(0f, 0.2f, 0.5f);
    [SerializeField] float frontSideSensorPosition = 0.2f;
    [SerializeField] float front_SensorAngle = 20f;
    [SerializeField] float frontSide_SensorAngle = 45f;
    [SerializeField] float side_SensorAngle = 90f;
    float avoidMultiplier = 0;

    List<Transform> nodes;
    int currentNode = 0;
    bool avoidingObstacle = false;


    void Awake()
    {
        car     = GetComponent<CarController>();
        stats   = GetComponent<Competitor_Info>();
    }

    void Start()
    {
        targetWP_V3 = circuit.waypoints[stats.Wp_Target].transform.position;
        targetNextWP_V3 = circuit.waypoints[(stats.Wp_Target + 1) % circuit.waypoints.Length].transform.position;
    }

    void FixedUpdate()
    {
        float distanceToTargetWP = Vector3.Distance(targetWP_V3, transform.position);
        Sensors(); // Obstacle avoidance detection
        DirectionCalculations(distanceToTargetWP);
        Race_WaypointLogic();
    }

    void DirectionCalculations(float distanceToTargetWP)
    {
        Vector3 localTarget = transform.InverseTransformPoint(targetWP_V3);
        float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        float speedFactor = car.actualSpeed / car.maxSpeed;
        float corner = Mathf.Clamp(Mathf.Abs(targetAngle), 0f, cornerDegrees);
        float cornerFactor = corner / cornerDegrees;    // Calcular cuan difícil es la curva para frenar o no

        Debug.DrawLine(transform.position, targetWP_V3, Color.green);

        float torque = 1f;
        float brake = 0f;
        float steer = Mathf.Clamp(targetAngle * steeringSensitivity, -1f, 1f) * Mathf.Sign(car.actualSpeed);

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
        if (avoidingObstacle)
            return;
        car.ApplySteering(steer);
    }
    /// <summary>
    ///     Waypoint vector3 point calculation to direct car.
    /// </summary>
    void Race_WaypointLogic()
    {
        targetWP_V3 = circuit.waypoints[stats.Wp_Target].transform.position;
        targetNextWP_V3 = circuit.waypoints[(stats.Wp_Target + 1) % circuit.waypoints.Length].transform.position;
    }

    /// <summary>
    ///     Logic of car sensors that Check obstacles
    /// </summary>
    void Sensors()
    {
        Vector3 sensorStartPos  = transform.position;
        sensorStartPos          += transform.forward * frontSensorPosition.z;
        sensorStartPos          += transform.up      * frontSensorPosition.y;
        avoidingObstacle        = false;


        /// Right side 
        sensorStartPos += transform.right * frontSideSensorPosition;
        Sensor_Create_Default(sensorStartPos, Quaternion.AngleAxis(front_SensorAngle        , transform.up  ) * transform.forward, frontSensorsLenght   );     // Front right side sensor
        Sensor_Create_Default(sensorStartPos, Quaternion.AngleAxis(frontSide_SensorAngle    , transform.up  ) * transform.forward, frontSensorsLenght   );     // Front right side angle sensor
        Sensor_Create_Default(sensorStartPos, Quaternion.AngleAxis(side_SensorAngle         , transform.up  ) * transform.forward, sideSensorsLenght    );     // Right side angle sensor

        /// Left side
        sensorStartPos -= transform.right * frontSideSensorPosition * 2;
        Sensor_Create_Default(sensorStartPos, Quaternion.AngleAxis(-front_SensorAngle       , transform.up  ) * transform.forward, frontSensorsLenght   );     // Front left side sensor
        Sensor_Create_Default(sensorStartPos, Quaternion.AngleAxis(-frontSide_SensorAngle   , transform.up  ) * transform.forward, frontSensorsLenght   );     // Front right side angle sensor
        Sensor_Create_Default(sensorStartPos, Quaternion.AngleAxis(-side_SensorAngle        , transform.up  ) * transform.forward, sideSensorsLenght    );     // Right side angle sensor

        /// Front center sensor
        //RaycastHit hit;
        //if (avoidMultiplier == 0)
        //{
        //    if (Physics.Raycast(sensorStartPos, transform.forward, out hit, frontSensorsLenght))
        //    {
        //        if (!hit.collider.CompareTag("Terrain") && !hit.collider.CompareTag("Waypoint"))
        //        {
        //            Debug.DrawLine(sensorStartPos, hit.point);
        //            avoidingObstacle = true;
        //            avoidMultiplier = 1f;

        //            // Backwards
        //            //if (hit.normal.x < 0f)  avoidMultiplier = -1f;
        //            //else                    avoidMultiplier = 1f;
        //        }
        //    }
        //}

        if (avoidingObstacle)
        {
            //Vector3 localTarget = transform.InverseTransformPoint(targetWP_V3);
            //float targetAngle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
            float steer = Mathf.Clamp(avoidMultiplier * steeringSensitivity, -1f, 1f) * Mathf.Sign(car.actualSpeed);

            car.ApplySteering(steer);
            return;
        }
    }

    /// <summary>
    ///     Builds Sensors
    /// </summary>
    void Sensor_Create_Default(Vector3 sensorStartPos, Vector3 direction, float sensorLenght)
    {
        RaycastHit hit;

        if (Physics.Raycast(sensorStartPos, direction, out hit, sensorLenght))
        {
            if (!hit.collider.CompareTag("Terrain") && !hit.collider.CompareTag("Waypoint"))
            {
                Debug.DrawLine(sensorStartPos, hit.point);
                avoidingObstacle = true;
                avoidMultiplier -= 1f;
            }
        }
    }
}
