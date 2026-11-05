using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class EquipmentCatalogPayload
{
    public int schemaVersion = 1;
    public List<EquipmentCatalogItem> equipments = new List<EquipmentCatalogItem>();
}

[Serializable]
public sealed class EquipmentCatalogItem
{
    public string id;
    public string name;
    public List<EquipmentPartCatalogItem> parts = new List<EquipmentPartCatalogItem>();
}

[Serializable]
public sealed class EquipmentPartCatalogItem
{
    public string id;
    public string name;
}

/// <summary>
/// Creates the small, stable catalog payload shared with the web application.
/// Unity model paths stay local and are not sent to the backend.
/// </summary>
public static class EquipmentCatalogJson
{
    public static bool TrySerialize(
        IReadOnlyList<EquipmentProductData> products,
        out string json,
        out string error)
    {
        json = null;
        error = null;

        if (products == null || products.Count == 0)
        {
            error = "No equipment products are configured.";
            return false;
        }

        var payload = new EquipmentCatalogPayload();
        var equipmentIds = new HashSet<string>(StringComparer.Ordinal);
        var partIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (EquipmentProductData product in products)
        {
            if (product == null)
                continue;

            if (string.IsNullOrWhiteSpace(product.ProductId) || string.IsNullOrWhiteSpace(product.ProductName))
            {
                error = "Every equipment needs a non-empty ID and name.";
                return false;
            }

            if (!equipmentIds.Add(product.ProductId))
            {
                error = $"Duplicate equipment ID '{product.ProductId}'. IDs must be unique.";
                return false;
            }

            var equipment = new EquipmentCatalogItem
            {
                id = product.ProductId,
                name = product.ProductName
            };

            if (product.Parts == null)
            {
                error = $"Part data is missing for equipment '{product.ProductId}'.";
                return false;
            }
            if (product.Parts.Count == 0)
            {
                error = $"No parts are configured for equipment '{product.ProductId}'. Add part IDs, names, and model paths before exporting the catalog.";
                return false;
            }

            foreach (EquipmentPartData part in product.Parts)
            {
                if (part == null)
                {
                    error = $"Equipment '{product.ProductId}' contains an empty part entry.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(part.PartId) || string.IsNullOrWhiteSpace(part.PartName))
                {
                    error = $"Every part in equipment '{product.ProductId}' needs a non-empty ID and name.";
                    return false;
                }

                if (string.IsNullOrWhiteSpace(part.ModelPath))
                {
                    error = $"Part '{part.PartId}' in equipment '{product.ProductId}' needs a model path for Unity selection.";
                    return false;
                }

                if (!partIds.Add(part.PartId))
                {
                    error = $"Duplicate part ID '{part.PartId}'. PART.id is a primary key, so IDs must be unique across the catalog.";
                    return false;
                }

                equipment.parts.Add(new EquipmentPartCatalogItem
                {
                    id = part.PartId,
                    name = part.PartName
                });
            }

            payload.equipments.Add(equipment);
        }

        if (payload.equipments.Count == 0)
        {
            error = "No valid equipment products are configured.";
            return false;
        }

        json = JsonUtility.ToJson(payload);
        return true;
    }
}
