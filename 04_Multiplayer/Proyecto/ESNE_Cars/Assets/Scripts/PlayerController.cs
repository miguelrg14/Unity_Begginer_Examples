using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CarController))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] CircuitMng circuitMng;
    [SerializeField] CarController car;
    [SerializeField] Competitor_Info stats;
    [SerializeField] CanvasManager canvasManager;

    [Header("Canvas Info to update")]
    [SerializeField] TMP_Text actualPosition_txt;
    [SerializeField] TMP_Text maxPosition_txt;
    [SerializeField] TMP_Text actualLap_txt;
    [SerializeField] TMP_Text maxLap_txt;

    void Awake()
    {
        car = GetComponent<CarController>();
        stats = GetComponent<Competitor_Info>();

        if (canvasManager == null)
            canvasManager = GameObject.Find("Canvas").GetComponent<CanvasManager>();

        if (circuitMng == null)
            circuitMng = GameObject.Find("Manager_Circuit").GetComponent<CircuitMng>();
    }
    void Start()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        maxLap_txt.text         = circuitMng.maxLap.ToString();
        maxPosition_txt.text    = circuitMng.competitors_Total.ToString();
        canvasManager.Check_MaxCompetitors();
    }
    void FixedUpdate()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        float torqueInput   = Input.GetAxis("Vertical"  );
        float brakeInput    = Input.GetAxis("Jump"      );
        float steeringInput = Input.GetAxis("Horizontal");

        // Torque
        car.ApplyTorque(torqueInput);

        // Brake
        car.ApplyBrake(brakeInput);

        // Steering
        car.ApplySteering(steeringInput);
    }
    void Update()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            car.ResetCar();
        }
    }

    public void Set_Canvas_Lap()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        if (actualLap_txt != null)
            actualLap_txt.text = stats.lap.ToString();
    }
    public void Set_Canvas_Position()
    {
        if (transform.GetComponent<OnlineCarController>() != null)
            return;

        if (actualPosition_txt != null)
            actualPosition_txt.text = stats.position.ToString();
    }
}
