using AlgoritmoPCG.Nucleo;
using UnityEngine;
using UnityEngine.Tilemaps;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AlgoritmoPCG.Visualizacao
{
    /// <summary>
    /// Camada visual do mapa: desenha um grid de Largura x Altura células em uma
    /// Tilemap isométrica.
    ///
    /// Nesta primeira etapa o visualizador só desenha um "tabuleiro de xadrez" para
    /// validar o grid. Nas próximas etapas ele passará a apenas LER a estrutura lógica
    /// do mapa (gerada no assembly Nucleo) e traduzir cada tipo de célula em um tile —
    /// sem nunca conter regras de geração ou de análise tática.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Grid))]
    public class VisualizadorGrid : MonoBehaviour
    {
        // Limite para desenhar rótulos de coordenada na Scene view sem travar o editor.
        private const int MaximoRotulosCoordenada = 1600;

        [Header("Dimensões do mapa (em células)")]
        [Tooltip("Número de colunas do grid (eixo X lógico).")]
        [SerializeField, Min(1)] private int largura = 16;

        [Tooltip("Número de linhas do grid (eixo Y lógico).")]
        [SerializeField, Min(1)] private int altura = 16;

        [Header("Referências")]
        [Tooltip("Tilemap onde o terreno é desenhado. Deve ser filha deste Grid.")]
        [SerializeField] private Tilemap tilemapTerreno;

        [Tooltip("Câmera a ser enquadrada no mapa. Se vazio, usa Camera.main.")]
        [SerializeField] private Camera cameraAlvo;

        [Header("Aparência (placeholder)")]
        [SerializeField] private Color corCelulaPar = new Color(0.55f, 0.72f, 0.45f);
        [SerializeField] private Color corCelulaImpar = new Color(0.47f, 0.64f, 0.39f);

        [Header("Câmera")]
        [SerializeField] private bool enquadrarCameraAutomaticamente = true;
        [Tooltip("Espaço extra (em unidades de mundo) ao redor do mapa ao enquadrar.")]
        [SerializeField, Min(0f)] private float margemCamera = 1f;

        [Header("Depuração (Scene view)")]
        [Tooltip("Desenha o contorno do mapa na Scene view, mesmo fora do Play Mode.")]
        [SerializeField] private bool mostrarContorno = true;
        [Tooltip("Escreve a coordenada lógica (X,Y) sobre cada célula na Scene view.")]
        [SerializeField] private bool mostrarCoordenadas = true;

        private Grid _grid;
        private Tile _tilePar;
        private Tile _tileImpar;
        private bool _redesenhoPendente;

        public int Largura => largura;
        public int Altura => altura;

        private Grid Grid => _grid != null ? _grid : (_grid = GetComponent<Grid>());

        private void Start()
        {
            Desenhar();
        }

        private void Update()
        {
            // OnValidate não pode alterar a Tilemap diretamente; o redesenho é adiado para cá.
            if (_redesenhoPendente)
            {
                _redesenhoPendente = false;
                Desenhar();
            }
        }

        private void OnValidate()
        {
            // Permite alterar largura/altura/cores no Inspector durante o Play Mode e ver o
            // resultado imediatamente.
            if (Application.isPlaying)
                _redesenhoPendente = true;
        }

        private void OnDestroy()
        {
            DestruirTiles();
        }

        /// <summary>
        /// Limpa a Tilemap e desenha novamente todas as células do grid.
        /// </summary>
        [ContextMenu("Redesenhar")]
        public void Desenhar()
        {
            if (tilemapTerreno == null)
            {
                Debug.LogWarning($"[{nameof(VisualizadorGrid)}] Nenhuma Tilemap de terreno atribuída.", this);
                return;
            }

            GarantirTiles();

            int total = largura * altura;
            var posicoes = new Vector3Int[total];
            var tiles = new TileBase[total];

            for (int y = 0; y < altura; y++)
            {
                for (int x = 0; x < largura; x++)
                {
                    int indice = y * largura + x;
                    posicoes[indice] = ParaCelula(new Coordenada(x, y));
                    tiles[indice] = (x + y) % 2 == 0 ? _tilePar : _tileImpar;
                }
            }

            tilemapTerreno.ClearAllTiles();
            tilemapTerreno.SetTiles(posicoes, tiles); // em lote: bem mais rápido que SetTile célula a célula

            if (enquadrarCameraAutomaticamente)
                EnquadrarCamera();
        }

        /// <summary>
        /// Converte uma coordenada lógica do núcleo para a célula da Tilemap.
        /// É o ÚNICO ponto de tradução lógica -> visual do grid.
        /// </summary>
        public static Vector3Int ParaCelula(Coordenada coordenada)
        {
            return new Vector3Int(coordenada.X, coordenada.Y, 0);
        }

        /// <summary>
        /// Posição, no mundo, do centro da célula indicada (útil para posicionar
        /// unidades, marcadores e gizmos de depuração).
        /// </summary>
        public Vector3 CentroNoMundo(Coordenada coordenada)
        {
            return Grid.GetCellCenterWorld(ParaCelula(coordenada));
        }

        /// <summary>
        /// Centraliza a câmera ortográfica no mapa e ajusta o zoom para que ele caiba
        /// inteiro na tela, considerando a proporção (aspect) da câmera.
        /// </summary>
        public void EnquadrarCamera()
        {
            var cam = cameraAlvo != null ? cameraAlvo : Camera.main;
            if (cam == null || !cam.orthographic)
                return;

            Bounds limites = CalcularLimitesNoMundo();
            var posicao = cam.transform.position;
            cam.transform.position = new Vector3(limites.center.x, limites.center.y, posicao.z);

            float meiaAlturaNecessaria = limites.extents.y + margemCamera;
            float meiaLarguraNecessaria = (limites.extents.x + margemCamera) / Mathf.Max(cam.aspect, 0.01f);
            cam.orthographicSize = Mathf.Max(meiaAlturaNecessaria, meiaLarguraNecessaria);
        }

        /// <summary>
        /// Caixa que envolve o mapa inteiro no espaço do mundo. Em um grid isométrico os
        /// 4 cantos do mapa formam um grande losango; a caixa é calculada a partir deles.
        /// </summary>
        private Bounds CalcularLimitesNoMundo()
        {
            Vector3[] cantos = CantosDoMapaNoMundo();
            var limites = new Bounds(cantos[0], Vector3.zero);
            for (int i = 1; i < cantos.Length; i++)
                limites.Encapsulate(cantos[i]);
            return limites;
        }

        /// <summary>
        /// Os 4 cantos do mapa no mundo, em ordem (contorno do losango).
        /// CellToWorld devolve o canto "de origem" de cada célula, então as células
        /// (0,0), (L,0), (L,A) e (0,A) correspondem exatamente às bordas do mapa.
        /// </summary>
        private Vector3[] CantosDoMapaNoMundo()
        {
            return new[]
            {
                Grid.CellToWorld(new Vector3Int(0, 0, 0)),
                Grid.CellToWorld(new Vector3Int(largura, 0, 0)),
                Grid.CellToWorld(new Vector3Int(largura, altura, 0)),
                Grid.CellToWorld(new Vector3Int(0, altura, 0)),
            };
        }

        private void GarantirTiles()
        {
            if (_tilePar == null) _tilePar = FabricaTilesPlaceholder.CriarTile(corCelulaPar, "Celula_Par");
            if (_tileImpar == null) _tileImpar = FabricaTilesPlaceholder.CriarTile(corCelulaImpar, "Celula_Impar");

            // As cores podem ter sido alteradas no Inspector desde a criação dos tiles.
            _tilePar.color = corCelulaPar;
            _tileImpar.color = corCelulaImpar;
        }

        private void DestruirTiles()
        {
            if (_tilePar != null) Destroy(_tilePar);
            if (_tileImpar != null) Destroy(_tileImpar);
            _tilePar = null;
            _tileImpar = null;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Gizmos de depuração: permitem ver o tamanho do mapa e a orientação dos eixos
        /// lógicos X/Y diretamente na Scene view, sem entrar no Play Mode.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (mostrarContorno)
            {
                Vector3[] cantos = CantosDoMapaNoMundo();
                Gizmos.color = Color.yellow;
                for (int i = 0; i < cantos.Length; i++)
                    Gizmos.DrawLine(cantos[i], cantos[(i + 1) % cantos.Length]);

                // Setas indicando a direção dos eixos lógicos a partir da origem.
                Vector3 origem = Grid.CellToWorld(Vector3Int.zero);
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origem, Grid.CellToWorld(new Vector3Int(Mathf.Min(3, largura), 0, 0)));
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(origem, Grid.CellToWorld(new Vector3Int(0, Mathf.Min(3, altura), 0)));
            }

            if (mostrarCoordenadas && largura * altura <= MaximoRotulosCoordenada)
            {
                var estilo = new GUIStyle(EditorStyles.miniLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = Color.white }
                };

                for (int y = 0; y < altura; y++)
                    for (int x = 0; x < largura; x++)
                        Handles.Label(CentroNoMundo(new Coordenada(x, y)), $"{x},{y}", estilo);
            }
        }
#endif
    }
}
