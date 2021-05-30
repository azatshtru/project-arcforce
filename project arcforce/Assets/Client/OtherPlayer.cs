using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherPlayer : MonoBehaviour
{
    string playerName;

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
        transform.position = pos;
    }
}
