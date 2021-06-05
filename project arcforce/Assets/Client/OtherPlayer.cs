using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    public float lagDistance = 20f;

    [SerializeField]
    private float stepSpeed = 20f;

    string playerName;

    Vector3 recievedPosition;

    public string GetOtherName()
    {
        return playerName;
    }

    public void SetOtherName (string _name)
    {
        playerName = _name;
    }

    public void SetOtherPosition(Vector3 pos)
    {
        recievedPosition = pos;
    }

    private void Update()
    {
        if((recievedPosition - transform.position).sqrMagnitude > lagDistance)
        {
            transform.position = recievedPosition;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, recievedPosition, stepSpeed * Time.deltaTime);
        }
    }
}
