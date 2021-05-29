using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClientUI : MonoBehaviour
{
    public GameObject clientUi;
    public InputField nameInputField;

    private void Start()
    {
        Client.onPlayerSpawned += DisableClientUI;
    }

    public void GetIPAddress()
    {
        Client.Instance.SetName(nameInputField.text);
    }

    public void DisableClientUI()
    {
        clientUi.SetActive(false);
    }

    public void EnableClientUI()
    {
        clientUi.SetActive(true);
    }
}
