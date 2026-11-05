using System.Runtime.InteropServices;
using UnityEngine;
using TMPro;

public class UnityWebBridge : MonoBehaviour
{
    [SerializeField] private SelectModel selectModel;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void DispatchEquipmentCatalog(string json);
#endif

    private void Awake()
    {
        ResolveSelectModel();
    }

    /// <summary>Called by the web page to select a model by prefab name, product name, or product ID.</summary>
    public void SetEquipment(string modelName)
    {
        ResolveSelectModel();

        Debug.Log($"React에서 받은 모델 이름: {modelName}");

        if (selectModel != null && !selectModel.SelectModelByName(modelName))
            Debug.LogWarning($"No model is configured for name '{modelName}'.", this);
    }

    /// <summary>
    /// Called by the web page when it needs the equipment/part catalog for backend sync.
    /// The catalog is sent to the browser as the `unity-equipment-catalog` event.
    /// </summary>
    public void RequestEquipmentCatalog()
    {
        ResolveSelectModel();
        if (selectModel == null)
        {
            Debug.LogError("UnityWebBridge could not find a SelectModel in the scene.", this);
            return;
        }

        if (!EquipmentCatalogJson.TrySerialize(selectModel.Products, out string json, out string error))
        {
            Debug.LogError($"Could not export equipment catalog: {error}", this);
            return;
        }

        SendEquipmentCatalogToBrowser(json);
    }

    /// <summary>Called by the web page after backend data identifies a part to show.</summary>
    public void SelectPart(string partId)
    {
        ResolveSelectModel();
        if (selectModel == null || !selectModel.SelectPartById(partId))
            Debug.LogWarning($"Could not select part ID '{partId}'.", this);
    }

    private void ResolveSelectModel()
    {
        if (selectModel == null)
            selectModel = GetComponent<SelectModel>();
        if (selectModel == null)
            selectModel = FindFirstObjectByType<SelectModel>();
    }

    private void SendEquipmentCatalogToBrowser(string json)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        DispatchEquipmentCatalog(json);
#else
        Debug.Log($"Equipment catalog JSON preview:\n{json}", this);
#endif
    }
}
