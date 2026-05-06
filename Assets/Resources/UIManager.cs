using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    [Header("--- UI REFERENCES ---")]
    public Transform content;
    public GameObject Prefab;
    public GameObject loadingPanel;

    [Header("--- WORLD SPACE CANVAS ---")]
    public GameObject uiRootCanvas;

    [Header("--- OPTIONAL REFERENCES ---")]
    public GetQRpose qrScanner;
    public ARRaycastManager arRaycastManager;
    public TextMeshProUGUI errorText;

    private void OnEnable()
    {
        APIManager.GotApiData += PopulateUI;
        APIManager.OnApiFailed += OnApiFailed;

        loadingPanel.SetActive(false);
        uiRootCanvas.SetActive(false);
    }

    private void OnDisable()
    {
        APIManager.GotApiData -= PopulateUI;
        APIManager.OnApiFailed -= OnApiFailed;
    }

    // ✅ Called from ARPlanePlacement
    public void ShowUI(Vector3 position, Quaternion rotation)
    {
        uiRootCanvas.transform.position = position;
        uiRootCanvas.transform.rotation = rotation;

        uiRootCanvas.SetActive(true);
        loadingPanel.SetActive(false);

        if (arRaycastManager != null)
            arRaycastManager.enabled = false;

        if (qrScanner != null)
            qrScanner.PauseScanning();
    }

    void PopulateUI(MachineData machineData)
    {
        Debug.Log("UI RECEIVED DATA");

        loadingPanel.SetActive(true);

        foreach (Transform child in content)
            Destroy(child.gameObject);

        CreateItem("Machine", machineData.machineName);
        CreateItem("Status", machineData.status);

        if (machineData.parameters != null)
        {
            foreach (var param in machineData.parameters)
            {
                CreateItem(param.Key, param.Value.ToString());
            }
        }
    }

    void CreateItem(string key, string value)
    {
        GameObject obj = Instantiate(Prefab, content);

        TextMeshProUGUI[] texts = obj.GetComponentsInChildren<TextMeshProUGUI>();

        texts[0].text = key;
        texts[1].text = value;
    }

    void OnApiFailed(string errorMessage)
    {
        Debug.LogError("API Failed: " + errorMessage);

        if (errorText != null)
            errorText.text = "Failed: " + errorMessage;

        loadingPanel.SetActive(true);
    }

    public void OnDismissButton()
    {
        uiRootCanvas.SetActive(false);

        foreach (Transform child in content)
            Destroy(child.gameObject);

        loadingPanel.SetActive(false);

        if (arRaycastManager != null)
            arRaycastManager.enabled = true;

        if (qrScanner != null)
            qrScanner.ResumeScanning();
    }
}