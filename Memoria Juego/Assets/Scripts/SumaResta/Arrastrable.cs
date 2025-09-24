using System.Collections.Generic;    
using UnityEngine;                   
using UnityEngine.UI;                
using TMPro;                         
using UnityEngine.EventSystems;      

public enum ModoOperacion
{
    Suma,
    Resta
}

[System.Serializable]
public class DropSlotConfig
{
    public Button slot;
    public ModoOperacion modo; // 👈 Cada slot puede ser suma o resta
}

public class Arrastrable : MonoBehaviour
{
    [Header("Arrastrables con valores")]
    public List<Image> draggableImages;
    public List<int> draggableValues;

    [Header("Slots receptores con configuración")]
    public List<DropSlotConfig> dropSlots; // 👈 ahora cada slot tiene su propio modo

    [Header("UI")]
    public TextMeshProUGUI totalText;

    private Canvas canvas;
    private Image draggingImage;
    private RectTransform draggingRect;
    private Transform originalParent;
    private Vector2 originalPosition;

    private int currentTotal = 0;

    private Dictionary<Button, GameObject> slotClones = new Dictionary<Button, GameObject>();
    private Dictionary<Button, int> slotValues = new Dictionary<Button, int>();

    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();

        foreach (var config in dropSlots)
        {
            slotClones[config.slot] = null;
            slotValues[config.slot] = 0;

            AddDropEvents(config.slot, config.modo);
            AddButtonDestroyListener(config.slot, config.modo);
        }

        foreach (var img in draggableImages)
            AddDragEvents(img);

        UpdateTotalText();
    }

    void AddDragEvents(Image img)
    {
        EventTrigger trigger = img.gameObject.AddComponent<EventTrigger>();

        // BeginDrag
        var begin = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        begin.callback.AddListener((data) =>
        {
            draggingImage = img;
            draggingRect = img.rectTransform;
            originalParent = img.transform.parent;
            originalPosition = draggingRect.anchoredPosition;

            var cg = img.GetComponent<CanvasGroup>();
            if (cg == null) cg = img.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
        });
        trigger.triggers.Add(begin);

        // Drag
        var drag = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        drag.callback.AddListener((data) =>
        {
            if (draggingRect != null)
            {
                var ped = (PointerEventData)data;
                draggingRect.anchoredPosition += ped.delta / canvas.scaleFactor;
            }
        });
        trigger.triggers.Add(drag);

        // EndDrag
        var end = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        end.callback.AddListener((data) =>
        {
            if (draggingRect != null)
            {
                var cg = draggingImage.GetComponent<CanvasGroup>();
                if (cg != null) cg.blocksRaycasts = true;

                draggingImage.transform.SetParent(originalParent);
                draggingRect.anchoredPosition = originalPosition;
            }

            draggingImage = null;
            draggingRect = null;
        });
        trigger.triggers.Add(end);
    }

    void AddDropEvents(Button slot, ModoOperacion modo)
    {
        EventTrigger trigger = slot.gameObject.AddComponent<EventTrigger>();

        var drop = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
        drop.callback.AddListener((data) =>
        {
            if (draggingImage != null)
            {
                int index = draggableImages.IndexOf(draggingImage);
                int value = (index >= 0 && index < draggableValues.Count) ? draggableValues[index] : 0;

                if (slotClones[slot] != null)
                {
                    // Revertir el valor anterior según modo
                    if (modo == ModoOperacion.Suma) currentTotal -= slotValues[slot];
                    else currentTotal += slotValues[slot];

                    Destroy(slotClones[slot]);
                }

                GameObject copia = Instantiate(draggingImage.gameObject, slot.transform);
                var copiaRect = copia.GetComponent<RectTransform>();
                copiaRect.sizeDelta = draggingRect.sizeDelta;
                copiaRect.localScale = Vector3.one;
                copiaRect.anchoredPosition = Vector2.zero;

                Destroy(copia.GetComponent<EventTrigger>());
                Destroy(copia.GetComponent<CanvasGroup>());

                slotClones[slot] = copia;
                slotValues[slot] = value;

                if (modo == ModoOperacion.Suma) currentTotal += value;
                else currentTotal -= value;

                UpdateTotalText();
            }
        });
        trigger.triggers.Add(drop);
    }

    void AddButtonDestroyListener(Button slot, ModoOperacion modo)
    {
        slot.onClick.AddListener(() =>
        {
            if (slotClones.ContainsKey(slot) && slotClones[slot] != null)
            {
                if (modo == ModoOperacion.Suma) currentTotal -= slotValues[slot];
                else currentTotal += slotValues[slot];

                UpdateTotalText();

                Destroy(slotClones[slot]);
                slotClones[slot] = null;
                slotValues[slot] = 0;
            }
        });
    }

    void UpdateTotalText()
    {
        if (totalText != null)
        {
            // Mostrar siempre el total
            totalText.text = "Total: " + currentTotal;
        }
    }

    public int GetCurrentTotal()
    {
        return currentTotal;
    }
    public void SetInitialTotal(int total)
    {
        currentTotal = total;

        if (totalText != null)
            totalText.text = "Total: " + total;  // forzar mostrar
    }
    public void LimpiarSlots()
    {
        // Crear una lista de claves para recorrer sin modificar el diccionario directamente
        List<Button> keys = new List<Button>(slotClones.Keys);

        foreach (var key in keys)
        {
            if (slotClones[key] != null)
            {
                Destroy(slotClones[key]);
            }
            slotClones[key] = null;
            slotValues[key] = 0;
        }

        currentTotal = 0; // reiniciar total
        UpdateTotalText();
    }
    public bool UsaSlotDeTipo(ModoOperacion modo)
    {
        foreach (var kvp in slotClones)
        {
            if (kvp.Value != null)
            {
                var config = dropSlots.Find(c => c.slot == kvp.Key);
                if (config != null && config.modo == modo)
                    return true;
            }
        }
        return false;
    }

}
