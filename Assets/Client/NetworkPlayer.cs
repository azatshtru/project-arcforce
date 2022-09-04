using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviour
{
    public float syncInterval = 0.25f;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("SendPosition");
    }

    IEnumerator SendPosition()
    {
        while (true)
        {
            yield return new WaitForSeconds(syncInterval);
            Client.Instance.SendVector(transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
