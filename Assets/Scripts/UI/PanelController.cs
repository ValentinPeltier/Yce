using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PanelController : MonoBehaviour {

    protected bool isLoaded = false;
    protected MainMenu mainMenu;

    protected abstract void LoadPanel();
    protected abstract void ResetPanel();
    public abstract void OnOpen();

}
