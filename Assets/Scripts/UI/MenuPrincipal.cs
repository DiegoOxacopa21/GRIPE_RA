using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    // Método llamado por el botón 1
    public void AbrirEscena1()
    {
        SceneManager.LoadScene("PRUEBAS");
    }

    // Método llamado por el botón 2
    public void AbrirEscena2()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
