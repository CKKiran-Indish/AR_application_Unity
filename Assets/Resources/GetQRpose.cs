using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ZXing;
using Unity.Collections;
using TMPro;
using System;

public class GetQRpose : MonoBehaviour
{
    // Now carries both the QR string AND the world position
    public static Action<string, Vector3, Quaternion> OnQRDetected;

    [SerializeField] private ARCameraManager cameraManager;
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private TextMeshProUGUI resultText;

    private IBarcodeReader reader;
    private float lastScanTime;
    private const float scanInterval = 0.5f;
    private string lastScannedQR = "";

    // Controlled externally by UIManager to pause/resume scanning
    private bool isScanning = true;

    void Start()
    {
        reader = new BarcodeReader
        {
            AutoRotate = false,
            TryInverted = true,
            Options = new ZXing.Common.DecodingOptions
            {
                TryHarder = true
            }
        };
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnFrame;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnFrame;
    }

    // Called by UIManager when UI is dismissed
    public void ResumeScanning()
    {
        lastScannedQR = "";   // reset so the same QR can be scanned again
        isScanning = true;
        Debug.Log("Scanner resumed.");
    }

    // Called by UIManager when QR is detected
    public void PauseScanning()
    {
        isScanning = false;
        Debug.Log("Scanner paused.");
    }

    void OnFrame(ARCameraFrameEventArgs args)
    {
        // Hard gate — don't scan while UI is active
        if (!isScanning) return;

        if (Time.time - lastScanTime < scanInterval) return;

        if (!cameraManager.TryAcquireLatestCpuImage(out XRCpuImage image)) return;

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

            int width = conversionParams.outputDimensions.x;
            int height = conversionParams.outputDimensions.y;

            var result = reader.Decode(
                buffer.ToArray(),
                width,
                height,
                RGBLuminanceSource.BitmapFormat.RGBA32
            );

            buffer.Dispose();

            if (result != null && result.Text != lastScannedQR)
            {
                lastScannedQR = result.Text;

                // --- Get world position via raycast from screen center ---
                Vector3 worldPos = Vector3.zero;
                Quaternion worldRot = Quaternion.identity;

                // Average the QR code's corner points to find its screen-space center
                Vector2 qrScreenCenter = Vector2.zero;
                foreach (var point in result.ResultPoints)
                {
                    qrScreenCenter += new Vector2(point.X, point.Y);
                }
                qrScreenCenter /= result.ResultPoints.Length;

                // ZXing uses a downscaled image, scale back up to screen coordinates
                qrScreenCenter.x = (qrScreenCenter.x / width) * Screen.width;
                qrScreenCenter.y = (1f - (qrScreenCenter.y / height)) * Screen.height; // flip Y
                var hits = new System.Collections.Generic.List<ARRaycastHit>();

                if (raycastManager.Raycast(qrScreenCenter, hits, TrackableType.PlaneWithinPolygon | TrackableType.PlaneWithinBounds))
                {
                    worldPos = hits[0].pose.position;
                    worldRot = hits[0].pose.rotation;
                }
                else
                {
                    // Fallback: place 1.5m in front of camera if no AR plane hit
                    Transform cam = Camera.main.transform;
                    worldPos = cam.position + cam.forward * 1.5f;
                    worldRot = Quaternion.LookRotation(-cam.forward, cam.up);
                }

                Handheld.Vibrate();
                resultText.text = "QR Code: " + result.Text;
                Debug.Log("QR Detected: " + result.Text + " at " + worldPos);

                OnQRDetected?.Invoke(result.Text, worldPos, worldRot);
            }
        }
    }
}