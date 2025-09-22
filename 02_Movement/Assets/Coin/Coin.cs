using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coin : MonoBehaviour
{
    public int valor;
    public static int coinsCount = 0;
    public float velocidad = 15;

    public Text text;
    public GameObject monedo;


    // Start is called before the first frame update
    void Start()
    {
        Coin.coinsCount++;
        Debug.Log("Comienza el juego y ahora hay " + Coin.coinsCount + "monedas");

        monedo = GameObject.FindGameObjectWithTag("Text");
        text = monedo.GetComponent<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(velocidad * Time.deltaTime, 0, 0); // La moneda rota sobre su eje x
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("¡Colisión detectada!");

        if (other.CompareTag("Player") == true)
        {
            Coin.coinsCount--;

            Debug.Log("¡Moneda recogida!, restantes: " + coinsCount);

            if (coinsCount <= 0)
            {
                Debug.Log("Has ganado!");
            }

            Destroy(this.gameObject);
            text.text = Convert.ToString(coinsCount);
        }
    }
}
