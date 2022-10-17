using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    [SerializeField] private LevelsController lc;

    // Start is called before the first frame update
    void Start()
    {
        lc = GameObject.Find("LevelController").GetComponent<LevelsController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCollisionEnter(Collision other) {
        string objName = other.gameObject.name;
        Debug.Log(objName);
        if (objName == "Player" || objName == "Package") {
            lc.RestartLevel();
        }
    }
}
