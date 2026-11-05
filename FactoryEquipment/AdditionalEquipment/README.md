# Factory equipment additions

These three original procedural Blender models were generated for the SWHack Unity project.

## Unity locations

- FBX: `Assets/05_Models/`
- Texture maps: `Assets/10_Textures/<equipmentId>/`
- Unity materials: `Assets/04_Materials/FactoryEquipment/`
- Unity prefabs: `Assets/03_Prefabs/FactoryEquipment/`

Each FBX contains one equipment root and individually named `PART_<partId>` empty nodes with child meshes. The included JSON uses the same node names and stable part IDs. One Unity MeshCollider per mesh can be added by the consuming application if raycast hit points are needed.

## Texture and Unity material setup

Each model has Base Color, Metallic, Roughness, and tangent Normal PNG atlases at 512×512. UVs map each part to its own palette tile. Set Base Color to sRGB; set Metallic, Roughness, and Normal to linear data, with Normal imported as a Normal Map. The existing editor utility combines Metallic into RGB and `1 - Roughness` into alpha for URP/Lit, then assigns the normal map.

## Reproduction

Open `Scripts/create_equipment_additions.py` in Blender and run it. The source `.blend` files are saved under `Blender/`; one preview PNG and part metadata JSON are saved per model. FBX uses Unity-friendly axis conversion and meter-scale scene units.

The Unity Editor import and prefab setup step is performed separately in the open Unity project.
