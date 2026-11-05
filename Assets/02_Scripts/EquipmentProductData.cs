using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Factory Equipment/Product Model Data", fileName = "EquipmentProduct")]
public sealed class EquipmentProductData : ScriptableObject
{
    [Header("Product")]
    [SerializeField] private string productId;
    [SerializeField] private string productName;
    [SerializeField] private GameObject modelPrefab;

    [Header("Parts")]
    [SerializeField] private List<EquipmentPartData> parts = new List<EquipmentPartData>();

    public string ProductId => productId;
    public string ProductName => productName;
    public GameObject ModelPrefab => modelPrefab;
    public IReadOnlyList<EquipmentPartData> Parts => parts;
}

[Serializable]
public sealed class EquipmentPartData
{
    [SerializeField] private string partId;
    [SerializeField] private string partName;
    [SerializeField] private string modelPath;

    public string PartId => partId;
    public string PartName => partName;
    public string ModelPath => modelPath;
}
