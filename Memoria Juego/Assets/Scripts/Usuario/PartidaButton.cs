using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PartidaButton : MonoBehaviour
{
    public string fechaPartida;
    public int escenaADondeIr = 4; // cambia este índice por el de tu escena real

    private void Start()
    {
        // Asignar el listener si aún no lo haces desde el generador
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void Configurar(string fecha, int escena)
    {
        fechaPartida = fecha;
        escenaADondeIr = escena;
        GetComponentInChildren<TextMeshProUGUI>().text = fecha;
    }

    public void OnClick()
    {
        string nombreGuardado = PlayerPrefs.GetString("nombreJugador", "");
        Debug.Log("Nombre guardado actualmente: " + nombreGuardado);

        PlayerPrefs.SetString("fechaSeleccionada", fechaPartida);
        PlayerPrefs.Save();

        Debug.Log("Fecha guardada: " + fechaPartida);
        SceneManager.LoadScene(escenaADondeIr);
    }



}
