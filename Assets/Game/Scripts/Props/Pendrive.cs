using NuevaAndinia.Controller;
using NuevaAndinia.Core;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class Pendrive : MonoBehaviour
{
    private bool agarrado = false;
    private void Start()
    {
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (!agarrado && player.CanUsePendrive())
        {
            agarrado = true;
            player.RecogerPendrive(); // Agarro el pendrive
            Debug.Log("Pendrive recogido exitosamente.");
            //Destroy(gameObject);          // Destruccion del pendrive de escena
        }
    }


    /*public void Interact(PlayerController player)
    {
        // Validamos si el jugador tiene agua lista para usar
        if (!agarrado && player.CanUsePendrive())
        {
            agarrado=true;
            player.RecogerPendrive(); // Agarro el pendrive
            Debug.Log("Pendrive recogido exitosamente.");
            Destroy(gameObject);          // Destruccion del pendrive de escena
        }
    }*/
}
