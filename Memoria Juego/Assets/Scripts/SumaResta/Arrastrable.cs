using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;   // 👈 Importante para usar TextMeshPro
using System.Collections.Generic;

public class Arrastrable : MonoBehaviour
{
    [Header("Arrastrables con valores")]
    public List<Image> draggableImages;   // Imágenes que se pueden arrastrar
    public List<int> draggableValues;     // Valores numéricos asociados a cada imagen

    [Header("Slots receptores (Botones)")]
    public List<Button> dropSlots;        // Slots que son botones

    [Header("UI")]
    public TextMeshProUGUI totalText;     // Texto donde se muestra la suma total

    private Canvas canvas;
    private Image draggingImage;
    private RectTransform draggingRect;
    private Transform originalParent;
    private Vector2 originalPosition;

    private int currentTotal = 0; // Total acumulado de valores

    // Guardar la copia creada en cada slot y su valor
    private Dictionary<Button, GameObject> slotClones = new Dictionary<Button, GameObject>();
    private Dictionary<Button, int> slotValues = new Dictionary<Button, int>();

    void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();

        // Inicializar diccionarios
        foreach (var slot in dropSlots)
        {
            slotClones[slot] = null;
            slotValues[slot] = 0;
        }

        // Configurar arrastrables
        foreach (var img in draggableImages)
        {
            AddDragEvents(img);
        }

        // Configurar slots
        foreach (var slot in dropSlots)
        {
            AddDropEvents(slot);
            AddButtonDestroyListener(slot);
        }

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

                // Siempre vuelve a la posición original
                draggingImage.transform.SetParent(originalParent);
                draggingRect.anchoredPosition = originalPosition;
            }

            draggingImage = null;
            draggingRect = null;
        });
        trigger.triggers.Add(end);
    }

    void AddDropEvents(Button slot)
    {
        EventTrigger trigger = slot.gameObject.AddComponent<EventTrigger>();

        // Drop
        var drop = new EventTrigger.Entry { eventID = EventTriggerType.Drop };
        drop.callback.AddListener((data) =>
        {
            if (draggingImage != null)
            {
                int index = draggableImages.IndexOf(draggingImage);
                int value = (index >= 0 && index < draggableValues.Count) ? draggableValues[index] : 0;

                // Si ya hay un clon en el slot, eliminarlo y restar su valor
                if (slotClones[slot] != null)
                {
                    currentTotal -= slotValues[slot];
                    Destroy(slotClones[slot]);
                }

                // Crear una copia del arrastrable en el slot
                GameObject copia = Instantiate(draggingImage.gameObject, slot.transform);
                var copiaRect = copia.GetComponent<RectTransform>();

                // Igualar tamaño y escala al original
                copiaRect.sizeDelta = draggingRect.sizeDelta;
                copiaRect.localScale = Vector3.one;
                copiaRect.anchoredPosition = Vector2.zero;

                // La copia no debe ser arrastrable
                Destroy(copia.GetComponent<EventTrigger>());
                Destroy(copia.GetComponent<CanvasGroup>());

                // Guardar referencia y valor
                slotClones[slot] = copia;
                slotValues[slot] = value;

                // Sumar al total
                currentTotal += value;
                UpdateTotalText();
            }
        });
        trigger.triggers.Add(drop);
    }

    void AddButtonDestroyListener(Button slot)
    {
        slot.onClick.AddListener(() =>
        {
            if (slotClones.ContainsKey(slot) && slotClones[slot] != null)
            {
                // Restar el valor antes de destruir
                currentTotal -= slotValues[slot];
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
            totalText.text = "Total: " + currentTotal;
    }
    
    public int GetCurrentTotal()
    {
        return currentTotal;
    }
}