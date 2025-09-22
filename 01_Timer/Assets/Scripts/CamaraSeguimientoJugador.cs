using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamaraSeguimientoJugador : MonoBehaviour
{
    public Transform Player;

    Vector3 vel = Vector3.zero;

    public float Tiemsua = .15f;

    public GameObject Jugador;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 Pospla = Player.position;
        Pospla.x = transform.position.x;

        transform.position = Vector3.SmoothDamp(transform.position, Pospla, ref vel, Tiemsua);
    }
}
