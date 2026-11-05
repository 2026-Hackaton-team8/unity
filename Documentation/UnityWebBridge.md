# Unity and web equipment catalog

## Data ownership

`EquipmentProductData` is the Unity source for the equipment and repairable parts catalog. Its `ProductId` and each `PartId` are the external IDs used as the backend primary keys. Keep them stable when a model is edited. Since `PART.id` is a primary key in the current schema, part IDs must be unique across all equipment. Names are display text, not identifiers.

Each part entry also needs a `Model Path`. This is a Unity-only path from the spawned equipment root to the transform that represents that part. A unique node name such as `PART_BASE_PLATE` is accepted; use a full relative path if names repeat. The path is used to select and focus the 3D object and is deliberately omitted from the backend catalog JSON.

## Web to Unity

The scene currently has a GameObject named `==== UNITY WEB BRIGE ====` with `UnityWebBridge` attached. The web page can call these public methods with Unity WebGL's `SendMessage`. `SetEquipment` selects by model prefab name or product display name; the stable product ID is also accepted for existing integrations:

```javascript
unityInstance.SendMessage("==== UNITY WEB BRIGE ====", "RequestEquipmentCatalog");
unityInstance.SendMessage("==== UNITY WEB BRIGE ====", "SetEquipment", "PUMP-002");
// The product display name also works:
unityInstance.SendMessage("==== UNITY WEB BRIGE ====", "SetEquipment", "산업용 원심펌프");
unityInstance.SendMessage("==== UNITY WEB BRIGE ====", "SelectPart", "PUMP-002:BASE_PLATE");
```

`RequestEquipmentCatalog` sends a browser event named `unity-equipment-catalog`. The `detail` value is the parsed JSON object. The web app can listen for it and forward the object to the backend:

```javascript
window.addEventListener("unity-equipment-catalog", ({ detail }) => {
  // Send detail to the backend catalog registration/sync endpoint.
});
```

The payload shape is:

```json
{
  "schemaVersion": 1,
  "equipments": [
    {
      "id": "PUMP-002",
      "name": "산업용 원심펌프",
      "parts": [
        { "id": "PUMP-002:BASE_PLATE", "name": "설치 베이스" }
      ]
    }
  ]
}
```

The catalog is sent only when the web page requests it; Unity does not call the backend directly. The web app owns the backend request and authentication.

## Unity asset setup

The active ROBOT-001, CNC-001, PUMP-001, and COMPRESSOR-001 product assets are populated from the part metadata embedded in their FBX models. For new products, add one `EquipmentPartData` entry per selectable repair part with its stable ID, display name, and model path. When using the included additional equipment metadata, run **Assets → Factory Equipment → Create Additional Product Data** to import part names and model node names; the importer prefixes each part ID with its equipment ID so it remains globally unique. The exporter reports an error if a configured product has no parts, rather than sending an incomplete catalog. Select the matching equipment with `SetEquipment` before calling `SelectPart`.
