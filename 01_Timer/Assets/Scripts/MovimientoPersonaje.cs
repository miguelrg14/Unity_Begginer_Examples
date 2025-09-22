using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovimientoPersonaje : MonoBehaviour
{
    public float velocidad = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(0, 0, 0.5f); // Nos movemos en el eje z
        transform.Translate
            (
            Input.GetAxis("Horizontal") + Time.deltaTime * velocidad,       // X
            0,                                                              // Y
            Input.GetAxis("Vertical") + Time.deltaTime * velocidad          // Z
            );
    }
}
