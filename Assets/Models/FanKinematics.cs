using UnityEngine;
using Newtonsoft.Json.Linq;

public class FanKinematics : MonoBehaviour
{
    [Header("Machine Name (must match API machineName)")]
    public string machineName;

    [Header("Blade Transform")]
    public Transform blades;

    [Header("Rotation Axis")]
    public Vector3 rotationAxis = Vector3.up;

    private float rpm = 0f;

    void OnEnable()
    {
        APIManager.GotApiData += OnMachineDataReceived;
    }

    void OnDisable()
    {
        APIManager.GotApiData -= OnMachineDataReceived;
    }

    void Update()
    {
        if (rpm == 0 || blades == null) return;

        float degreesPerSecond = rpm * 360f / 60f;
        blades.Rotate(rotationAxis, degreesPerSecond * Time.deltaTime);
    }

    void OnMachineDataReceived(MachineData data)
    {
        // Check if this prefab matches machine name
        if (data.machineName != machineName)
            return;

        if (data.parameters == null)
            return;

        // Get speed parameter
        JToken speedToken = data.parameters["speed"];

        if (speedToken != null)
        {
            rpm = speedToken.Value<float>();
            Debug.Log($"Updated RPM for {machineName}: {rpm}");
        }
    }
}