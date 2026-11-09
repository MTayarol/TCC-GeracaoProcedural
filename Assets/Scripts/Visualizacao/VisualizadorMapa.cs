using AlgoritmoPCG.Nucleo;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlgoritmoPCG.Visualizacao
{
    /// <summary>
    /// Camada visual do mapa: traduz um <see cref="MapaTatico"/> (dados do núcleo) em
    /// tiles de uma Tilemap isométrica, uma cor por tipo de célula.
    ///
    /// O visualizador apenas LÊ o mapa — não decide nada sobre geração ou regras. Quem
    /// fornece o mapa é outro componente (por ora o <see cref="ControladorPrototipo"/>;
    /// mais adiante, o gerador procedural).
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Grid))]
    public class VisualizadorMapa : MonoBehaviour
    {
        // Limites para os gizmos não deixarem a Scene view lenta em mapas grandes.
        private const int MaximoRotulosCoordenada = 1600;
        private const int MaximoCelulasPreVisualizacao = 10000;

        [Header("Referências")]
        [Tooltip("Tilemap onde o terreno é desenhado. Deve ser filha deste Grid.")]
        [SerializeField] private Tilemap tilemapTerreno;

        [Tooltip("Câmera a ser enquadrada no mapa. Se vazio, usa Camera.main.")]
        [SerializeField] private Camera cameraAlvo;

        [Header("Aparência (placeholder)")]
        [SerializeField] private PaletaCores paleta = new PaletaCores();

        [Header("Câmera")]
        [SerializeField] private bool enquadrarCameraAutomaticamente = true;
        [Tooltip("Espaço extra (em unidades de mundo) ao redor do mapa ao enquadrar.")]
        [SerializeField, Min(0f)] private float margemCamera = 1f;

        [Header("Depuração (Scene view)")]
        [Tooltip("Fora do Play Mode, pinta as células com as cores da paleta (pré-visualização).")]
        [SerializeField] private bool mostrarPreVisualizacao = true;
        [Tooltip("Desenha o contorno do mapa e os eixos lógicos X (vermelho) e Y (ciano).")]
        [SerializeField] private bool mostrarContorno = true;
        [Tooltip("Escreve a coordenada lógica (X,Y) sobre cada célula.")]
        [SerializeField] private bool mostrarCoordenadas = false;

        private Grid _grid;
        private readonly Tile[] _tiles = new Tile[PaletaCores.TotalCategorias];
        private MapaTatico _mapa;

        /// <summary>Último mapa desenhado (ou pré-visualizado).</summary>
        public MapaTatico Mapa => _mapa;

        private Grid Grid => _grid != null ? _grid : (_grid = GetComponent<Grid>());

        private void OnDestroy()
        {
            for (int i = 0; i < _tiles.Length; i++)
            {
                if (_tiles[i] != null) Destroy(_tiles[i]);
                _tiles[i] = null;
            }
        }

        /// <summary>
        /// Limpa a Tilemap e desenha todas as células do mapa informado.
        /// </summary>
        public void Desenhar(MapaTatico mapa)
        {
            _mapa = mapa;
            if (mapa == null) return;

            if (tilemapTerreno == null)
            {
                Debug.LogWarning($"[{nameof(VisualizadorMapa)}] Nenhuma Tilemap de terreno atribuída.", this);
                return;
            }

            AtualizarTiles();

            var posicoes = new Vector3Int[mapa.TotalCelulas];
            var tiles = new TileBase[mapa.TotalCelulas];
            int i = 0;
            foreach (var c in mapa.TodasAsCoordenadas())
            {
                posicoes[i] = ParaCelula(c);
                tiles[i] = _tiles[PaletaCores.Categoria(mapa.ObterTipo(c), mapa.ObterLadoSpawn(c))];
                i++;
            }

            tilemapTerreno.ClearAllTiles();
            tilemapTerreno.SetTiles(posicoes, tiles); // em lote: bem mais rápido que SetTile célula a célula

            if (enquadrarCameraAutomaticamente)
                EnquadrarCamera();
        }

        /// <summary>
        /// Guarda o mapa apenas para os gizmos da Scene view (fora do Play Mode), sem
        /// mexer na Tilemap — assim a cena salva não é alterada pela pré-visualização.
        /// </summary>
        public void DefinirPreVisualizacao(MapaTatico mapa)
        {
            _mapa = mapa;
#if UNITY_EDITOR
            SceneView.RepaintAll();
#endif
        }

        /// <summary>
        /// Converte uma coordenada lógica do núcleo para a célula da Tilemap.
        /// É o ÚNICO ponto de tradução lógica -> visual do grid.
        /// </summary>
        public static Vector3Int ParaCelula(Coordenada coordenada)
        {
            return new Vector3Int(coordenada.X, coordenada.Y, 0);
        }

        /// <summary>Posição, no mundo, do centro da célula indicada.</summary>
        public Vector3 CentroNoMundo(Coordenada coordenada)
        {
            return Grid.GetCellCenterWorld(ParaCelula(coordenada));
        }

        /// <summary>
        /// Centraliza a câmera ortográfica no mapa atual e ajusta o zoom para que ele caiba
        /// inteiro na tela, considerando a proporção (aspect) da câmera.
        /// </summary>
        public void EnquadrarCamera()
        {
            var cam = cameraAlvo != null ? cameraAlvo : Camera.main;
            if (cam == null || !cam.orthographic || _mapa == null)
                return;

            Vector3[] cantos = CantosDoMapaNoMundo(_mapa);
            var limites = new Bounds(cantos[0], Vector3.zero);
            for (int i = 1; i < cantos.Length; i++)
                limites.Encapsulate(cantos[i]);

            var posicao = cam.transform.position;
            cam.transform.position = new Vector3(limites.center.x, limites.center.y, posicao.z);

            float meiaAlturaNecessaria = limites.extents.y + margemCamera;
            float meiaLarguraNecessaria = (limites.extents.x + margemCamera) / Mathf.Max(cam.aspect, 0.01f);
            cam.orthographicSize = Mathf.Max(meiaAlturaNecessaria, meiaLarguraNecessaria);
        }

        /// <summary>
        /// Os 4 cantos do mapa no mundo, em ordem (contorno do grande losango).
        /// CellToWorld devolve o canto "de origem" de cada célula, então as células
        /// (0,0), (L,0), (L,A) e (0,A) correspondem exatamente às bordas do mapa.
        /// </summary>
        private Vector3[] CantosDoMapaNoMundo(MapaTatico mapa)
        {
            return new[]
            {
                Grid.CellToWorld(new Vector3Int(0, 0, 0)),
                Grid.CellToWorld(new Vector3Int(mapa.Largura, 0, 0)),
                Grid.CellToWorld(new Vector3Int(mapa.Largura, mapa.Altura, 0)),
                Grid.CellToWorld(new Vector3Int(0, mapa.Altura, 0)),
            };
        }

        private void AtualizarTiles()
        {
            for (int categoria = 0; categoria < _tiles.Length; categoria++)
            {
                if (_tiles[categoria] == null)
                    _tiles[categoria] = FabricaTilesPlaceholder.CriarTile(paleta.CorDaCategoria(categoria), $"Tile_Categoria{categoria}");

                // As cores podem ter sido alteradas no Inspector desde a criação dos tiles.
                _tiles[categoria].color = paleta.CorDaCategoria(categoria);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (_mapa == null) return;

            if (mostrarPreVisualizacao && !Application.isPlaying && _mapa.TotalCelulas <= MaximoCelulasPreVisualizacao)
                DesenharPreVisualizacao();

            if (mostrarContorno)
                DesenharContornoEEixos();

            if (mostrarCoordenadas && _mapa.TotalCelulas <= MaximoRotulosCoordenada)
                DesenharCoordenadas();
        }

        private void DesenharPreVisualizacao()
        {
            var losango = new Vector3[4];
            foreach (var c in _mapa.TodasAsCoordenadas())
            {
                // Os 4 cantos do losango da célula, levemente encolhidos para mostrar a grade.
                Vector3 centro = CentroNoMundo(c);
                losango[0] = Vector3.Lerp(centro, Grid.CellToWorld(new Vector3Int(c.X, c.Y, 0)), 0.92f);
                losango[1] = Vector3.Lerp(centro, Grid.CellToWorld(new Vector3Int(c.X + 1, c.Y, 0)), 0.92f);
                losango[2] = Vector3.Lerp(centro, Grid.CellToWorld(new Vector3Int(c.X + 1, c.Y + 1, 0)), 0.92f);
                losango[3] = Vector3.Lerp(centro, Grid.CellToWorld(new Vector3Int(c.X, c.Y + 1, 0)), 0.92f);

                Handles.color = paleta.CorDe(_mapa.ObterTipo(c), _mapa.ObterLadoSpawn(c));
                Handles.DrawAAConvexPolygon(losango);
            }
        }

        private void DesenharContornoEEixos()
        {
            Vector3[] cantos = CantosDoMapaNoMundo(_mapa);
            Gizmos.color = Color.yellow;
            for (int i = 0; i < cantos.Length; i++)
                Gizmos.DrawLine(cantos[i], cantos[(i + 1) % cantos.Length]);

            Vector3 origem = Grid.CellToWorld(Vector3Int.zero);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(origem, Grid.CellToWorld(new Vector3Int(Mathf.Min(3, _mapa.Largura), 0, 0)));
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(origem, Grid.CellToWorld(new Vector3Int(0, Mathf.Min(3, _mapa.Altura), 0)));
        }

        private void DesenharCoordenadas()
        {
            var estilo = new GUIStyle(EditorStyles.miniLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            foreach (var c in _mapa.TodasAsCoordenadas())
                Handles.Label(CentroNoMundo(c), $"{c.X},{c.Y}", estilo);
        }
#endif
    }
}
