using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Mob {
    public GameObject gameObject;
}

public class MobManager : MonoBehaviour {

    [SerializeField] private GameObject[] mobPrefabs;

    private List<Mob> mobs = new List<Mob>();

    private World world;

    // ----------------

    private void Awake() {
        world = GameObject.Find("World").GetComponent<World>();

        mobPrefabs = world.mobPrefabs;
    }

}
