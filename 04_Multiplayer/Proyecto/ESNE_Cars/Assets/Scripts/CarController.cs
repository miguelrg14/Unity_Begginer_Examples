using TMPro;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Competitor References")]
    public CircuitMng circuitMng;
    Competitor_Info stats;

    [Header("References")]
    public Rigidbody rb;

    [HideInInspector] public WheelController[] frontWheels  = new WheelController[2];
    [HideInInspector] public WheelController[] backWheels   = new WheelController[2];
    [HideInInspector] public WheelController[] allWheels    = new WheelController[4];

    [Header("Car config")]
    public float goTorque               = 1000f;
    public float brakeTorque            = 2000f;
    public float maxSteerAngle          = 30f;

    public float antiRoll               = 2000f;

    public float stereoFactor           = 5f;
    public float skidThreshold          = 0.1f;
    float[] skidValues                  = new float[4];

    public AnimationCurve engineSoundCurve;
    public float engineSoundMinPitch    = 0.2f;
    public float engineSoundMaxPitch    = 2.5f;

    public enum CarType
    {
        FWD,
        RWD,
        AWD
    }
    public CarType carType              = CarType.FWD;

    [Header("Audio")]
    public AudioSource skidSound;
    public AudioSource engineSound;

    public float maxSpeed               = 90f;
    public float actualSpeed { get { return rb.velocity.magnitude; } }

    // Visual Feedback
    [Header("Lights")]
    Material material;
    public GameObject leftLight;
    public GameObject rightLight;
    [Header("Smoke")]
    public ParticleSystem smokePrefab;
    public ParticleSystem[] skidSmokes = new ParticleSystem[4];


    void Awake()
    {
        rb      = GetComponent<Rigidbody>();
        stats   = GetComponent<Competitor_Info>();

        if (circuitMng == null)
            circuitMng      = GameObject.Find("Manager_Circuit").GetComponent<CircuitMng>();

        //Get the wheels references
        frontWheels[0]      = transform.Find("FR_Wheel").GetComponent<WheelController>();
        frontWheels[1]      = transform.Find("FL_Wheel").GetComponent<WheelController>();
        backWheels[0]       = transform.Find("BL_Wheel").GetComponent<WheelController>();
        backWheels[1]       = transform.Find("BR_Wheel").GetComponent<WheelController>();

        //Add all wheels    
        allWheels[0]        = frontWheels[0];
        allWheels[1]        = frontWheels[1];
        allWheels[2]        = backWheels [0];
        allWheels[3]        = backWheels [1];

        stats.Wp_Target     = 0;
        stats.Wp_Previous   = 0;
        stats.lap           = 1;
    }
    void Start()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        // No funciona => No cambia el material ni emissive
        //
        //material = GetComponent<Renderer>().material;
        //material.SetColor("_EmissionColor", Color.black);
        //

        for (int i = 0; i < skidSmokes.Length; i++)
        {
            skidSmokes[i] = Instantiate(smokePrefab, this.transform);
            skidSmokes[i].Stop();
        }

    }
    void FixedUpdate()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        GroundWheels(frontWheels[1].wCollider, frontWheels[0].wCollider);
        GroundWheels(backWheels[1].wCollider, backWheels[0].wCollider);

        CheckSkid();
        CalculateEngineSound();

        if (this.transform.rotation.z >= 70 || this.transform.rotation.z <= -70)
            ResetCar();

        if (Input.GetKeyDown(KeyCode.X))
            AntiTipSystem();

        // If car gets upside down
        if (Vector3.Dot(transform.up, Vector3.down) > 0)
            ResetCar();
    }
    void OnDestroy()
    {
        Destroy(material);
    }

    #region CAR
    void BrakeLights(float brakeInput)
    {
        if (brakeInput > 0)
        {

        }


        /*
        if (brakeInput)
        {

        }
        */
    }

    public void ApplyTorque(float torqueInput)
    {
        float torque = goTorque * torqueInput;

        if (actualSpeed < maxSpeed)
        {
            //Apply impulse depending on the type of the car we decide
            switch (carType)
            {
                case CarType.FWD:
                    foreach (WheelController wheel in frontWheels)
                    {
                        wheel.wCollider.motorTorque = torque;
                    }
                    break;

                case CarType.RWD:
                    foreach (WheelController wheel in backWheels)
                    {
                        wheel.wCollider.motorTorque = torque;
                    }
                    break;

                case CarType.AWD:

                    foreach (WheelController wheel in allWheels)
                    {
                        wheel.wCollider.motorTorque = torque;
                    }

                    break;

                default:
                    print("No car type selected");
                    break;
            }
        }
    }

    public void ApplyBrake(float brakeInput)
    {
        float brake = brakeTorque * brakeInput;

        //Brake on all wheels
        foreach (WheelController wheel in allWheels)
        {
            wheel.wCollider.brakeTorque = brake;

            leftLight.SetActive(true);
            rightLight.SetActive(true);
        }
    }

    public void ApplySteering(float steeringInput)
    {
        float steer = steeringInput * maxSteerAngle;

        //Steer on the front ones just
        foreach (WheelController wheel in frontWheels)
        {
            wheel.wCollider.steerAngle = steer;
        }
    }

    public void ResetCar()
    {
        Debug.Log("Resetting car");

        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        foreach (WheelController wheel in allWheels)
        {
            wheel.wCollider.brakeTorque = Mathf.Infinity;

        }
        transform.position += Vector3.up * 2f;
        transform.rotation = Quaternion.LookRotation(transform.forward);
    }

    private void GroundWheels(WheelCollider leftWheel, WheelCollider rightWheel)
    {
        WheelHit hit;

        float leftTravel = 1f, rightTravel = 1f;

        bool leftGrounded = leftWheel.GetGroundHit(out hit);

        //Comprobar si estamos hundidos en el suelo, lo cual implicaría que tenemos demasiada fuerza aplicada sobre una rueda

        if (leftGrounded)
        {
            leftTravel = (-leftWheel.transform.InverseTransformPoint(hit.point).y - leftWheel.radius) / leftWheel.suspensionDistance;
        }

        bool rightGrounded = rightWheel.GetGroundHit(out hit);

        if (leftGrounded)
        {
            rightTravel = (-rightWheel.transform.InverseTransformPoint(hit.point).y - rightWheel.radius) / rightWheel.suspensionDistance;
        }

        //Si estamos hundidos, lo que queremos es aplicar fuerza sobre el otro lado del coche para bajarlo, como si le metieramos más peso

        float antiRollForce = (leftTravel - rightTravel) * antiRoll;

        if (leftGrounded)
        {
            rb.AddForceAtPosition(leftWheel.transform.up * -antiRollForce, leftWheel.transform.position);
        }

        if (rightGrounded)
        {
            rb.AddForceAtPosition(rightWheel.transform.up * antiRollForce, rightWheel.transform.position);
        }
    }

    private void CheckSkid()
    {
        int wheelsSkidding = 0;
        WheelHit wheelHit;

        for (int i = 0; i < allWheels.Length; i++)
        {
            allWheels[i].wCollider.GetGroundHit(out wheelHit);

            float forwardSlip = Mathf.Abs(wheelHit.forwardSlip);
            float sidewaysSlip = Mathf.Abs(wheelHit.sidewaysSlip);


            if (forwardSlip >= skidThreshold ||
                sidewaysSlip >= skidThreshold)
            {
                wheelsSkidding++;
                skidValues[i] = forwardSlip + sidewaysSlip;

                // Skid smoke particles
                skidSmokes[i].transform.position =
                    allWheels[i].wCollider.transform.position -
                    allWheels[i].wCollider.transform.up *
                    allWheels[i].wCollider.radius
                    ;
                skidSmokes[i].Emit(1);
            }
            else
                skidValues[i] = 0f;
        }

        //skidding sound
        if (wheelsSkidding == 0 && skidSound.isPlaying)
        {
            skidSound.Stop();
        }
        else if (wheelsSkidding > 0)
        {
            //Update the drift sound
            skidSound.volume = (float)wheelsSkidding / allWheels.Length;

            skidSound.panStereo = (-skidValues[0] + skidValues[1] + skidValues[2] - skidValues[3]) * stereoFactor;

            if (!skidSound.isPlaying)
                skidSound.Play();
        }

    }

    private void CalculateEngineSound()
    {
        float speedProp = actualSpeed / maxSpeed;
        engineSound.pitch = Mathf.Lerp
            (
            engineSoundMinPitch,
            engineSoundMaxPitch,
            engineSoundCurve.Evaluate(speedProp)
            );
    }

    void AntiTipSystem()
    {
        // Rotation ==> UP
        Vector3 eulerRotation = this.transform.rotation.eulerAngles;
        this.transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0);

        Vector3 position = this.transform.position;
        this.transform.position = new Vector3(position.x, position.y + 0.5f, position.z);
    }
    #endregion

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Waypoint")
        {         
            if (other.gameObject.GetComponent<WaypointID>().ID == stats.Wp_Target)
            {
                Check_WaypointLogic();
            }
        }
    }

    /// <summary>
    ///     Waypoint logic & lap summation.
    /// </summary>
    void Check_WaypointLogic()
    {
        // Win => Last waypoint
        if (stats.lap == circuitMng.maxLap && stats.Wp_Target == 0 && stats.Wp_TotalSurpassed != 0)
        {
            stats.Wp_Target++;
            stats.finishedRace = true;
            stats.Wp_TotalSurpassed++;
            circuitMng.Check_Ranking();
            transform.gameObject.SetActive(false);
            return;
        }

        // Last waypoint in lap
        int circuitLastWPNumber = circuitMng.waypoints.Length - 1;
        if (stats.Wp_Target >= circuitLastWPNumber)
        {
            stats.Wp_Target = 0;
            stats.Wp_startCounting = true;
            Check_LastWp();
            stats.Wp_TotalSurpassed++;
            circuitMng.Check_Ranking();
            return;
        }
        // General waypoints in race
        else
        {
            stats.Wp_Target++;
            Check_LastWp();
            if (stats.Wp_startCounting == true)
                Add_Lap();
            stats.Wp_TotalSurpassed++;
            circuitMng.Check_Ranking();
        }
    }

    /// <summary>
    ///     Lap summation logic.
    /// </summary>
    void Add_Lap()
    {
        if (stats.lap <  circuitMng.maxLap) stats.lap++;
        else                                stats.lap = circuitMng.maxLap;
    }
    /// <summary>
    ///     Last WayPoint summation logic.
    /// </summary>
    void Check_LastWp()
    {
        if (stats.Wp_Target > 0)              stats.Wp_Previous = stats.Wp_Target - 1;
        // Last WayPoint in the list
        if (stats.Wp_Target == 0)             stats.Wp_Previous++;
    }

    void OnDrawGizmos()
    {
        for (int i = 0; i < skidValues.Length; i++)
        {
            if (allWheels[i])
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(allWheels[i].transform.position, skidValues[i]);
            }
        }
    }
}
