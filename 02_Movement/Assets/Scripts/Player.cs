using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public Text txtMonedasRecogidas;
    private int monedasRecogidas;

    float velocidad = 0;
    public float fuerzaSalto = 100f;
    // Start is called before the first frame update
    void Start()
    {
        monedasRecogidas = 0;
        //txtMonedasRecogidas.text = monedasRecogidas.ToString();
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
        if (Input.GetButtonDown("Jump"))
        {
            GetComponent<Rigidbody>().AddForce(Vector3.up * fuerzaSalto);
        }
    }

    public void SumarMonedas()
    {
        monedasRecogidas++;
        txtMonedasRecogidas.text = monedasRecogidas.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == true)
        {
            Coin.coinsCount--;

            other.GetComponent<Player>().SumarMonedas();
        }
    }
}
