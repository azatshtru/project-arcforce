using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float destroyAfter = 2f;

    private Rigidbody rb;

    private GameObject owner;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(transform.forward * 20f, ForceMode.VelocityChange);

        Invoke(nameof(DestroySelf), destroyAfter);
    }

    void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void SetOwner(GameObject _owner)
    {
        owner = _owner;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject != owner)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                if (other.GetComponent<NetworkPlayer>())
                {
                    other.GetComponent<Health>().TakeDamage();
                }
            }
            Destroy(gameObject);
        }
    }
}
