using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField] public Rigidbody rigidBody {get; private set; }
    [SerializeField] public Vector3 localPos  {get; private set; }
    [SerializeField] public float maxLocalDiffSq {get; private set; }

    // Start is called before the first frame update
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void Attach(GameObject newParent)
    {
        transform.SetParent(newParent.transform.Find("PlayerOrientation"));
        rigidBody.useGravity = false;
        localPos = transform.parent.forward * transform.parent.GetComponent<PlayerControl>().GetHoldDistance();   
        transform.localPosition = localPos;   
    }

    public void Eject(Vector3 playerOrientation, float force)
    {
        transform.parent = null;
        rigidBody.useGravity = true;
        rigidBody.AddForce(Vector3.RotateTowards(playerOrientation, Vector3.up, Mathf.PI/4, 10000) * force, ForceMode.Impulse);
    }

    public void Detach()
    {
        transform.parent = null;
        rigidBody.useGravity = true;
    }
}
