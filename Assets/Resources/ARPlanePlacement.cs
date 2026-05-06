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

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (Touchscreen.current == null)
            return;

        var activeTouches = Touchscreen.current.touches;

        foreach (var touch in activeTouches)
        {
            if (touch.phase.ReadValue() != UnityEngine.InputSystem.TouchPhase.Began)
                continue;

            Vector2 touchPosition = touch.position.ReadValue();
            Debug.Log($"Touch detected at {touchPosition}");

            if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                StartCoroutine(DownloadAndPlace("123", hitPose));
            }

            break; // only handle first touch
        }
    }



    IEnumerator DownloadAndPlace(string modelName, Pose pose)
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

            Instantiate(prefab, pose.position, pose.rotation);

            bundle.Unload(false);
        }
    }
}