using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerOnlineController : MonoBehaviour
{
    public string onlineName;

    [Header("References")]
    [SerializeField] Competitor_Info    competitorInfo;
    [SerializeField] OnlineManager      onlineManager;

    [Header("Stats")]
    public  float timeToUpdatePosition      = 0.5f;
    private float timeToUpdatePositionAux   = 0.5f;
    public  float timeToUpdateRotation      = 0.5f;
    private float timeToUpdateRotationAux   = 0.5f;


    void Awake()
    {
        onlineManager   = GetComponent<OnlineManager>();
        competitorInfo  = GetComponent<Competitor_Info>();

        if (onlineManager == null)
            onlineManager = GameObject.Find("Manager_Online").GetComponent<OnlineManager>();
    }

    void Start()
    {
        //onlineName = gameObject.name;
        //competitorInfo.nickname = onlineName;
    }

    void LateUpdate()
    {
        if (onlineManager != null)
        {
            timeToUpdatePositionAux -= Time.deltaTime;
            timeToUpdateRotationAux -= Time.deltaTime;

            if (timeToUpdatePositionAux <= 0f)
            {
                onlineManager.UpdatePosition();
                timeToUpdatePositionAux = timeToUpdatePosition;
            }

            if (timeToUpdateRotationAux <= 0f )
            {
                onlineManager.UpdateRotation();
                timeToUpdateRotationAux = timeToUpdateRotation;
            }
        }
    }
}
