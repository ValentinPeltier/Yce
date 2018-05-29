using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    [SerializeField] private GameObject display;

    // ------------

    public void OnPointerEnter(PointerEventData eventData) {
        display.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData) {
        display.SetActive(false);
    }

    // -----------

    private void Awake() {
        display.SetActive(false);
    }
}
