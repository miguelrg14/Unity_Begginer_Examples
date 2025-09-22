using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Timer : MonoBehaviour
{
    private float maxTime = 5f;
    private float countdown = 0f;

    // Start is called before the first frame update
    void Start()
    {
        countdown = maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        countdown -= Time.deltaTime;
        Debug.Log("Tiempo restante: " + countdown);

        if(countdown <= 0)
        {
            SceneManager.LoadScene("Scene2");
        }
    }
}
