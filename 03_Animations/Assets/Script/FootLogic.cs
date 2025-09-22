using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootLogic : MonoBehaviour
{
    public PlayerLogic logicPlayer;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // El personaje está en el suelo
    private void OnTriggerStay(Collider other)
    {
        logicPlayer.puedoSaltar = true;
    }
    // El personaje está en el aire
    private void OnTriggerExit(Collider other)
    {
        logicPlayer.puedoSaltar = false;
    }
}
