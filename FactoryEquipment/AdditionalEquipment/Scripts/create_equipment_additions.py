"""Generate three industrial equipment assets in the open Unity project.

Run this file from Blender's Python Console. Geometry is procedural and every
major serviceable component is its own named Empty with one or more meshes.
"""

import bpy
import json
import math
import os
import shutil
from array import array
from mathutils import Vector


PROJECT = "/Users/anjaemin/GithupPro/SWHack"
ASSET_MODELS = os.path.join(PROJECT, "Assets", "05_Models")
ASSET_TEXTURES = os.path.join(PROJECT, "Assets", "10_Textures")
PACKAGE = os.path.join(PROJECT, "FactoryEquipment", "AdditionalEquipment")
BLEND_DIR = os.path.join(PACKAGE, "Blender")
METADATA_DIR = os.path.join(PACKAGE, "Metadata")
PREVIEW_DIR = os.path.join(PACKAGE, "Previews")
SCRIPT_DIR = os.path.join(PACKAGE, "Scripts")
SOURCE_PATH = "/Users/anjaemin/Documents/Codex/2026-11-04/new-chat/work/create_equipment_additions.py"
ATLAS_SIZE = 512
TILE_COUNT = 4
TILE_SIZE = ATLAS_SIZE // TILE_COUNT


ASSETS = [
    {
        "id": "PUMP-002",
        "name": "산업용 원심펌프",
        "name_en": "Industrial Centrifugal Pump",
        "center": (0.0, 0.0, 1.18),
        "camera_offset": (4.8, -7.5, 3.1),
        "width": 4.5,
        "height": 2.65,
        "palette": [
            (0.035, 0.27, 0.22), (0.83, 0.37, 0.035), (0.43, 0.50, 0.52), (0.12, 0.16, 0.18),
            (0.025, 0.035, 0.04), (0.035, 0.14, 0.19), (0.69, 0.72, 0.67), (0.62, 0.045, 0.025),
            (0.04, 0.44, 0.19), (0.20, 0.26, 0.25), (0.55, 0.60, 0.58), (0.92, 0.58, 0.07),
            (0.08, 0.10, 0.10), (0.30, 0.37, 0.38), (0.73, 0.76, 0.70), (0.18, 0.30, 0.27),
        ],
        "parts": [
            ("BASE_PLATE", "설치 베이스", ["앵커 볼트 풀림", "베이스 균열", "과도한 진동"]),
            ("ELECTRIC_MOTOR", "전동 모터", ["과열", "비정상 소음", "절연 저하"]),
            ("COUPLING", "커플링", ["정렬 불량", "커플링 마모", "비정상 진동"]),
            ("SHAFT", "회전축", ["축 마모", "축 휨", "베어링 소음"]),
            ("PUMP_CASING", "펌프 케이싱", ["케이싱 누수", "부식", "균열"]),
            ("IMPELLER", "임펠러", ["임펠러 마모", "이물질 막힘", "캐비테이션 흔적"]),
            ("INLET_PIPE", "흡입 배관", ["흡입 압력 저하", "플랜지 누수", "배관 막힘"]),
            ("OUTLET_PIPE", "토출 배관", ["토출 압력 저하", "배관 누수", "밸브 작동 불량"]),
            ("PRESSURE_GAUGE", "압력계", ["지침 고착", "계기 파손", "압력 표시 이상"]),
            ("CONTROL_BOX", "단자함", ["단자 과열", "배선 손상", "커버 체결 불량"]),
        ],
    },
    {
        "id": "WASHER-001",
        "name": "산업용 세탁기",
        "name_en": "Industrial Front-Load Washer",
        "center": (0.0, 0.0, 1.24),
        "camera_offset": (4.5, -8.0, 3.1),
        "width": 2.7,
        "height": 2.75,
        "palette": [
            (0.64, 0.68, 0.67), (0.15, 0.20, 0.22), (0.035, 0.045, 0.05), (0.48, 0.55, 0.56),
            (0.018, 0.024, 0.03), (0.035, 0.15, 0.23), (0.70, 0.055, 0.025), (0.035, 0.45, 0.19),
            (0.82, 0.84, 0.80), (0.28, 0.34, 0.34), (0.48, 0.51, 0.50), (0.84, 0.41, 0.035),
            (0.11, 0.14, 0.15), (0.61, 0.66, 0.64), (0.025, 0.21, 0.22), (0.80, 0.78, 0.69),
        ],
        "parts": [
            ("FRAME", "외함 프레임", ["외함 변형", "패널 진동", "부식"]),
            ("DRUM", "세탁 드럼", ["드럼 회전 불량", "드럼 내벽 손상", "이물질 끼임"]),
            ("DOOR_RING", "도어 링", ["도어 잠금 불량", "힌지 흔들림", "패킹 이탈"]),
            ("DOOR_GLASS", "도어 창", ["창 파손", "시야 오염", "누수 흔적"]),
            ("HINGE", "도어 힌지", ["힌지 소음", "힌지 처짐", "체결부 풀림"]),
            ("CONTROL_PANEL", "제어 패널", ["버튼 입력 불량", "표시창 이상", "비상 정지 불량"]),
            ("DETERGENT_DRAWER", "세제 투입함", ["투입함 막힘", "레일 걸림", "세제 누수"]),
            ("WATER_INLET", "급수 장치", ["급수 지연", "급수 호스 누수", "밸브 작동 불량"]),
            ("DRAIN_HOSE", "배수 호스", ["배수 지연", "호스 누수", "호스 막힘"]),
            ("SUPPORT_LEG", "지지 다리", ["수평 불량", "고무 받침 마모", "체결부 풀림"]),
        ],
    },
    {
        "id": "BLOWER-001",
        "name": "산업용 송풍기",
        "name_en": "Industrial Centrifugal Blower",
        "center": (0.0, 0.0, 1.08),
        "camera_offset": (4.8, -8.0, 3.3),
        "width": 3.25,
        "height": 2.7,
        "palette": [
            (0.045, 0.16, 0.28), (0.86, 0.34, 0.025), (0.16, 0.21, 0.23), (0.54, 0.60, 0.60),
            (0.018, 0.025, 0.03), (0.04, 0.35, 0.17), (0.66, 0.70, 0.66), (0.64, 0.045, 0.02),
            (0.075, 0.25, 0.36), (0.31, 0.38, 0.39), (0.77, 0.78, 0.72), (0.92, 0.57, 0.06),
            (0.09, 0.11, 0.12), (0.46, 0.52, 0.53), (0.025, 0.045, 0.055), (0.22, 0.31, 0.35),
        ],
        "parts": [
            ("BASE_FRAME", "설치 프레임", ["프레임 균열", "앵커 풀림", "수평 불량"]),
            ("BLOWER_HOUSING", "송풍기 케이싱", ["케이싱 진동", "공기 누설", "부식"]),
            ("IMPELLER", "임펠러", ["임펠러 오염", "날개 손상", "회전 불균형"]),
            ("ELECTRIC_MOTOR", "전동 모터", ["모터 과열", "베어링 소음", "절연 저하"]),
            ("SHAFT", "구동축", ["축 흔들림", "축 마모", "커플링 정렬 불량"]),
            ("INLET_GUARD", "흡입구 보호망", ["보호망 변형", "흡입구 막힘", "체결부 풀림"]),
            ("OUTLET_DUCT", "토출 덕트", ["덕트 누설", "토출 막힘", "연결부 진동"]),
            ("PRESSURE_GAUGE", "차압계", ["지침 고착", "표시 이상", "계기 파손"]),
            ("CONTROL_BOX", "제어함", ["배선 손상", "단자 과열", "커버 체결 불량"]),
        ],
    },
]


def link_object(obj, collection=None):
    (collection or bpy.context.scene.collection).objects.link(obj)
    return obj


def create_empty(name, parent=None, location=(0, 0, 0), display="CIRCLE"):
    obj = bpy.data.objects.new(name, None)
    obj.empty_display_type = display
    obj.empty_display_size = 0.16
    link_object(obj)
    obj.location = location
    if parent is not None:
        obj.parent = parent
    return obj


def keep_world_parent(obj, parent):
    world = obj.matrix_world.copy()
    obj.parent = parent
    obj.matrix_world = world


def uv_to_region(obj, tile_index):
    mesh = obj.data
    if not mesh.uv_layers:
        mesh.uv_layers.new(name="UVMap")
    uv_layer = mesh.uv_layers.active.data
    col = tile_index % TILE_COUNT
    row = tile_index // TILE_COUNT
    pad = 0.055
    u0, v0 = col / TILE_COUNT, row / TILE_COUNT
    span = (1.0 / TILE_COUNT) * (1.0 - pad * 2.0)
    for poly in mesh.polygons:
        normal = poly.normal
        if abs(normal.z) >= abs(normal.x) and abs(normal.z) >= abs(normal.y):
            axes = (0, 1)
        elif abs(normal.y) >= abs(normal.x):
            axes = (0, 2)
        else:
            axes = (1, 2)
        coords = [mesh.vertices[mesh.loops[li].vertex_index].co for li in poly.loop_indices]
        a_values = [co[axes[0]] for co in coords]
        b_values = [co[axes[1]] for co in coords]
        amin, amax = min(a_values), max(a_values)
        bmin, bmax = min(b_values), max(b_values)
        da = max(amax - amin, 0.00001)
        db = max(bmax - bmin, 0.00001)
        for li in poly.loop_indices:
            co = mesh.vertices[mesh.loops[li].vertex_index].co
            uu = (co[axes[0]] - amin) / da
            vv = (co[axes[1]] - bmin) / db
            uv_layer[li].uv = (u0 + (pad + uu * (1.0 - 2 * pad)) / TILE_COUNT,
                               v0 + (pad + vv * (1.0 - 2 * pad)) / TILE_COUNT)


def finish_mesh(obj, parent, tile_index, smooth=False, bevel=0.0, material=None):
    obj.data.name = obj.name + "_Mesh"
    if material is not None:
        obj.data.materials.append(material)
    uv_to_region(obj, tile_index)
    for poly in obj.data.polygons:
        poly.use_smooth = smooth
    if bevel > 0:
        mod = obj.modifiers.new("Edge radii", "BEVEL")
        mod.width = bevel
        mod.segments = 2
        mod.limit_method = "ANGLE"
        mod.angle_limit = math.radians(25)
        weighted = obj.modifiers.new("Weighted corner normals", "WEIGHTED_NORMAL")
        weighted.keep_sharp = True
    if parent is not None:
        keep_world_parent(obj, parent)
        obj["partId"] = parent.get("partId", "")
    return obj


def add_cube(name, parent, location, dimensions, tile, material, bevel=0.035, rotation=None):
    bpy.ops.mesh.primitive_cube_add(size=1.0, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    if rotation is not None:
        obj.rotation_euler = rotation
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, parent, tile, bevel=bevel, material=material)


def add_cylinder(name, parent, center, axis, radius, depth, tile, material, vertices=32,
                 bevel=0.015, smooth=True):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=center)
    obj = bpy.context.object
    obj.name = name
    direction = Vector(axis).normalized()
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    return finish_mesh(obj, parent, tile, smooth=smooth, bevel=bevel, material=material)


def add_torus(name, parent, center, axis, major, minor, tile, material, segments=40, sides=8):
    bpy.ops.mesh.primitive_torus_add(major_segments=segments, minor_segments=sides,
                                     major_radius=major, minor_radius=minor, location=center)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = Vector(axis).normalized().to_track_quat("Z", "Y").to_euler()
    return finish_mesh(obj, parent, tile, smooth=True, material=material)


def add_sphere(name, parent, center, scale, tile, material):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=20, ring_count=12, radius=1.0, location=center)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    return finish_mesh(obj, parent, tile, smooth=True, material=material)


def basis_for_axis(axis):
    n = Vector(axis).normalized()
    seed = Vector((0, 0, 1)) if abs(n.z) < 0.88 else Vector((0, 1, 0))
    u = n.cross(seed).normalized()
    v = n.cross(u).normalized()
    return u, v


def add_bolts(parent, center, axis, count, bolt_circle, bolt_radius, tile, material, depth=0.05):
    c = Vector(center)
    n = Vector(axis).normalized()
    u, v = basis_for_axis(n)
    for i in range(count):
        angle = 2 * math.pi * i / count
        p = c + bolt_circle * (math.cos(angle) * u + math.sin(angle) * v)
        add_cylinder(f"Fastener_{i + 1:02d}_Mesh", parent, p, n, bolt_radius, depth,
                     tile, material, vertices=12, bevel=0.004, smooth=False)


def add_pipe(parent, name, points, radius, tile, material, flange_points=()):
    for i in range(len(points) - 1):
        a, b = Vector(points[i]), Vector(points[i + 1])
        delta = b - a
        add_cylinder(f"{name}_Segment_{i + 1}_Mesh", parent, (a + b) * 0.5, delta,
                     radius, delta.length, tile, material, vertices=24, bevel=0.012, smooth=True)
    for i, point in enumerate(flange_points):
        axis = Vector(points[min(i + 1, len(points) - 1)]) - Vector(points[max(i, 0)])
        if axis.length < 0.001:
            axis = Vector((0, 0, 1))
        add_torus(f"{name}_Flange_{i + 1}_Mesh", parent, point, axis, radius * 1.12,
                  radius * 0.18, tile, material, segments=32, sides=8)
        add_bolts(parent, point, axis, 6, radius * 0.91, radius * 0.10,
                  tile, material, depth=0.035)


def add_fan_blades(parent, name, center, axis="X", count=8, inner=0.13, outer=0.49,
                   tile=2, material=None, thickness=0.055, twist=0.24):
    center = Vector(center)
    vertices, faces = [], []
    axis_key = axis.upper()
    for i in range(count):
        base = 2 * math.pi * i / count
        a0 = base - 0.18
        a1 = base + 0.15
        a2 = base + twist + 0.20
        a3 = base + twist + 0.49
        yz = [(inner * math.cos(a0), inner * math.sin(a0)),
              (inner * math.cos(a1), inner * math.sin(a1)),
              (outer * math.cos(a2), outer * math.sin(a2)),
              (outer * math.cos(a3), outer * math.sin(a3))]
        start = len(vertices)
        for side in (-1, 1):
            for p, q in yz:
                if axis_key == "X":
                    vertices.append((side * thickness * 0.5, p, q))
                elif axis_key == "Y":
                    vertices.append((p, side * thickness * 0.5, q))
                else:
                    vertices.append((p, q, side * thickness * 0.5))
        faces.extend([
            (start, start + 1, start + 2, start + 3),
            (start + 7, start + 6, start + 5, start + 4),
            (start, start + 4, start + 5, start + 1),
            (start + 1, start + 5, start + 6, start + 2),
            (start + 2, start + 6, start + 7, start + 3),
            (start + 3, start + 7, start + 4, start),
        ])
    mesh = bpy.data.meshes.new(name + "_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    link_object(obj)
    obj.location = center
    return finish_mesh(obj, parent, tile, material=material)


def generate_atlases(asset):
    folder = os.path.join(ASSET_TEXTURES, asset["id"])
    os.makedirs(folder, exist_ok=True)
    palettes = asset["palette"]
    image_files = {}
    maps = {
        "BaseColor": [],
        "Metallic": [],
        "Roughness": [],
        "Normal": [],
    }
    category_metallic = [0.72, 0.70, 0.96, 0.92, 0.02, 0.04, 0.84, 0.76,
                         0.42, 0.81, 0.94, 0.64, 0.26, 0.90, 0.77, 0.69]
    category_roughness = [0.39, 0.42, 0.26, 0.31, 0.82, 0.18, 0.30, 0.38,
                          0.32, 0.48, 0.25, 0.43, 0.58, 0.29, 0.47, 0.34]
    for map_name in maps:
        image = bpy.data.images.new(f"{asset['id']}_{map_name}", width=ATLAS_SIZE,
                                    height=ATLAS_SIZE, alpha=False, float_buffer=False)
        if map_name != "BaseColor":
            image.colorspace_settings.name = "Non-Color"
        pixels = array("f")
        for y in range(ATLAS_SIZE):
            row = y // TILE_SIZE
            ly = (y % TILE_SIZE) / (TILE_SIZE - 1)
            for x in range(ATLAS_SIZE):
                col = x // TILE_SIZE
                lx = (x % TILE_SIZE) / (TILE_SIZE - 1)
                tile = row * TILE_COUNT + col
                wave = math.sin((x % TILE_SIZE) * 0.63 + asset["seed"] * 1.7) * 0.006
                fine = math.sin((x * 17 + y * 13 + asset["seed"] * 31) * 0.071) * 0.008
                seam = 0.88 if (lx < 0.018 or lx > 0.982 or ly < 0.018 or ly > 0.982) else 1.0
                if map_name == "BaseColor":
                    color = palettes[tile % len(palettes)]
                    mod = 1.0 + wave + fine
                    pixels.extend((max(0, min(1, color[0] * mod * seam)),
                                   max(0, min(1, color[1] * mod * seam)),
                                   max(0, min(1, color[2] * mod * seam)), 1.0))
                elif map_name == "Metallic":
                    value = category_metallic[tile] * (1.0 + fine * 0.55) * seam
                    pixels.extend((value, value, value, 1.0))
                elif map_name == "Roughness":
                    value = category_roughness[tile] + wave * 1.8 + fine
                    value = max(0.03, min(1.0, value * (1.0 if seam == 1 else 0.93)))
                    pixels.extend((value, value, value, 1.0))
                else:
                    phase = ly * math.tau * 22.0
                    dx = math.cos(phase) * 0.018 + math.cos(lx * math.tau * 9.0) * 0.004
                    dy = math.sin(phase) * 0.006
                    nx, ny, nz = -dx, -dy, 1.0
                    inv = 1.0 / math.sqrt(nx * nx + ny * ny + nz * nz)
                    pixels.extend((0.5 + 0.5 * nx * inv, 0.5 + 0.5 * ny * inv,
                                   0.5 + 0.5 * nz * inv, 1.0))
        image.pixels.foreach_set(pixels)
        image.update()
        path = os.path.join(folder, f"{asset['id']}_{map_name}.png")
        image.filepath_raw = path
        image.file_format = "PNG"
        image.save()
        image.pack()
        maps[map_name] = image
        image_files[map_name] = path
    return maps, image_files


def create_material(asset, images):
    mat = bpy.data.materials.new(asset["id"] + "_MAT")
    mat.diffuse_color = (*asset["palette"][0], 1.0)
    mat.use_nodes = True
    nodes = mat.node_tree.nodes
    links = mat.node_tree.links
    principled = nodes.get("Principled BSDF")
    principled.inputs["Metallic"].default_value = 0.6
    principled.inputs["Roughness"].default_value = 0.38
    for map_name, image in images.items():
        tex = nodes.new("ShaderNodeTexImage")
        tex.image = image
        tex.label = map_name
        tex.location = (-620, {"BaseColor": 260, "Metallic": 40, "Roughness": -180, "Normal": -420}[map_name])
        if map_name == "BaseColor":
            links.new(tex.outputs["Color"], principled.inputs["Base Color"])
        elif map_name == "Metallic":
            links.new(tex.outputs["Color"], principled.inputs["Metallic"])
        elif map_name == "Roughness":
            links.new(tex.outputs["Color"], principled.inputs["Roughness"])
        else:
            normal = nodes.new("ShaderNodeNormalMap")
            normal.location = (-330, -420)
            normal.inputs["Strength"].default_value = 0.32
            links.new(tex.outputs["Color"], normal.inputs["Color"])
            links.new(normal.outputs["Normal"], principled.inputs["Normal"])
    return mat


def make_part(root, part_id, name_kr, location=(0, 0, 0)):
    node = create_empty("PART_" + part_id, root, location=location)
    node["partId"] = part_id
    node["partName"] = name_kr
    node["equipmentId"] = root.get("equipmentId", "")
    return node


def build_pump(root, mat):
    part = make_part(root, "BASE_PLATE", "설치 베이스")
    add_cube("BasePlate_Mesh", part, (0, 0, 0.16), (4.1, 1.28, 0.28), 0, mat, 0.065)
    add_cube("FrontStiffener_Mesh", part, (0, -0.49, 0.35), (3.72, 0.16, 0.16), 2, mat, 0.025)
    add_cube("RearStiffener_Mesh", part, (0, 0.49, 0.35), (3.72, 0.16, 0.16), 2, mat, 0.025)
    for x in (-1.65, 1.55):
        for y in (-0.48, 0.48):
            add_cylinder("AnchorPad_Mesh", part, (x, y, 0.025), (0, 0, 1), 0.12, 0.05, 2, mat, 20, 0.008)
            add_cylinder("AnchorBolt_Mesh", part, (x, y, 0.075), (0, 0, 1), 0.046, 0.09, 10, mat, 12, 0.003, False)

    part = make_part(root, "ELECTRIC_MOTOR", "전동 모터", (-1.05, 0, 0.78))
    add_cylinder("MotorStator_Mesh", part, (-1.03, 0, 0.88), (1, 0, 0), 0.48, 1.42, 0, mat, 48, 0.025)
    add_cylinder("MotorFrontBell_Mesh", part, (-0.28, 0, 0.88), (1, 0, 0), 0.43, 0.20, 9, mat, 40, 0.018)
    add_cylinder("MotorRearCover_Mesh", part, (-1.81, 0, 0.88), (1, 0, 0), 0.41, 0.22, 9, mat, 40, 0.018)
    for i in range(10):
        x = -1.64 + i * 0.12
        add_torus(f"CoolingFin_{i + 1:02d}_Mesh", part, (x, 0, 0.88), (1, 0, 0), 0.455, 0.022, 2, mat, 32, 6)
    for y in (-0.30, 0.30):
        add_cube("MotorFoot_Mesh", part, (-1.03, y, 0.47), (0.94, 0.22, 0.18), 2, mat, 0.035)
    add_cube("TerminalBox_Mesh", part, (-1.02, 0.03, 1.45), (0.47, 0.48, 0.26), 2, mat, 0.035)
    add_cube("TerminalCover_Mesh", part, (-1.02, -0.01, 1.59), (0.42, 0.42, 0.045), 0, mat, 0.018)
    add_cylinder("FanGuard_Mesh", part, (-1.96, 0, 0.88), (1, 0, 0), 0.31, 0.06, 3, mat, 32, 0.01)
    add_torus("FanGuard_Rim_Mesh", part, (-2.0, 0, 0.88), (1, 0, 0), 0.30, 0.025, 2, mat)
    for j, z in enumerate((-0.14, 0, 0.14)):
        add_cube(f"FanGuardBar_H_{j}_Mesh", part, (-2.01, 0, 0.88 + z), (0.026, 0.52, 0.026), 2, mat, 0.006)

    part = make_part(root, "COUPLING", "커플링", (-0.02, 0, 0.88))
    add_cylinder("FlexibleCoupling_Mesh", part, (-0.03, 0, 0.88), (1, 0, 0), 0.27, 0.36, 1, mat, 40, 0.02)
    add_torus("CouplingBand_Mesh", part, (-0.03, 0, 0.88), (1, 0, 0), 0.265, 0.038, 2, mat)
    add_bolts(part, (-0.03, 0, 0.88), (1, 0, 0), 6, 0.245, 0.035, 2, mat, 0.07)
    add_cylinder("MotorShaft_Mesh", part, (-0.39, 0, 0.88), (1, 0, 0), 0.09, 0.54, 2, mat, 24, 0.006)

    part = make_part(root, "SHAFT", "회전축", (0.36, 0, 0.88))
    add_cylinder("PumpShaft_Mesh", part, (0.47, 0, 0.88), (1, 0, 0), 0.10, 0.76, 2, mat, 28, 0.006)
    add_cylinder("ShaftSleeve_Mesh", part, (0.73, 0, 0.88), (1, 0, 0), 0.16, 0.20, 10, mat, 32, 0.01)

    part = make_part(root, "PUMP_CASING", "펌프 케이싱", (0.75, 0, 0.88))
    add_cylinder("VoluteBack_Mesh", part, (0.53, 0, 0.88), (1, 0, 0), 0.66, 0.36, 0, mat, 48, 0.03)
    add_torus("VoluteScroll_Mesh", part, (0.70, 0, 0.88), (1, 0, 0), 0.50, 0.18, 0, mat, 56, 10)
    add_cylinder("CasingCover_Mesh", part, (0.87, 0, 0.88), (1, 0, 0), 0.57, 0.12, 0, mat, 48, 0.025)
    add_torus("CasingSeal_Rim_Mesh", part, (0.94, 0, 0.88), (1, 0, 0), 0.47, 0.032, 2, mat, 48, 8)
    add_bolts(part, (0.95, 0, 0.88), (1, 0, 0), 10, 0.51, 0.035, 2, mat, 0.045)
    add_cylinder("BearingHousing_Mesh", part, (0.31, 0, 0.88), (1, 0, 0), 0.24, 0.24, 2, mat, 36, 0.018)

    part = make_part(root, "IMPELLER", "임펠러", (0.92, 0, 0.88))
    add_cylinder("ImpellerHub_Mesh", part, (0.93, 0, 0.88), (1, 0, 0), 0.16, 0.22, 2, mat, 32, 0.012)
    add_fan_blades(part, "ImpellerBlades_Mesh", (1.04, 0, 0.88), axis="X", count=7,
                   inner=0.13, outer=0.43, tile=10, material=mat, thickness=0.06, twist=0.32)
    add_torus("ImpellerShroud_Mesh", part, (1.08, 0, 0.88), (1, 0, 0), 0.43, 0.028, 2, mat, 40, 6)

    part = make_part(root, "INLET_PIPE", "흡입 배관", (1.25, 0, 0.88))
    add_pipe(part, "SuctionLine", [(0.87, 0, 0.88), (1.40, 0, 0.88), (1.75, 0, 0.88)],
             0.205, 3, mat, flange_points=[(1.48, 0, 0.88), (1.73, 0, 0.88)])
    add_cylinder("SuctionFlange_Face_Mesh", part, (1.77, 0, 0.88), (1, 0, 0), 0.32, 0.08, 2, mat, 40, 0.01)

    part = make_part(root, "OUTLET_PIPE", "토출 배관", (0.64, 0, 1.40))
    add_pipe(part, "DischargeLine", [(0.63, 0, 1.22), (0.63, 0, 1.66),
                                      (0.63, 0, 2.04), (1.00, 0, 2.04)],
             0.15, 3, mat, flange_points=[(0.63, 0, 1.61), (0.96, 0, 2.04)])
    add_cylinder("DischargeFlange_Face_Mesh", part, (1.00, 0, 2.04), (1, 0, 0), 0.23, 0.06, 2, mat, 32, 0.008)

    part = make_part(root, "PRESSURE_GAUGE", "압력계", (0.30, -0.40, 1.36))
    add_cylinder("GaugeStem_Mesh", part, (0.30, -0.44, 1.43), (0, 1, 0), 0.042, 0.20, 3, mat, 16, 0.006)
    add_cylinder("GaugeCase_Mesh", part, (0.30, -0.57, 1.62), (0, 1, 0), 0.17, 0.10, 2, mat, 36, 0.012)
    add_cylinder("GaugeDial_Mesh", part, (0.30, -0.63, 1.62), (0, 1, 0), 0.138, 0.025, 14, mat, 36, 0.004)
    add_cylinder("GaugeNeedle_Mesh", part, (0.30, -0.65, 1.62), (0, 1, 0), 0.012, 0.014, 7, mat, 8, 0.0)
    add_torus("GaugeBezel_Mesh", part, (0.30, -0.66, 1.62), (0, 1, 0), 0.148, 0.017, 2, mat, 36, 8)

    part = make_part(root, "CONTROL_BOX", "단자함", (-1.04, 0.02, 1.47))
    add_cube("JunctionBox_Mesh", part, (-1.04, 0.33, 1.46), (0.38, 0.32, 0.24), 9, mat, 0.025)
    for y in (0.18, 0.48):
        add_cylinder("CableGland_Mesh", part, (-1.04, y, 1.31), (0, 0, 1), 0.055, 0.07, 2, mat, 16, 0.004)
    for x in (-1.16, -0.92):
        add_cylinder("CoverScrew_Mesh", part, (x, 0.16, 1.47), (0, 1, 0), 0.018, 0.022, 2, mat, 10, 0.002, False)


def build_washer(root, mat):
    part = make_part(root, "FRAME", "외함 프레임")
    add_cube("LeftCabinetPanel_Mesh", part, (-0.91, 0.02, 1.25), (0.16, 1.48, 2.35), 0, mat, 0.06)
    add_cube("RightCabinetPanel_Mesh", part, (0.91, 0.02, 1.25), (0.16, 1.48, 2.35), 0, mat, 0.06)
    add_cube("CabinetTop_Mesh", part, (0, 0.02, 2.39), (1.95, 1.48, 0.15), 0, mat, 0.045)
    add_cube("CabinetBack_Mesh", part, (0, 0.70, 1.25), (1.82, 0.12, 2.22), 3, mat, 0.025)
    add_cube("CabinetBottom_Mesh", part, (0, 0.02, 0.16), (1.80, 1.42, 0.20), 1, mat, 0.035)
    add_cube("FrontUpperRail_Mesh", part, (0, -0.72, 2.20), (1.72, 0.12, 0.32), 0, mat, 0.035)
    add_cube("FrontLeftRail_Mesh", part, (-0.77, -0.72, 1.23), (0.20, 0.12, 1.65), 0, mat, 0.035)
    add_cube("FrontRightRail_Mesh", part, (0.77, -0.72, 1.23), (0.20, 0.12, 1.65), 0, mat, 0.035)
    add_cube("FrontLowerRail_Mesh", part, (0, -0.72, 0.32), (1.58, 0.12, 0.22), 1, mat, 0.03)
    add_cube("DrumSupport_Mesh", part, (0, 0.18, 1.22), (1.35, 0.98, 0.12), 2, mat, 0.025)

    part = make_part(root, "SUPPORT_LEG", "지지 다리")
    for x in (-0.72, 0.72):
        for y in (-0.53, 0.53):
            add_cylinder("LevelingFoot_Mesh", part, (x, y, 0.055), (0, 0, 1), 0.11, 0.11, 4, mat, 24, 0.012)
            add_cylinder("LevelingStem_Mesh", part, (x, y, 0.15), (0, 0, 1), 0.052, 0.16, 3, mat, 16, 0.005)

    part = make_part(root, "DRUM", "세탁 드럼", (0, 0.10, 1.25))
    add_cylinder("StainlessDrumShell_Mesh", part, (0, 0.12, 1.25), (0, 1, 0), 0.60, 0.92, 3, mat, 56, 0.025)
    add_cylinder("DrumRearPlate_Mesh", part, (0, 0.60, 1.25), (0, 1, 0), 0.56, 0.05, 3, mat, 48, 0.014)
    add_torus("DrumFrontLip_Mesh", part, (0, -0.34, 1.25), (0, 1, 0), 0.55, 0.045, 10, mat, 48, 8)
    for i in range(8):
        angle = 2 * math.pi * i / 8
        x, z = 0.42 * math.cos(angle), 1.25 + 0.42 * math.sin(angle)
        add_cube("DrumLifter_Mesh", part, (x, 0.13, z), (0.12, 0.64, 0.12), 8, mat,
                 0.035, rotation=(0, -angle, 0))
    for i in range(12):
        angle = 2 * math.pi * i / 12
        x, z = 0.47 * math.cos(angle), 1.25 + 0.47 * math.sin(angle)
        add_sphere("DrumPerforationDetail_Mesh", part, (x, -0.367, z), (0.022, 0.012, 0.022), 2, mat)

    part = make_part(root, "DOOR_RING", "도어 링", (0, -0.80, 1.24))
    add_torus("DoorOuterRing_Mesh", part, (0, -0.81, 1.24), (0, 1, 0), 0.64, 0.105, 8, mat, 56, 10)
    add_torus("DoorSeal_Mesh", part, (0, -0.73, 1.24), (0, 1, 0), 0.525, 0.065, 4, mat, 56, 10)
    add_cube("DoorLatch_Mesh", part, (0.65, -0.85, 1.22), (0.12, 0.10, 0.22), 2, mat, 0.025)
    for z in (0.80, 1.68):
        add_cube("HingeMount_Mesh", part, (-0.78, -0.78, z), (0.12, 0.13, 0.22), 3, mat, 0.025)

    part = make_part(root, "DOOR_GLASS", "도어 창", (0, -0.90, 1.24))
    add_cylinder("DoorWindow_Mesh", part, (0, -0.86, 1.24), (0, 1, 0), 0.47, 0.055, 5, mat, 56, 0.012)
    add_torus("WindowInnerBead_Mesh", part, (0, -0.90, 1.24), (0, 1, 0), 0.47, 0.018, 3, mat, 56, 8)

    part = make_part(root, "HINGE", "도어 힌지", (-0.81, -0.79, 1.25))
    for z in (0.82, 1.67):
        add_cylinder("HingePin_Mesh", part, (-0.81, -0.83, z), (0, 0, 1), 0.06, 0.40, 3, mat, 20, 0.006)
        add_cube("HingeLeaf_Mesh", part, (-0.77, -0.78, z), (0.18, 0.12, 0.28), 2, mat, 0.02)

    part = make_part(root, "CONTROL_PANEL", "제어 패널", (0.42, -0.81, 2.02))
    add_cube("ConsoleHousing_Mesh", part, (0.42, -0.81, 2.05), (0.93, 0.18, 0.40), 1, mat, 0.045)
    add_cube("DisplayBezel_Mesh", part, (0.20, -0.916, 2.11), (0.37, 0.045, 0.22), 2, mat, 0.018)
    add_cube("DisplayGlass_Mesh", part, (0.20, -0.943, 2.11), (0.31, 0.018, 0.16), 5, mat, 0.008)
    add_cylinder("ProgramDial_Mesh", part, (0.64, -0.93, 2.08), (0, 1, 0), 0.105, 0.065, 10, mat, 32, 0.009)
    add_cylinder("DialCenter_Mesh", part, (0.64, -0.97, 2.08), (0, 1, 0), 0.045, 0.022, 3, mat, 24, 0.004)
    for x, tile in ((0.84, 7), (0.48, 11), (0.32, 6)):
        add_cylinder("PanelButton_Mesh", part, (x, -0.93, 1.96), (0, 1, 0), 0.045, 0.04, tile, mat, 20, 0.005)

    part = make_part(root, "DETERGENT_DRAWER", "세제 투입함", (-0.42, -0.80, 2.04))
    add_cube("DrawerFace_Mesh", part, (-0.42, -0.82, 2.06), (0.52, 0.14, 0.27), 8, mat, 0.025)
    add_cube("DrawerHandle_Mesh", part, (-0.42, -0.91, 2.04), (0.23, 0.035, 0.045), 3, mat, 0.012)
    add_cube("DrawerLabel_Mesh", part, (-0.42, -0.91, 2.13), (0.18, 0.018, 0.03), 2, mat, 0.005)

    part = make_part(root, "WATER_INLET", "급수 장치")
    add_pipe(part, "WaterFeed", [(-0.48, 0.58, 2.08), (-0.48, 0.82, 2.08), (-0.48, 0.82, 1.85)],
             0.055, 3, mat, flange_points=[(-0.48, 0.82, 2.08)])
    add_cylinder("SolenoidValve_Mesh", part, (-0.48, 0.72, 1.91), (0, 1, 0), 0.12, 0.16, 11, mat, 24, 0.015)
    add_pipe(part, "InletHose", [(-0.48, 0.70, 1.91), (-0.48, 0.57, 1.91)], 0.045, 4, mat)

    part = make_part(root, "DRAIN_HOSE", "배수 호스")
    add_pipe(part, "DrainHose", [(0.58, 0.46, 0.34), (0.58, 0.78, 0.34),
                                  (0.58, 0.82, 0.16), (0.58, 0.55, 0.11)], 0.075, 4, mat)
    add_cylinder("DrainCoupler_Mesh", part, (0.58, 0.78, 0.34), (0, 1, 0), 0.10, 0.12, 3, mat, 24, 0.008)


def build_blower(root, mat):
    part = make_part(root, "BASE_FRAME", "설치 프레임")
    add_cube("BlowerSkid_Mesh", part, (0, 0, 0.15), (3.25, 1.50, 0.28), 0, mat, 0.055)
    add_cube("SkidFrontRail_Mesh", part, (0, -0.61, 0.34), (3.00, 0.16, 0.16), 2, mat, 0.025)
    add_cube("SkidRearRail_Mesh", part, (0, 0.61, 0.34), (3.00, 0.16, 0.16), 2, mat, 0.025)
    for x in (-1.18, 0.95):
        for y in (-0.52, 0.52):
            add_cube("MachineFoot_Mesh", part, (x, y, 0.47), (0.58, 0.72, 0.17), 2, mat, 0.025)
    for x in (-1.38, 1.38):
        for y in (-0.56, 0.56):
            add_cylinder("AnchorBolt_Mesh", part, (x, y, 0.055), (0, 0, 1), 0.045, 0.07, 3, mat, 12, 0.003, False)

    part = make_part(root, "ELECTRIC_MOTOR", "전동 모터", (-1.05, 0.38, 0.95))
    add_cylinder("MotorBody_Mesh", part, (-1.08, 0.31, 1.02), (0, 1, 0), 0.44, 1.15, 0, mat, 44, 0.024)
    add_cylinder("MotorFrontEndbell_Mesh", part, (-1.08, -0.31, 1.02), (0, 1, 0), 0.39, 0.18, 9, mat, 36, 0.018)
    add_cylinder("MotorRearEndbell_Mesh", part, (-1.08, 0.95, 1.02), (0, 1, 0), 0.38, 0.16, 9, mat, 36, 0.018)
    for i in range(9):
        y = -0.18 + i * 0.13
        add_torus(f"CoolingFin_{i + 1:02d}_Mesh", part, (-1.08, y, 1.02), (0, 1, 0), 0.42, 0.023, 2, mat, 32, 6)
    add_cube("MotorTerminalBox_Mesh", part, (-1.08, 0.30, 1.58), (0.42, 0.38, 0.24), 2, mat, 0.028)
    add_cube("TerminalCover_Mesh", part, (-1.08, 0.30, 1.72), (0.38, 0.34, 0.035), 0, mat, 0.012)
    add_cylinder("MotorFanCover_Mesh", part, (-1.08, 1.08, 1.02), (0, 1, 0), 0.28, 0.08, 3, mat, 32, 0.008)
    add_torus("MotorFanGuardRim_Mesh", part, (-1.08, 1.13, 1.02), (0, 1, 0), 0.27, 0.022, 11, mat, 32, 6)

    part = make_part(root, "SHAFT", "구동축", (-0.35, 0.07, 1.02))
    add_cylinder("DriveShaft_Mesh", part, (-0.12, 0.04, 1.02), (0, 1, 0), 0.09, 0.88, 3, mat, 24, 0.006)
    add_cylinder("BearingCarrier_Mesh", part, (-0.12, 0.46, 1.02), (0, 1, 0), 0.18, 0.18, 2, mat, 32, 0.012)
    add_cylinder("CouplingSleeve_Mesh", part, (-0.12, -0.43, 1.02), (0, 1, 0), 0.18, 0.22, 1, mat, 32, 0.012)

    part = make_part(root, "BLOWER_HOUSING", "송풍기 케이싱", (0.36, 0, 1.05))
    add_cylinder("ScrollShell_Mesh", part, (0.30, 0.03, 1.08), (0, 1, 0), 0.70, 0.48, 0, mat, 56, 0.035)
    add_torus("ScrollRim_Mesh", part, (0.30, -0.23, 1.08), (0, 1, 0), 0.66, 0.09, 0, mat, 56, 10)
    add_cylinder("FrontScrollPlate_Mesh", part, (0.30, -0.27, 1.08), (0, 1, 0), 0.59, 0.07, 0, mat, 56, 0.022)
    add_cylinder("RearScrollPlate_Mesh", part, (0.30, 0.32, 1.08), (0, 1, 0), 0.60, 0.07, 8, mat, 48, 0.022)
    add_torus("InletLip_Mesh", part, (0.30, -0.33, 1.08), (0, 1, 0), 0.39, 0.06, 3, mat, 48, 9)
    add_bolts(part, (0.30, -0.315, 1.08), (0, 1, 0), 10, 0.53, 0.035, 2, mat, 0.04)
    add_cube("OutletNeck_Mesh", part, (0.52, 0.02, 1.79), (0.47, 0.54, 0.48), 0, mat, 0.07)
    add_cube("ScrollFoot_Mesh", part, (0.30, 0.05, 0.53), (0.82, 0.56, 0.20), 2, mat, 0.03)

    part = make_part(root, "IMPELLER", "임펠러", (0.30, -0.35, 1.08))
    add_cylinder("ImpellerHub_Mesh", part, (0.30, -0.33, 1.08), (0, 1, 0), 0.17, 0.20, 2, mat, 32, 0.01)
    add_fan_blades(part, "BackwardCurvedBlades_Mesh", (0.30, -0.43, 1.08), axis="Y", count=9,
                   inner=0.14, outer=0.48, tile=3, material=mat, thickness=0.065, twist=0.20)
    add_torus("ImpellerShroud_Mesh", part, (0.30, -0.47, 1.08), (0, 1, 0), 0.48, 0.027, 10, mat, 48, 7)

    part = make_part(root, "INLET_GUARD", "흡입구 보호망", (0.30, -0.40, 1.08))
    add_torus("GuardOuterRing_Mesh", part, (0.30, -0.40, 1.08), (0, 1, 0), 0.35, 0.028, 11, mat, 44, 7)
    add_torus("GuardInnerRing_Mesh", part, (0.30, -0.41, 1.08), (0, 1, 0), 0.15, 0.020, 2, mat, 32, 6)
    for i in range(12):
        a = math.pi * 2 * i / 12
        end = (0.30 + 0.34 * math.cos(a), -0.40, 1.08 + 0.34 * math.sin(a))
        add_cylinder("GuardRadialBar_Mesh", part, (0.30, -0.40, 1.08),
                     (end[0] - 0.30, end[1] + 0.40, end[2] - 1.08), 0.012, 0.68, 2, mat, 8, 0.002)
    add_cylinder("GuardCenterCap_Mesh", part, (0.30, -0.43, 1.08), (0, 1, 0), 0.09, 0.06, 1, mat, 24, 0.008)

    part = make_part(root, "OUTLET_DUCT", "토출 덕트", (0.54, 0, 1.96))
    add_cube("OutletDuctVertical_Mesh", part, (0.54, 0.02, 2.08), (0.49, 0.55, 0.60), 0, mat, 0.045)
    add_cube("OutletDuctFlange_Mesh", part, (0.54, 0.02, 2.40), (0.66, 0.72, 0.10), 2, mat, 0.025)
    for x in (0.30, 0.78):
        for y in (-0.20, 0.24):
            add_cylinder("DuctBolt_Mesh", part, (x, y, 2.46), (0, 0, 1), 0.032, 0.06, 3, mat, 12, 0.003, False)

    part = make_part(root, "PRESSURE_GAUGE", "차압계", (0.90, -0.35, 1.68))
    add_cylinder("GaugeStem_Mesh", part, (0.90, -0.37, 1.78), (0, 0, 1), 0.035, 0.20, 3, mat, 16, 0.004)
    add_cylinder("GaugeBody_Mesh", part, (0.90, -0.39, 1.94), (0, 1, 0), 0.15, 0.08, 2, mat, 32, 0.01)
    add_cylinder("GaugeFace_Mesh", part, (0.90, -0.44, 1.94), (0, 1, 0), 0.12, 0.02, 14, mat, 32, 0.003)
    add_torus("GaugeBezel_Mesh", part, (0.90, -0.455, 1.94), (0, 1, 0), 0.13, 0.014, 3, mat, 32, 7)

    part = make_part(root, "CONTROL_BOX", "제어함", (-1.06, 0.32, 1.64))
    add_cube("ControlBox_Mesh", part, (-1.06, 0.30, 1.62), (0.48, 0.42, 0.33), 2, mat, 0.032)
    add_cube("ControlBoxDoor_Mesh", part, (-1.06, 0.075, 1.62), (0.41, 0.035, 0.27), 0, mat, 0.014)
    add_cube("ControlDisplay_Mesh", part, (-1.06, 0.052, 1.68), (0.18, 0.018, 0.09), 5, mat, 0.006)
    add_cylinder("EmergencyStop_Mesh", part, (-0.88, 0.04, 1.55), (0, 1, 0), 0.05, 0.04, 7, mat, 20, 0.005)


def make_atlas_materials(asset):
    images, paths = generate_atlases(asset)
    material = create_material(asset, images)
    return material, images, paths


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.cameras, bpy.data.lights):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)


def aim_at(obj, point):
    direction = Vector(point) - obj.location
    obj.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def add_area(collection, name, location, target, energy, size, color=(1.0, 1.0, 1.0)):
    data = bpy.data.lights.new(name, type="AREA")
    data.energy = energy
    data.shape = "DISK"
    data.size = size
    data.color = color
    obj = bpy.data.objects.new(name, data)
    collection.objects.link(obj)
    obj.location = location
    aim_at(obj, target)
    return obj


def configure_studio(asset, root):
    scene = bpy.context.scene
    studio = bpy.data.collections.new("STUDIO_RENDER_ONLY")
    scene.collection.children.link(studio)
    target = asset["center"]
    camera_data = bpy.data.cameras.new("PreviewCamera")
    camera = bpy.data.objects.new("PreviewCamera", camera_data)
    studio.objects.link(camera)
    camera.location = Vector(target) + Vector(asset["camera_offset"])
    aim_at(camera, target)
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = max(asset["width"] * 1.27, asset["height"] * 1.55)
    camera.data.lens = 55
    scene.camera = camera
    add_area(studio, "Key_Area", (4.3, -4.5, 6.2), target, 1250, 5.0, (1.0, 0.87, 0.70))
    add_area(studio, "Fill_Area", (-4.5, -2.0, 3.7), target, 850, 4.0, (0.64, 0.79, 1.0))
    add_area(studio, "Rim_Area", (1.4, 4.5, 4.6), target, 1450, 3.5, (0.70, 0.86, 1.0))
    floor_mat = bpy.data.materials.new("Preview neutral floor")
    floor_mat.diffuse_color = (0.105, 0.13, 0.15, 1)
    floor_mat.use_nodes = True
    floor_mat.node_tree.nodes.get("Principled BSDF").inputs["Base Color"].default_value = (0.105, 0.13, 0.15, 1)
    floor_mat.node_tree.nodes.get("Principled BSDF").inputs["Roughness"].default_value = 0.72
    bpy.ops.mesh.primitive_cube_add(size=1, location=(0, 0, -0.11))
    floor = bpy.context.object
    floor.name = "PreviewFloor_RENDER_ONLY"
    floor.dimensions = (30, 30, 0.18)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    floor.data.materials.append(floor_mat)
    for poly in floor.data.polygons:
        poly.use_smooth = False
    for obj in (camera, floor, *list(studio.objects)):
        if obj is not root and obj.type != "EMPTY":
            obj["renderOnly"] = True
    if scene.world is None:
        scene.world = bpy.data.worlds.new("FactoryEquipmentWorld")
    scene.world.use_nodes = True
    bg = scene.world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value = (0.035, 0.05, 0.065, 1)
    bg.inputs["Strength"].default_value = 0.34
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1100
    scene.render.resolution_y = 850
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.image_settings.color_mode = "RGB"
    scene.render.film_transparent = False
    scene.render.image_settings.compression = 15
    scene.view_settings.view_transform = "AgX"
    scene.render.filepath = os.path.join(PREVIEW_DIR, asset["id"] + ".png")


def metadata_for(asset):
    return {
        "equipmentId": asset["id"],
        "equipmentName": asset["name_en"],
        "equipmentNameKo": asset["name"],
        "modelVersion": "1.0",
        "units": "meters",
        "parts": [
            {"partId": part_id, "partName": name, "nodeName": "PART_" + part_id,
             "faultExamples": faults}
            for part_id, name, faults in asset["parts"]
        ],
    }


def build_asset(asset):
    reset_scene()
    asset["seed"] = len(asset["id"]) + sum(ord(ch) for ch in asset["id"]) % 71
    material, images, map_paths = make_atlas_materials(asset)
    root = create_empty(asset["id"], display="PLAIN_AXES")
    root["equipmentId"] = asset["id"]
    root["equipmentName"] = asset["name_en"]
    root["modelVersion"] = "1.0"
    if asset["id"] == "PUMP-002":
        build_pump(root, material)
    elif asset["id"] == "WASHER-001":
        build_washer(root, material)
    else:
        build_blower(root, material)
    configure_studio(asset, root)

    model_path = os.path.join(ASSET_MODELS, asset["id"] + ".fbx")
    blend_path = os.path.join(BLEND_DIR, asset["id"] + ".blend")
    preview_path = os.path.join(PREVIEW_DIR, asset["id"] + ".png")
    metadata_path = os.path.join(METADATA_DIR, asset["id"] + ".json")

    bpy.ops.object.select_all(action="DESELECT")
    selected = [root]
    selected.extend(list(root.children_recursive))
    for obj in selected:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    bpy.ops.render.render(write_still=True)
    bpy.ops.export_scene.fbx(
        filepath=model_path,
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        apply_unit_scale=True,
        global_scale=1.0,
        axis_forward="-Z",
        axis_up="Y",
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        use_custom_props=True,
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="COPY",
    )

    with open(metadata_path, "w", encoding="utf-8") as f:
        json.dump(metadata_for(asset), f, ensure_ascii=False, indent=2)
        f.write("\n")

    expected_nodes = {"PART_" + p[0] for p in asset["parts"]}
    present_nodes = {obj.name for obj in selected if obj.type == "EMPTY"}
    if not expected_nodes.issubset(present_nodes):
        raise RuntimeError(f"Hierarchy validation failed for {asset['id']}: {expected_nodes - present_nodes}")
    if len({p[0] for p in asset["parts"]}) != len(asset["parts"]):
        raise RuntimeError(f"Duplicate partId in metadata for {asset['id']}")
    for output in [model_path, blend_path, preview_path, metadata_path, *map_paths.values()]:
        if not os.path.isfile(output) or os.path.getsize(output) == 0:
            raise RuntimeError(f"Missing or empty generated output: {output}")
    return {"id": asset["id"], "parts": len(asset["parts"]),
            "fbxBytes": os.path.getsize(model_path), "blendBytes": os.path.getsize(blend_path),
            "preview": preview_path, "textures": list(map_paths.values())}


def write_readme():
    readme = os.path.join(PACKAGE, "README.md")
    text = """# Factory equipment additions\n\n\
These three original procedural Blender models were generated for the SWHack Unity project.\n\n\
## Unity locations\n\n\
- FBX: `Assets/05_Models/`\n- Texture maps: `Assets/10_Textures/<equipmentId>/`\n- Unity materials: `Assets/04_Materials/FactoryEquipment/`\n- Unity prefabs: `Assets/03_Prefabs/FactoryEquipment/`\n\n\
Each FBX contains one equipment root and individually named `PART_<partId>` empty nodes with child meshes. The included JSON uses the same node names and stable part IDs. One Unity MeshCollider per mesh can be added by the consuming application if raycast hit points are needed.\n\n\
## Texture and Unity material setup\n\n\
Each model has Base Color, Metallic, Roughness, and tangent Normal PNG atlases at 512×512. UVs map each part to its own palette tile. Set Base Color to sRGB; set Metallic, Roughness, and Normal to linear data, with Normal imported as a Normal Map. The existing editor utility combines Metallic into RGB and `1 - Roughness` into alpha for URP/Lit, then assigns the normal map.\n\n\
## Reproduction\n\n\
Open `Scripts/create_equipment_additions.py` in Blender and run it. The source `.blend` files are saved under `Blender/`; one preview PNG and part metadata JSON are saved per model. FBX uses Unity-friendly axis conversion and meter-scale scene units.\n\n\
The Unity Editor import and prefab setup step is performed separately in the open Unity project.\n"""
    with open(readme, "w", encoding="utf-8") as f:
        f.write(text)


def main():
    for folder in (ASSET_MODELS, ASSET_TEXTURES, BLEND_DIR, METADATA_DIR, PREVIEW_DIR, SCRIPT_DIR):
        os.makedirs(folder, exist_ok=True)
    if os.path.isfile(SOURCE_PATH):
        shutil.copy2(SOURCE_PATH, os.path.join(SCRIPT_DIR, "create_equipment_additions.py"))
    write_readme()
    results = []
    for asset in ASSETS:
        results.append(build_asset(asset))
        print("FACTORY_EQUIPMENT_READY", json.dumps(results[-1], ensure_ascii=False))
    print("FACTORY_EQUIPMENT_COMPLETE", json.dumps(results, ensure_ascii=False))


main()
