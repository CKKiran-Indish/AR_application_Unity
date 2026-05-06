using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[Serializable]
public class MachineData
{
    public string machineName;
    public string status;
    public JObject parameters;   
}
[Serializable]
public class HealthResponse
{
    public bool status;
}
public class APIManager : MonoBehaviour
{
    public static Action<MachineData> GotApiData;
    public static Action<string> OnApiFailed;   // New: notifies UIManager on failure

    public string address = "http://192.168.40.30";

    [SerializeField] public int port = 8000;
    [SerializeField] public bool isConnected = false;
    [SerializeField] GameObject connetedStatus;
    [SerializeField] GameObject notConnectedStatus;

    void OnEnable()
    {
        // GetQRpose.OnQRDetected += OnQRCodeScanned;
        ConnectBtn.OnConnectFromPanel2 +=GotAddress;
    }

    void OnDisable()
    {
        // GetQRpose.OnQRDetected -= OnQRCodeScanned;
        ConnectBtn.OnConnectFromPanel2 -=GotAddress;
    }
    void GotAddress(string newAddress)
    {
        address= newAddress;
        CheckHealth();

    }

   public void OnQRCodeScanned(string qrCodeData, Vector3 worldPos, Quaternion worldRot)
    {
        Debug.Log("QR Code Scanned: " + qrCodeData);
        string url = $"{address}:{port}/data?qr=" + UnityWebRequest.EscapeURL(qrCodeData);
        StartCoroutine(GetRequest(url));
    }
    void CheckHealth()
    {
        string url =$"{address}:{port}/health";
        StartCoroutine(GetHealthRequest(url, (status) =>
        {
            if (status){
                Debug.Log(" Healthy");
                connetedStatus.SetActive(true);
                notConnectedStatus.SetActive(false);
            }

            else{
                    Debug.Log(" Not Healthy");
                    notConnectedStatus.SetActive(true);
                    connetedStatus.SetActive(false);
                }
        }));

        
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Timeout after 10 seconds so the loader doesn't hang forever
            webRequest.timeout = 10;

            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Error: " + webRequest.error);
                OnApiFailed?.Invoke(webRequest.error);   // Tell UIManager it failed
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            Debug.Log("Received: " + jsonResponse);

            MachineData machineData = JsonConvert.DeserializeObject<MachineData>(jsonResponse);
            GotApiData?.Invoke(machineData);
        }
    }
    IEnumerator GetHealthRequest(string uri, Action<bool> onResult)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            webRequest.timeout = 10;
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Health API Error: " + webRequest.error);
                onResult?.Invoke(false);
                yield break;
            }

            string jsonResponse = webRequest.downloadHandler.text;
            HealthResponse response = JsonConvert.DeserializeObject<HealthResponse>(jsonResponse);
            bool isHealthy = response.status;

            onResult?.Invoke(isHealthy);
        }
    }
}