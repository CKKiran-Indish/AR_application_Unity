using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using Unity.Collections;

public class QRToARCanvasPlacer : MonoBehaviour
{
    [Header("AR References")]
    public ARCameraManager cameraManager;
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;

    [Header("Canvas to Move")]
    public Transform canvasTransform;

    [Header("Settings")]
    public float scanInterval = 0.3f;
    public int stableFrameRequired = 5;

    [Header("Manual Offset (LOCAL SPACE DEBUG)")]
    public float xOffset = 0f;
    public float yOffset = 0f;
    public float zOffset = 0f;

    private IBarcodeReader reader;
    private float lastScanTime;

    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool isLocked = false;
    private ARAnchor anchor;

    private Vector3 lastPosePosition;
    private int stableFrameCount = 0;

    void Start()
    {
        reader = new BarcodeReader
        {
            AutoRotate = false,
            TryInverted = true
        };
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrame;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrame;
    }

    void OnCameraFrame(ARCameraFrameEventArgs args)
    {
        if (isLocked)
            return;

        if (Time.time - lastScanTime < scanInterval)
            return;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image))
            return;

        lastScanTime = Time.time;

        using (image)
        {
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, image.width, image.height),
                outputDimensions = new Vector2Int(image.width / 2, image.height / 2),
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };

            int size = image.GetConvertedDataSize(conversionParams);
            var buffer = new NativeArray<byte>(size, Allocator.Temp);

            image.Convert(conversionParams, buffer);

            var result = reader.Decode(
                buffer.ToArray(),
                conversionParams.outputDimensions.x,
                conversionParams.outputDimensions.y,
                RGBLuminanceSource.BitmapFormat.RGBA32
            );

            buffer.Dispose();

            if (result != null)
            {
                TryPlaceCanvas(result.ResultPoints,
                    conversionParams.outputDimensions.x,
                    conversionParams.outputDimensions.y);
            }
        }
    }

    void TryPlaceCanvas(ResultPoint[] points, int imgWidth, int imgHeight)
    {
        if (points == null || points.Length == 0 || isLocked)
            return;

        float cx = 0f;
        float cy = 0f;

        foreach (var p in points)
        {
            cx += p.X;
            cy += p.Y;
        }

        cx /= points.Length;
        cy /= points.Length;

        Vector2 screenPoint = new Vector2(
            cx / imgWidth * Screen.width,
            (1f - (cy / imgHeight)) * Screen.height
        );

        if (raycastManager.Raycast(screenPoint, hits,
            TrackableType.PlaneWithinPolygon | TrackableType.FeaturePoint))
        {
            Pose pose = hits[0].pose;

            float distance = Vector3.Distance(lastPosePosition, pose.position);

            if (distance < 0.01f)
                stableFrameCount++;
            else
                stableFrameCount = 0;

            lastPosePosition = pose.position;

            if (stableFrameCount >= stableFrameRequired)
            {
                Vector3 adjustedPosition =
                    pose.position +
                    pose.right * xOffset +
                    pose.up * yOffset +
                    pose.forward * zOffset;

                var hit = hits[0];
                ARAnchor newAnchor = null;

                // Try plane anchor first
                if (hit.trackable is ARPlane plane)
                {
                    newAnchor = anchorManager.AttachAnchor(plane, new Pose(adjustedPosition, pose.rotation));
                }
                else
                {
                    // Fallback anchor (no plane yet)
                    GameObject temp = new GameObject("TempAnchor");
                    temp.transform.position = adjustedPosition;
                    temp.transform.rotation = pose.rotation;

                    newAnchor = temp.AddComponent<ARAnchor>();
                }

                if (newAnchor == null)
                {
                    Debug.LogWarning("Anchor creation failed");
                    return;
                }

                anchor = newAnchor;

                canvasTransform.SetParent(anchor.transform, false);
                canvasTransform.localPosition = Vector3.zero;
                canvasTransform.localRotation = Quaternion.identity;

                isLocked = true;

                Debug.Log("QR locked with anchor");
            }
        }
    }

    public void ResetTracking()
    {
        isLocked = false;
        stableFrameCount = 0;

        if (anchor != null)
        {
            Destroy(anchor.gameObject);
            anchor = null;
        }

        canvasTransform.SetParent(null);
    }
}