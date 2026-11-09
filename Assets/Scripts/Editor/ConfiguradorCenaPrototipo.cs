using AlgoritmoPCG.Nucleo;
using AlgoritmoPCG.Visualizacao;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

namespace AlgoritmoPCG.Ferramentas
{
    /// <summary>
    /// Ferramenta de editor que monta, do zero, a cena do protótipo com a hierarquia
    /// correta (Câmera + Grid isométrico + Tilemap + VisualizadorMapa + ControladorPrototipo).
    ///
    /// Montar a cena por código garante que a configuração (Cell Layout, Cell Size,
    /// material, referências) seja reproduzível e documentada, em vez de depender de
    /// cliques manuais no editor.
    /// </summary>
    public static class ConfiguradorCenaPrototipo
    {
        public const string CaminhoCena = "Assets/Scenes/Prototipo.unity";

        // Material "Unlit" de sprite do URP: as cores dos tiles aparecem exatamente como
        // definidas, sem depender de luzes 2D na cena.
        private const string CaminhoMaterialUnlit =
            "Packages/com.unity.render-pipelines.universal/Runtime/Materials/Sprite-Unlit-Default.mat";

        [MenuItem("PCG/Criar Cena do Protótipo")]
        public static void CriarCenaPrototipo()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!Application.isBatchMode && AssetDatabase.LoadAssetAtPath<SceneAsset>(CaminhoCena) != null &&
                !EditorUtility.DisplayDialog("Cena já existe",
                    $"{CaminhoCena} já existe. Deseja recriá-la (as alterações nela serão perdidas)?",
                    "Recriar", "Cancelar"))
                return;

            Scene cena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Camera camera = CriarCamera();
            Tilemap tilemap = CriarGridIsometrico(out GameObject objetoMapa);

            var visualizador = objetoMapa.AddComponent<VisualizadorMapa>();
            var visualizadorSerializado = new SerializedObject(visualizador);
            visualizadorSerializado.FindProperty("tilemapTerreno").objectReferenceValue = tilemap;
            visualizadorSerializado.FindProperty("cameraAlvo").objectReferenceValue = camera;
            visualizadorSerializado.ApplyModifiedPropertiesWithoutUndo();

            var controlador = objetoMapa.AddComponent<ControladorPrototipo>();
            var controladorSerializado = new SerializedObject(controlador);
            controladorSerializado.FindProperty("visualizador").objectReferenceValue = visualizador;
            controladorSerializado.ApplyModifiedPropertiesWithoutUndo();

            // Pré-visualiza o mapa padrão e enquadra a câmera já no editor, para a Scene e a
            // Game view ficarem coerentes antes do Play.
            visualizador.DefinirPreVisualizacao(MapasDeExemplo.CriarDemonstracao(
                controladorSerializado.FindProperty("largura").intValue,
                controladorSerializado.FindProperty("altura").intValue));
            visualizador.EnquadrarCamera();

            EditorSceneManager.SaveScene(cena, CaminhoCena);
            Debug.Log($"[PCG] Cena do protótipo criada em {CaminhoCena}. Entre no Play Mode para ver o grid.");
        }

        private static Camera CriarCamera()
        {
            var objeto = new GameObject("Main Camera") { tag = "MainCamera" };
            objeto.transform.position = new Vector3(0f, 0f, -10f);

            var camera = objeto.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 6f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.13f, 0.16f);
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 1000f;
            return camera;
        }

        private static Tilemap CriarGridIsometrico(out GameObject objetoMapa)
        {
            objetoMapa = new GameObject("MapaTatico");
            var grid = objetoMapa.AddComponent<Grid>();
            grid.cellLayout = GridLayout.CellLayout.Isometric;
            grid.cellSize = new Vector3(1f, 0.5f, 1f); // losango 2:1, padrão isométrico clássico
            grid.cellSwizzle = GridLayout.CellSwizzle.XYZ;

            var objetoTilemap = new GameObject("Tilemap_Terreno");
            objetoTilemap.transform.SetParent(objetoMapa.transform, false);

            var tilemap = objetoTilemap.AddComponent<Tilemap>();
            var renderer = objetoTilemap.AddComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;

            var materialUnlit = AssetDatabase.LoadAssetAtPath<Material>(CaminhoMaterialUnlit);
            if (materialUnlit != null)
                renderer.sharedMaterial = materialUnlit;
            else
                Debug.LogWarning("[PCG] Material Sprite-Unlit-Default do URP não encontrado; usando o padrão.");

            return tilemap;
        }
    }
}
