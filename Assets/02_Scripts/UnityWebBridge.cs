using UnityEngine;
using TMPro;

public class UnityWebBridge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;

    private SelectModel selectModel;

    private void Awake()
    {
        selectModel = GetComponent<SelectModel>();
    }

    public void SetEquipment(string equipmentId)
    {
        if (text != null)
            text.text = $"설비 ID: {equipmentId}";

        Debug.Log($"React에서 받은 설비 ID: {equipmentId}");

        if (selectModel != null && !selectModel.SelectModelById(equipmentId))
            Debug.LogWarning($"No product is configured for ID '{equipmentId}'.", this);
    }
}
