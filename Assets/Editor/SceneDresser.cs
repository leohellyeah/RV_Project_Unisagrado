// SceneDresser.cs
// Utilitario de Editor para "vestir" as cenas de continente com uma paleta de cores
// basica + props placeholder feitos de primitivas (sem assets externos).
//
// Como usar (no Unity):
//   menu  Tools > VR Project > Dress ALL Scenes      (faz Europe/America/Asia/Africa)
//   menu  Tools > VR Project > Dress CURRENT Scene   (so a cena aberta)
//   menu  Tools > VR Project > Remove Dressing (current)   (desfaz na cena aberta)
//
// E idempotente: rodar de novo apaga o "SceneDressing" anterior e recria.
// Materiais/mesh ficam salvos em Assets/Materiais/Dressing/.
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using ProjetoRV.NPC;

public static class SceneDresser
{
    const string DRESS_ROOT = "SceneDressing";
    const string MAT_DIR = "Assets/Materiais/Dressing";

    // cache p/ evitar CreateAsset duplicado no mesmo caminho durante um Dress ALL
    static readonly Dictionary<string, Material> _matCache = new Dictionary<string, Material>();

    static readonly string[] CONTINENT_SCENES =
    {
        "Assets/Cenas/Scene_Europe.unity",
        "Assets/Cenas/Scene_America.unity",
        "Assets/Cenas/Scene_Asia.unity",
        "Assets/Cenas/Scene_Africa.unity",
        "Assets/Cenas/Scene_Lobby.unity",
    };

    struct Theme
    {
        public Color ground, skyTint, ambient, sun, fog;
        public float sunIntensity, planeScale;
    }

    // ---------------------------------------------------------------- menus
    [MenuItem("Tools/VR Project/Dress ALL Scenes")]
    public static void DressAll()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        _matCache.Clear();
        foreach (var path in CONTINENT_SCENES)
        {
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            DressOpenScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[SceneDresser] vestida: " + path);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SceneDresser] Concluido. Abra cada cena e tire os screenshots.");
    }

    [MenuItem("Tools/VR Project/Dress CURRENT Scene")]
    public static void DressCurrent()
    {
        _matCache.Clear();
        DressOpenScene();
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Tools/VR Project/Dress LOBBY")]
    public static void DressLobbyMenu()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        _matCache.Clear();
        var scene = EditorSceneManager.OpenScene("Assets/Cenas/Scene_Lobby.unity", OpenSceneMode.Single);
        DressOpenScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SceneDresser] Lobby vestido.");
    }

    [MenuItem("Tools/VR Project/Remove Dressing (current)")]
    public static void RemoveDressing()
    {
        var existing = GameObject.Find(DRESS_ROOT);
        if (existing) Object.DestroyImmediate(existing);
        RenderSettings.fog = false;
        var scene = SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ------------------------------------------------------------- core
    static void DressOpenScene()
    {
        string name = SceneManager.GetActiveScene().name;
        Theme t = GetTheme(name);
        EnsureMatDir();

        // chao
        var plane = GameObject.Find("Plane");
        if (plane)
        {
            plane.transform.localScale = new Vector3(t.planeScale, 1f, t.planeScale);
            var r = plane.GetComponent<Renderer>();
            if (r) r.sharedMaterial = SolidMat("Ground_" + name, t.ground);
        }

        // ceu + ambiente + neblina
        RenderSettings.skybox = SkyMat("Sky_" + name, t.skyTint);
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = t.ambient;
        RenderSettings.fog = true;
        RenderSettings.fogColor = t.fog;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;

        var sun = FindSun();
        if (sun)
        {
            sun.color = t.sun;
            sun.intensity = t.sunIntensity;
            sun.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
            sun.shadows = LightShadows.Soft;
        }
        DynamicGI.UpdateEnvironment();

        // raiz dos props (idempotente)
        var old = GameObject.Find(DRESS_ROOT);
        if (old) Object.DestroyImmediate(old);
        var root = new GameObject(DRESS_ROOT).transform;

        Vector3 npc = Vector3.zero;
        var story = Object.FindObjectOfType<NpcStory>();
        if (story) npc = story.transform.position;
        npc.y = 0f;

        switch (name)
        {
            case "Scene_Europe": DressEurope(root, npc); break;
            case "Scene_America": DressAmerica(root, npc); break;
            case "Scene_Asia": DressAsia(root, npc); break;
            case "Scene_Africa": DressAfrica(root, npc); break;
            case "Scene_Lobby": DressLobby(root); break;
        }
    }

    static Theme GetTheme(string scene)
    {
        switch (scene)
        {
            case "Scene_Europe":  // medieval: verde-musgo / ceu frio
                return new Theme { ground = new Color(0.27f, 0.33f, 0.23f), skyTint = new Color(0.45f, 0.55f, 0.72f), ambient = new Color(0.42f, 0.45f, 0.48f), sun = new Color(1f, 0.96f, 0.85f), fog = new Color(0.55f, 0.62f, 0.72f), sunIntensity = 1.0f, planeScale = 2f };
            case "Scene_America": // floresta brasileira: verde vivo
                return new Theme { ground = new Color(0.19f, 0.41f, 0.17f), skyTint = new Color(0.52f, 0.7f, 0.6f), ambient = new Color(0.48f, 0.52f, 0.42f), sun = new Color(1f, 0.98f, 0.9f), fog = new Color(0.6f, 0.72f, 0.6f), sunIntensity = 1.1f, planeScale = 2f };
            case "Scene_Asia":    // japao: pedra + ceu rosado (sakura)
                return new Theme { ground = new Color(0.40f, 0.44f, 0.37f), skyTint = new Color(0.86f, 0.62f, 0.72f), ambient = new Color(0.52f, 0.48f, 0.5f), sun = new Color(1f, 0.92f, 0.9f), fog = new Color(0.9f, 0.76f, 0.82f), sunIntensity = 1.0f, planeScale = 2f };
            case "Scene_Africa":  // egito: areia + ceu quente
                return new Theme { ground = new Color(0.84f, 0.71f, 0.44f), skyTint = new Color(0.92f, 0.76f, 0.5f), ambient = new Color(0.6f, 0.54f, 0.42f), sun = new Color(1f, 0.93f, 0.74f), fog = new Color(0.92f, 0.82f, 0.62f), sunIntensity = 1.2f, planeScale = 3f };
            case "Scene_Lobby":   // hub: piso escuro + ceu crepusculo
                return new Theme { ground = new Color(0.16f, 0.17f, 0.22f), skyTint = new Color(0.32f, 0.34f, 0.55f), ambient = new Color(0.40f, 0.40f, 0.50f), sun = new Color(0.95f, 0.95f, 1f), fog = new Color(0.30f, 0.32f, 0.46f), sunIntensity = 0.9f, planeScale = 2f };
            default:
                return new Theme { ground = new Color(0.5f, 0.5f, 0.5f), skyTint = new Color(0.5f, 0.5f, 0.6f), ambient = new Color(0.5f, 0.5f, 0.5f), sun = Color.white, fog = new Color(0.5f, 0.5f, 0.5f), sunIntensity = 1f, planeScale = 2f };
        }
    }

    // -------------------------------------------------------- props por cena
    static void DressEurope(Transform root, Vector3 npc)
    {
        var stone = SolidMat("Stone", new Color(0.5f, 0.5f, 0.52f));
        var wood = SolidMat("WoodBrown", new Color(0.40f, 0.26f, 0.13f));
        var pine = SolidMat("PineGreen", new Color(0.15f, 0.30f, 0.16f));

        Campfire(root, npc + new Vector3(2.4f, 0f, 1.0f));
        Tree(root, npc + new Vector3(-3.0f, 0f, 2.2f), wood, pine, 2.6f, 1.8f);
        Tree(root, npc + new Vector3(-4.2f, 0f, -1.0f), wood, pine, 2.1f, 1.5f);
        // ruinas de pedra
        for (int i = 0; i < 3; i++)
            Prim(root, PrimitiveType.Cube, npc + new Vector3(3.4f + i * 1.1f, 0.8f, -3.0f), new Vector3(0.6f, 1.6f, 0.6f), stone);
        Prim(root, PrimitiveType.Cube, npc + new Vector3(1.4f, 0.4f, -2.0f), Vector3.one * 0.8f, wood); // caixote
    }

    static void DressAmerica(Transform root, Vector3 npc)
    {
        var wood = SolidMat("WoodBrown", new Color(0.40f, 0.26f, 0.13f));
        var g1 = SolidMat("JungleGreen", new Color(0.13f, 0.42f, 0.15f));
        var g2 = SolidMat("JungleGreen2", new Color(0.21f, 0.53f, 0.22f));
        var rock = SolidMat("Stone", new Color(0.5f, 0.5f, 0.52f));

        Tree(root, npc + new Vector3(-3.0f, 0f, 2.5f), wood, g1, 2.7f, 2.0f);
        Tree(root, npc + new Vector3(3.2f, 0f, 2.0f), wood, g2, 3.1f, 2.3f);
        Tree(root, npc + new Vector3(-2.5f, 0f, -2.8f), wood, g2, 2.3f, 1.8f);
        Tree(root, npc + new Vector3(2.8f, 0f, -2.6f), wood, g1, 2.9f, 2.1f);
        Prim(root, PrimitiveType.Sphere, npc + new Vector3(1.5f, 0.3f, 1.4f), Vector3.one * 0.8f, g1);
        Prim(root, PrimitiveType.Sphere, npc + new Vector3(-1.5f, 0.3f, -1.0f), Vector3.one * 0.7f, g2);
        Prim(root, PrimitiveType.Sphere, npc + new Vector3(0.6f, 0.2f, -2.0f), new Vector3(0.9f, 0.5f, 0.9f), rock);
    }

    static void DressAsia(Transform root, Vector3 npc)
    {
        var red = SolidMat("ToriiRed", new Color(0.78f, 0.12f, 0.12f));
        var glow = EmissiveMat("LanternGlow", new Color(1f, 0.55f, 0.3f), 1.6f);
        var wood = SolidMat("WoodBrown", new Color(0.40f, 0.26f, 0.13f));
        var pink = SolidMat("CherryPink", new Color(0.95f, 0.62f, 0.76f));

        Torii(root, npc + new Vector3(0f, 0f, 4.0f), red);
        Lantern(root, npc + new Vector3(-1.6f, 0f, 2.2f), red, glow);
        Lantern(root, npc + new Vector3(1.6f, 0f, 2.2f), red, glow);
        Tree(root, npc + new Vector3(-3.0f, 0f, 0f), wood, pink, 2.5f, 1.9f);
        Tree(root, npc + new Vector3(3.0f, 0f, -1.0f), wood, pink, 2.3f, 1.7f);
    }

    static void DressAfrica(Transform root, Vector3 npc)
    {
        var sand = SolidMat("Sand", new Color(0.82f, 0.70f, 0.42f));
        var sandD = SolidMat("SandDark", new Color(0.72f, 0.58f, 0.32f));
        var wood = SolidMat("WoodBrown", new Color(0.40f, 0.26f, 0.13f));
        var palm = SolidMat("PalmGreen", new Color(0.20f, 0.45f, 0.20f));
        var mesh = PyramidMesh();

        Pyramid(root, npc + new Vector3(-5.5f, 0f, 7.0f), 5.5f, 4.5f, sand, mesh);
        Pyramid(root, npc + new Vector3(2.5f, 0f, 9.0f), 7.5f, 6.5f, sandD, mesh);
        Pyramid(root, npc + new Vector3(7.0f, 0f, 3.5f), 3.5f, 3.0f, sand, mesh);
        Prim(root, PrimitiveType.Cube, npc + new Vector3(-3.0f, 1.6f, 1.5f), new Vector3(0.5f, 3.2f, 0.5f), sandD); // obelisco
        Palm(root, npc + new Vector3(3.0f, 0f, -2.0f), wood, palm);
        Palm(root, npc + new Vector3(-2.5f, 0f, -2.5f), wood, palm);
    }

    static void DressLobby(Transform root)
    {
        // chao/ceu/luz ja foram aplicados em DressOpenScene; aqui sao os portais.
        var names = new[] { "Portal_Europe", "Portal_America", "Portal_Asia", "Portal_Africa" };
        var cols = new[]
        {
            new Color(0.30f, 0.55f, 0.95f), // azul (Europa)
            new Color(0.25f, 0.75f, 0.30f), // verde (Brasil)
            new Color(0.95f, 0.30f, 0.45f), // vermelho/rosa (Japao)
            new Color(0.95f, 0.72f, 0.25f), // dourado (Egito)
        };
        // centro = centroide dos portais (robusto: nao depende da origem nem da rotacao deles)
        Vector3 sum = Vector3.zero; int cnt = 0;
        foreach (var n in names)
        {
            var g = GameObject.Find(n);
            if (g) { sum += g.transform.position; cnt++; }
        }
        Vector3 center = cnt > 0 ? sum / cnt : Vector3.zero;

        for (int i = 0; i < names.Length; i++)
            DressPortal(root, names[i], cols[i], center);
    }

    static void DressPortal(Transform root, string portalName, Color c, Vector3 center)
    {
        var portal = GameObject.Find(portalName);
        if (!portal) return;

        // o slab do portal passa a brilhar na cor do continente
        var rend = portal.GetComponent<MeshRenderer>();
        if (rend) rend.sharedMaterial = EmissiveMat("Portal_" + portalName, c, 1.3f);

        Vector3 s = portal.transform.localScale;
        float w = Mathf.Max(s.x, 1f), h = Mathf.Max(s.y, 2f);

        // no auxiliar: na base do portal, SEMPRE encarando o centro do lobby
        var node = new GameObject(portalName + "_FX").transform;
        node.SetParent(root, false);
        node.position = new Vector3(portal.transform.position.x, 0f, portal.transform.position.z);
        Vector3 toCenter = center - node.position; toCenter.y = 0f;
        node.rotation = toCenter.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(toCenter.normalized, Vector3.up)
            : Quaternion.Euler(0f, portal.transform.eulerAngles.y, 0f);

        var frame = SolidMat("PortalFrame", new Color(0.13f, 0.13f, 0.16f));
        float px = w / 2f + 0.35f;
        Prim(node, PrimitiveType.Cube, new Vector3(-px, h / 2f, 0f), new Vector3(0.25f, h, 0.25f), frame);
        Prim(node, PrimitiveType.Cube, new Vector3(px, h / 2f, 0f), new Vector3(0.25f, h, 0.25f), frame);
        Prim(node, PrimitiveType.Cube, new Vector3(0f, h + 0.15f, 0f), new Vector3(2f * px + 0.25f, 0.3f, 0.4f), frame);
        Prim(node, PrimitiveType.Cylinder, new Vector3(0f, 0.06f, 0f), new Vector3(w + 1.4f, 0.12f, w + 1.4f), SolidMat("PortalBase_" + portalName, c));

        var lgo = new GameObject("PortalLight");
        lgo.transform.SetParent(node, false);
        lgo.transform.localPosition = new Vector3(0f, h * 0.6f, 0.7f);
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = c; l.range = 8f; l.intensity = 2.6f;
    }

    // -------------------------------------------------------- construtores
    static void Campfire(Transform parent, Vector3 c)
    {
        var stone = SolidMat("Stone", new Color(0.5f, 0.5f, 0.52f));
        var wood = SolidMat("WoodBrown", new Color(0.40f, 0.26f, 0.13f));
        var flame = EmissiveMat("Flame", new Color(1f, 0.5f, 0.12f), 2.5f);
        int n = 8; float rad = 0.6f;
        for (int i = 0; i < n; i++)
        {
            float a = i / (float)n * Mathf.PI * 2f;
            Prim(parent, PrimitiveType.Sphere, c + new Vector3(Mathf.Cos(a) * rad, 0.08f, Mathf.Sin(a) * rad), Vector3.one * 0.25f, stone);
        }
        Prim(parent, PrimitiveType.Cylinder, c + new Vector3(0f, 0.12f, 0f), new Vector3(0.12f, 0.4f, 0.12f), wood, Quaternion.Euler(78f, 0f, 0f));
        Prim(parent, PrimitiveType.Cylinder, c + new Vector3(0f, 0.12f, 0f), new Vector3(0.12f, 0.4f, 0.12f), wood, Quaternion.Euler(0f, 0f, 78f));
        Prim(parent, PrimitiveType.Capsule, c + new Vector3(0f, 0.45f, 0f), new Vector3(0.3f, 0.45f, 0.3f), flame);
        var lgo = new GameObject("Campfire_Light");
        lgo.transform.SetParent(parent, false);
        lgo.transform.localPosition = c + new Vector3(0f, 0.9f, 0f);
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.6f, 0.25f); l.range = 9f; l.intensity = 2.4f;
    }

    static void Tree(Transform parent, Vector3 pos, Material trunk, Material leaf, float h, float canopy)
    {
        Prim(parent, PrimitiveType.Cylinder, pos + new Vector3(0f, h * 0.5f, 0f), new Vector3(0.25f, h * 0.5f, 0.25f), trunk);
        Prim(parent, PrimitiveType.Sphere, pos + new Vector3(0f, h + canopy * 0.35f, 0f), Vector3.one * canopy, leaf);
    }

    static void Torii(Transform parent, Vector3 c, Material red)
    {
        float h = 3f, w = 2.4f;
        Prim(parent, PrimitiveType.Cylinder, c + new Vector3(-w / 2f, h / 2f, 0f), new Vector3(0.25f, h / 2f, 0.25f), red);
        Prim(parent, PrimitiveType.Cylinder, c + new Vector3(w / 2f, h / 2f, 0f), new Vector3(0.25f, h / 2f, 0.25f), red);
        Prim(parent, PrimitiveType.Cube, c + new Vector3(0f, h + 0.05f, 0f), new Vector3(w + 0.9f, 0.3f, 0.4f), red);
        Prim(parent, PrimitiveType.Cube, c + new Vector3(0f, h - 0.5f, 0f), new Vector3(w + 0.2f, 0.2f, 0.35f), red);
    }

    static void Lantern(Transform parent, Vector3 pos, Material red, Material glow)
    {
        Prim(parent, PrimitiveType.Cylinder, pos + new Vector3(0f, 1.0f, 0f), new Vector3(0.06f, 1.0f, 0.06f), red);
        Prim(parent, PrimitiveType.Capsule, pos + new Vector3(0f, 2.0f, 0f), new Vector3(0.4f, 0.4f, 0.4f), glow);
        var lgo = new GameObject("Lantern_Light");
        lgo.transform.SetParent(parent, false);
        lgo.transform.localPosition = pos + new Vector3(0f, 2.0f, 0f);
        var l = lgo.AddComponent<Light>();
        l.type = LightType.Point; l.color = new Color(1f, 0.5f, 0.3f); l.range = 5f; l.intensity = 1.6f;
    }

    static void Palm(Transform parent, Vector3 pos, Material trunk, Material leaf)
    {
        Prim(parent, PrimitiveType.Cylinder, pos + new Vector3(0f, 1.6f, 0f), new Vector3(0.2f, 1.6f, 0.2f), trunk);
        for (int i = 0; i < 6; i++)
        {
            float a = i / 6f * Mathf.PI * 2f;
            var rot = Quaternion.Euler(38f, a * Mathf.Rad2Deg, 0f);
            Prim(parent, PrimitiveType.Capsule, pos + new Vector3(Mathf.Cos(a) * 0.6f, 3.1f, Mathf.Sin(a) * 0.6f), new Vector3(0.25f, 0.9f, 0.1f), leaf, rot);
        }
    }

    static void Pyramid(Transform parent, Vector3 pos, float baseSize, float height, Material mat, Mesh mesh)
    {
        var go = new GameObject("Pyramid");
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = new Vector3(baseSize, height, baseSize);
        go.AddComponent<MeshFilter>().sharedMesh = mesh;
        go.AddComponent<MeshRenderer>().sharedMaterial = mat;
    }

    // -------------------------------------------------------- utilitarios
    static Transform Prim(Transform parent, PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Quaternion? rot = null)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent, false);
        go.transform.localPosition = pos;
        go.transform.localScale = scale;
        if (rot.HasValue) go.transform.localRotation = rot.Value;
        var col = go.GetComponent<Collider>();
        if (col) Object.DestroyImmediate(col); // decorativo: sem fisica
        var r = go.GetComponent<Renderer>();
        if (r && mat) r.sharedMaterial = mat;
        return go.transform;
    }

    static Light FindSun()
    {
        foreach (var l in Object.FindObjectsOfType<Light>())
            if (l.type == LightType.Directional) return l;
        return null;
    }

    static void EnsureMatDir()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materiais"))
            AssetDatabase.CreateFolder("Assets", "Materiais");
        if (!AssetDatabase.IsValidFolder(MAT_DIR))
            AssetDatabase.CreateFolder("Assets/Materiais", "Dressing");
    }

    static Material SolidMat(string key, Color c)
    {
        string path = MAT_DIR + "/" + key + ".mat";
        if (_matCache.TryGetValue(path, out var hit)) return hit;
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.shader = Shader.Find("Standard");
        m.color = c;
        m.SetColor("_Color", c);
        m.SetFloat("_Glossiness", 0f);
        m.SetFloat("_Metallic", 0f);
        EditorUtility.SetDirty(m);
        _matCache[path] = m;
        return m;
    }

    static Material EmissiveMat(string key, Color c, float intensity)
    {
        string path = MAT_DIR + "/" + key + ".mat";
        if (_matCache.TryGetValue(path, out var hit)) return hit;
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Standard"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.shader = Shader.Find("Standard");
        m.color = c;
        m.SetColor("_Color", c);
        m.EnableKeyword("_EMISSION");
        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        m.SetColor("_EmissionColor", c * intensity);
        EditorUtility.SetDirty(m);
        _matCache[path] = m;
        return m;
    }

    static Material SkyMat(string key, Color tint)
    {
        string path = MAT_DIR + "/" + key + ".mat";
        if (_matCache.TryGetValue(path, out var hit)) return hit;
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(Shader.Find("Skybox/Procedural"));
            AssetDatabase.CreateAsset(m, path);
        }
        m.shader = Shader.Find("Skybox/Procedural");
        m.SetColor("_SkyTint", tint);
        m.SetColor("_GroundColor", new Color(0.45f, 0.42f, 0.38f));
        m.SetFloat("_AtmosphereThickness", 1.0f);
        m.SetFloat("_Exposure", 1.25f);
        EditorUtility.SetDirty(m);
        _matCache[path] = m;
        return m;
    }

    static Mesh PyramidMesh()
    {
        string path = MAT_DIR + "/Pyramid.asset";
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null) return existing;

        // base 1x1 centrada na origem, apice em y=1
        Vector3 b0 = new Vector3(-0.5f, 0f, -0.5f);
        Vector3 b1 = new Vector3(0.5f, 0f, -0.5f);
        Vector3 b2 = new Vector3(0.5f, 0f, 0.5f);
        Vector3 b3 = new Vector3(-0.5f, 0f, 0.5f);
        Vector3 ap = new Vector3(0f, 1f, 0f);

        var verts = new List<Vector3>();
        var tris = new List<int>();
        // verts proprios por face (flat shading) + winding com normal para fora
        void Face(Vector3 a, Vector3 b, Vector3 c)
        {
            int i = verts.Count;
            verts.Add(a); verts.Add(b); verts.Add(c);
            tris.Add(i); tris.Add(i + 1); tris.Add(i + 2);
        }
        Face(b0, b1, b2); Face(b0, b2, b3);                       // base (normal -Y)
        Face(b1, b0, ap); Face(b2, b1, ap);                       // laterais (normal p/ fora)
        Face(b3, b2, ap); Face(b0, b3, ap);

        var mesh = new Mesh { name = "Pyramid" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh, path);
        return mesh;
    }
}
#endif
