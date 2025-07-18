using UnityEngine;
using System.Collections.Generic;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance;

    // Índices de niveles desbloqueados (puedes usar nombres si prefieres)
    public List<int> nivelesDesbloqueados = new List<int> { 2 }; // Solo el nivel 1 (index 2) desbloqueado

    private void Awake()
    {
        // Singleton simple para acceso global en memoria
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: para conservar entre escenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool EstaDesbloqueado(int sceneIndex)
    {
        return nivelesDesbloqueados.Contains(sceneIndex);
    }

    public void DesbloquearNivel(int sceneIndex)
    {
        if (!nivelesDesbloqueados.Contains(sceneIndex))
        {
            nivelesDesbloqueados.Add(sceneIndex);
        }
    }
}
