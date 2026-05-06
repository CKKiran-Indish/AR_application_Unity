using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.Networking;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class ARPlanePlacement : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public APIManager apiManager;
    public UIManager uiManager;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool isQRScanned = false;
    private string scannedQRData = "";
    private Vector3 scannedWorldPos;
    private Quaternion scannedWorldRot;

    private GameObject spawnedModel;

    void OnEnable()
    {
        GetQRpose.OnQRDetected += OnQRDetected;
    }

    void OnDisable()
    {
        GetQRpose.OnQRDetected -= OnQRDetected;
    }

    void OnQRDetected(string qrData, Vector3 worldPos, Quaternion worldRot)
    {
        scannedQRData = qrData;
        scannedWorldPos = worldPos;
        scannedWorldRot = worldRot;
        isQRScanned = true;
    }

    void Update()
    {
        if (!isQRScanned)
            return;

        if (Touchscreen.current == null)
            return;

        foreach (var touch in Touchscreen.current.touches)
        {
            if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
                continue;

            Vector2 touchPosition = touch.position.ReadValue();

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                StartCoroutine(DownloadPlaceThenCallAPI(scannedQRData, hitPose));
            }

            break;
        }
    }

    IEnumerator DownloadPlaceThenCallAPI(string modelName, Pose pose)
    {
        string url = $"{apiManager.address}:{apiManager.port}/bundle/fanbundle_{modelName}";

        using (UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(url))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Bundle download failed: " + www.error);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(www);
            GameObject prefab = bundle.LoadAsset<GameObject>(modelName);

            if (prefab == null)
            {
                Debug.LogError("Prefab not found in bundle!");
                yield break;
            }

            // Rotate 180° Y
            Quaternion correctedRotation = pose.rotation * Quaternion.Euler(0, 180f, 0);

            // Spawn model
            spawnedModel = Instantiate(prefab, pose.position, correctedRotation);

            bundle.Unload(false);

            // ✅ Calculate UI position beside model
            Vector3 uiPosition =
                spawnedModel.transform.position +
                spawnedModel.transform.right * 0.5f +
                Vector3.up * 0.2f;

            Vector3 dirToCamera = Camera.main.transform.position - uiPosition;
            Quaternion uiRotation = Quaternion.LookRotation(-dirToCamera, Vector3.up);

            // ✅ Show UI
            uiManager.ShowUI(uiPosition, uiRotation);

            // ✅ Call API AFTER model + UI placed
            apiManager.OnQRCodeScanned(scannedQRData, scannedWorldPos, scannedWorldRot);
        }
    }
}