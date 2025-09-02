using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PartidaButton : MonoBehaviour
{
    public string fechaPartida;
    public int fasesCompletadas;
    public int nivelesCompletados;
    public int escenaADondeIr = 4;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    // ✅ Recibe fecha, fases, niveles y escena
    public void Configurar(string fecha, int fases, int niveles, int escena)
    {
        fechaPartida = fecha;
        fasesCompletadas = fases;
        nivelesCompletados = niveles;
        escenaADondeIr = escena;

        // ✅ Mostrar solo fases y niveles en el botón
        GetComponentInChildren<TextMeshProUGUI>().text = $"Fases: {fases} | Niveles: {niveles}";
    }

    public void OnClick()
    {
        string nombreGuardado = PlayerPrefs.GetString("nombreJugador", "");

        PlayerPrefs.SetString("fechaSeleccionada", fechaPartida);
        PlayerPrefs.SetInt("fasesSeleccionadas", fasesCompletadas);
        PlayerPrefs.SetInt("nivelesSeleccionados", nivelesCompletados);
        PlayerPrefs.Save();

        SceneManager.LoadScene(escenaADondeIr);
    }
}
