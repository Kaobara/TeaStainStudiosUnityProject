using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawner : MonoBehaviour
{
    [SerializeField] private Transform spawn;
    [SerializeField] private Vector3 spawnPos;
    [SerializeField] private Quaternion spawnRot;
    [SerializeField] private Vector3 spawnScale;
    [SerializeField] Rigidbody rigidBody;

    // Start is called before the first frame update
    void Start()
    {
        spawnPos = transform.position;
        spawnRot = transform.rotation;
        spawnScale = transform.localScale;
        rigidBody = transform.gameObject.GetComponent<Rigidbody>();
    }

    public void Respawn()
    {
        transform.position = spawnPos;
        transform.rotation = spawnRot;
        transform.localScale = spawnScale;
        rigidBody.velocity = Vector3.zero;
    }
}
