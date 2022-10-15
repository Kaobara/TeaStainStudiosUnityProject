using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [SerializeField] Rigidbody rigidBody;
    private bool isColliding;

    // Start is called before the first frame update
    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {

    }

    private void FixedUpdate() {
        if (transform.parent != null) {
            Transform parentTrans = transform.parent.transform.Find("PlayerOrientation");
            transform.position = parentTrans.position + parentTrans.forward * transform.parent.GetComponent<PlayerControl>().GetHoldDistance();
        }        
    }

    public void Attach(GameObject newParent)
    {
        transform.SetParent(newParent.transform);
        rigidBody.useGravity = false;
    }

    public void Detach(Vector3 playerOrientation, float force)
    {
        transform.parent = null;
        rigidBody.useGravity = true;
        rigidBody.AddForce(Vector3.RotateTowards(playerOrientation, Vector3.up, Mathf.PI/4, 10000) * force, ForceMode.Impulse);
    }

    public void OnCollisionEnter(Collision other) {
        isColliding = true;
    }

    public void OnCollisionExit(Collision other) {
        isColliding = false;
    }
}
