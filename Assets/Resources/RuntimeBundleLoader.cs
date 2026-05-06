using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RuntimeBundleLoader : MonoBehaviour
{
    [SerializeField] public APIManager APIManager;

    void OnEnable()
    {
        GetQRpose.OnQRDetected += LoadModelFromBundle;
    }

    void OnDisable()
    {
        GetQRpose.OnQRDetected -= LoadModelFromBundle;
    }

    void LoadModelFromBundle(string qrCodeData, Vector3 worldPos, Quaternion worldRot)
    {
        StartCoroutine(DownloadBundle(qrCodeData, worldPos, worldRot));
    }

    IEnumerator DownloadBundle(string modelName, Vector3 pos, Quaternion rot)
    {
        string url = $"{APIManager.address}:{APIManager.port}/bundle/fanbundle_{modelName}";

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

            GameObject instance = Instantiate(prefab, pos, rot);

            bundle.Unload(false);
        }
    }
}