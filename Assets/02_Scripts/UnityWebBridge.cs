using UnityEngine;
using TMPro;

public class UnityWebBridge : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    public void SetEquipment(string equipmentId)
    {
        text.text = $"설비 ID: {equipmentId}";
        Debug.Log($"React에서 받은 설비 ID: {equipmentId}");
    }
}