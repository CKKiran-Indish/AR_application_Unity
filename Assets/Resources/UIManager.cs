using UnityEngine;
using TMPro;
using Newtonsoft.Json.Linq;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    [Header("--- YOUR ORIGINAL WORKING REFERENCES ---")]
    public Transform content;
    public GameObject Prefab;
    public GameObject loadingPanel;

    [Header("--- NEW: World Space Canvas Root ---")]
    public GameObject uiRootCanvas;           // The root world-space Canvas GameObject

    [Header("--- NEW: Scanner & AR References ---")]
    public GetQRpose qrScanner;               // Drag GetQRpose component here
    public ARRaycastManager arRaycastManager; // Drag ARRaycastManager here

    [Header("--- NEW: Optional Error Label ---")]
    public TextMeshProUGUI errorText;         // Assign if you have an error label

    [Header("Placement")]
    public float forwardOffset = 0.05f;       // How far UI floats in front of QR surface

    // ---------------------------------------------------------------
    private void OnEnable()
    {
        GetQRpose.OnQRDetected += OnQRDetected;
        APIManager.GotApiData += PopulateUI;
        APIManager.OnApiFailed += OnApiFailed;

        loadingPanel.SetActive(false);
        uiRootCanvas.SetActive(false);        // Start hidden
    }

    private void OnDisable()
    {
        GetQRpose.OnQRDetected -= OnQRDetected;
        APIManager.GotApiData -= PopulateUI;
        APIManager.OnApiFailed -= OnApiFailed;
    }

    // ---------------------------------------------------------------
    // NEW: QR detected — place canvas in world, pause scanner
    // ---------------------------------------------------------------
    void OnQRDetected(string qrData, Vector3 worldPos, Quaternion worldRot)
    {
        // Place canvas at QR world position with a small forward offset
        Vector3 offsetPos = worldPos + worldRot * Vector3.forward * forwardOffset;
        uiRootCanvas.transform.position = offsetPos;

        // Face the canvas toward the camera
        Vector3 dirToCamera = Camera.main.transform.position - offsetPos;
        uiRootCanvas.transform.rotation = Quaternion.LookRotation(-dirToCamera, Vector3.up);

        // Show canvas — loading panel stays off until data arrives (your original logic)
        uiRootCanvas.SetActive(true);
        loadingPanel.SetActive(false);

        // Stop AR stealing scroll touches
        arRaycastManager.enabled = false;

        // Pause scanner — no new scans while UI is open
        qrScanner.PauseScanning();
    }

    // ---------------------------------------------------------------
    // YOUR ORIGINAL PopulateUI — completely untouched
    // ---------------------------------------------------------------
    void PopulateUI(MachineData machineData)
    {
        Debug.Log("UI RECEIVED DATA");
        loadingPanel.SetActive(true);

        // Clear existing UI
        foreach (Transform child in content)
        {
            Destroy(child.gameObject);
        }

        // Add basic info
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

    // ---------------------------------------------------------------
    // YOUR ORIGINAL CreateItem — completely untouched
    // ---------------------------------------------------------------
    void CreateItem(string key, string value)
    {
        GameObject obj = Instantiate(Prefab, content);

        TextMeshProUGUI[] texts = obj.GetComponentsInChildren<TextMeshProUGUI>();

        texts[0].text = key;
        texts[1].text = value;
    }

    // ---------------------------------------------------------------
    // NEW: API failed — show optional error text, keep UI dismissable
    // ---------------------------------------------------------------
    void OnApiFailed(string errorMessage)
    {
        Debug.LogError("API Failed: " + errorMessage);

        if (errorText != null)
            errorText.text = "Failed: " + errorMessage;

        loadingPanel.SetActive(true);
    }

    // ---------------------------------------------------------------
    // NEW: Wire this to your Dismiss button in the Inspector
    // ---------------------------------------------------------------
    public void OnDismissButton()
    {
        uiRootCanvas.SetActive(false);

        // Clear rows for next scan
        foreach (Transform child in content)
            Destroy(child.gameObject);

        loadingPanel.SetActive(false);

        // Restore AR touch handling
        arRaycastManager.enabled = true;

        // Resume scanner — also resets lastScannedQR internally
        qrScanner.ResumeScanning();

        Debug.Log("UI dismissed. Scanner resumed.");
    }
}