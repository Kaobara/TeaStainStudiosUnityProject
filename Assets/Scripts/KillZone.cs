using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillZone : MonoBehaviour
{
    public void OnCollisionEnter(Collision other) {
        Respawner respawner = other.gameObject.GetComponent<Respawner>();
        if (respawner != null) {
            respawner.Respawn();
        }
    }
}
