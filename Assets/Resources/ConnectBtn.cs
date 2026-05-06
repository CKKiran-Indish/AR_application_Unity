using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;


public class ConnectBtn : MonoBehaviour
{
    public static Action<string> OnConnectFromPanel2;
    [SerializeField] private GameObject connectionPanel;
    [SerializeField] private GameObject connectedStatus;
    [SerializeField] private GameObject notconnectStatus;
    [SerializeField] private TMP_InputField source;

    void OnEnable()
    {
       if(connectionPanel) connectionPanel.SetActive(false);
       if(connectedStatus) connectedStatus.SetActive(false);
       if(notconnectStatus) notconnectStatus.SetActive(false);
    }
    void OnDisable()
    {
        if(connectionPanel) connectionPanel.SetActive(false);
       if(connectedStatus) connectedStatus.SetActive(false);
       if(notconnectStatus) notconnectStatus.SetActive(false);
    }
    
    public void OnConnectBtnFromPanel1()
    {
        connectionPanel.SetActive(true);

    }
    public void OnconneectBtnFromPanel2()
    {
        string address = source.text;
        if(address != "")
        {
            OnConnectFromPanel2?.Invoke(address);
        }
    }

    public void OnDoneConnection()
    {
        connectionPanel.SetActive(false);
    }

}