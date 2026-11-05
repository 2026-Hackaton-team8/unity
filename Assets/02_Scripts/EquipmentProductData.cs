using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory Equipment/Product Model Data", fileName = "EquipmentProduct")]
public sealed class EquipmentProductData : ScriptableObject
{
    [Header("Product")]
    [Tooltip("Stable external equipment ID. This value is sent as EQUIPMENT.id.")]
    [SerializeField] private string productId;
    [SerializeField] private string productName;
    [SerializeField] private GameObject modelPrefab;

    [Header("Parts")]
    [SerializeField] private List<EquipmentPartData> parts = new List<EquipmentPartData>();

    public string ProductId => productId;
    public string ProductName => productName;
    public GameObject ModelPrefab => modelPrefab;
    public IReadOnlyList<EquipmentPartData> Parts => parts;

    public bool TryGetPart(string partId, out EquipmentPartData part)
    {
        part = null;
        if (string.IsNullOrWhiteSpace(partId) || parts == null)
            return false;

        foreach (EquipmentPartData candidate in parts)
        {
            if (candidate == null || !string.Equals(candidate.PartId, partId, StringComparison.Ordinal))
                continue;

            part = candidate;
            return true;
        }

        return false;
    }
}

[Serializable]
public sealed class EquipmentPartData
{
    [Tooltip("Stable, globally unique external ID. This value is sent as PART.id.")]
    [SerializeField] private string partId;
    [SerializeField] private string partName;
    [Tooltip("Unique node name or relative Transform path below the spawned equipment root.")]
    [SerializeField] private string modelPath;

    public string PartId => partId;
    public string PartName => partName;
    public string ModelPath => modelPath;
}
