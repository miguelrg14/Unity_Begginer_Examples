using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLogic : MonoBehaviour
{
    // Con "velocidadActual" recojo la velocidad q uso en cada momento, para poder variar entre la
    //      velocidad normal == "velocidadMovimiento"
    //      correr           == "velocidadCorrer"
    public float velocidadActual;
    public float velocidadMovimiento = 5.0f;
    public float velocidadCorrer = 10.0f;
    public float velocidadRotacion = 200.0f;
    // Variable para sacar el componente "animator" del personaje
    private Animator anim;

    //  Uso estas variables "x" e "y" para coger la velocidad para...
    //      x == rotar la cámara al darle a "A" o "D"
    //      y == moverse hacia delante
    //  Tmb las uso en el blend tree del "playerController" para según la posición en estos ejes q haga diferentes animaciones
    public float x, y;

    // Control d caida y salto de personaje
    public Rigidbody rb;
    public float fuerzaSalto = 8.0f;
    public bool puedoSaltar;

    // Start is called before the first frame update
    void Start()
    {
        puedoSaltar = false;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // Hago q el personaje rote
        transform.Rotate(0, x * Time.deltaTime * velocidadRotacion, 0);
        // Hago q se desplace
        transform.Translate(0, 0, y * Time.deltaTime * velocidadActual);


        // Cojo el input de los ejes horizontal y vertical con las variables "x" e "y"
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");

        // Hago q las animaciones de movimiento funcionen
        anim.SetFloat("VelX", x);
        anim.SetFloat("VelY", y);

        // Código Salto
        if(puedoSaltar == true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // Añado la fuerza de impulso en el eje vertical
                rb.AddForce(new Vector3(0, fuerzaSalto, 0), ForceMode.Impulse);
            }
            // Pongo como true q toca el suelo para q deje d hacer la animación d salto
            anim.SetBool("TocoSuelo", true);
        }
        else
        {
            EstoyCayendo();
        }

        //Animación correr cuando presiono shift y el jugador está tocando el suelo ("puedoSaltar" == true)
        if (Input.GetKey(KeyCode.LeftShift) && puedoSaltar == true)
        {
            velocidadActual = velocidadCorrer;

            if (y > 0)
            {
                anim.SetBool("Correr", true);
            }
            // Por si presiono shift estando quieto
            else
            {
                anim.SetBool("Correr", false);
            }
        }
        else
        {
            anim.SetBool("Correr", false);
            velocidadActual = velocidadMovimiento;
        }
    }

    // Mediante el capsule collider de los pies del personaje q detecta si toca el suelo y este método
    //  pruebo si toca el suelo y activo la animación de salto o no
    public void EstoyCayendo()
    {
        anim.SetBool("TocoSuelo", false);
    }
}
