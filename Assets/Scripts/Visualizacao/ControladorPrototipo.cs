using System;
using AlgoritmoPCG.Nucleo;
using UnityEngine;

namespace AlgoritmoPCG.Visualizacao
{
    /// <summary>De onde vem o mapa exibido no protótipo.</summary>
    public enum FonteMapa
    {
        /// <summary>Mapa de demonstração paramétrico (<see cref="MapasDeExemplo.CriarDemonstracao"/>).</summary>
        Demonstracao,

        /// <summary>Mapa desenhado em texto ASCII no próprio Inspector.</summary>
        Texto,
    }

    /// <summary>
    /// Ponto de entrada do protótipo: obtém um <see cref="MapaTatico"/> (por enquanto
    /// feito à mão; na Etapa 6, do gerador procedural) e o entrega ao
    /// <see cref="VisualizadorMapa"/>.
    ///
    /// Fica separado do visualizador para respeitar a divisão de papéis:
    /// "quem produz o mapa" ≠ "quem desenha o mapa".
    /// </summary>
    [DisallowMultipleComponent]
    public class ControladorPrototipo : MonoBehaviour
    {
        [SerializeField] private VisualizadorMapa visualizador;

        [Header("Origem do mapa")]
        [SerializeField] private FonteMapa fonte = FonteMapa.Demonstracao;

        [Header("Demonstração (tamanho configurável)")]
        [SerializeField, Min(MapasDeExemplo.LarguraMinimaDemonstracao)] private int largura = 16;
        [SerializeField, Min(MapasDeExemplo.AlturaMinimaDemonstracao)] private int altura = 12;

        [Header("Texto  ( . piso  # parede  h meia  H total  A/B spawn )")]
        [Tooltip("A primeira linha é a de maior Y. Todas as linhas devem ter o mesmo tamanho.")]
        [SerializeField, TextArea(6, 40)] private string mapaEmTexto = MapasDeExemplo.TextoMapaDeTeste;

        private bool _reconstrucaoPendente;

        /// <summary>Mapa atualmente exibido.</summary>
        public MapaTatico MapaAtual { get; private set; }

        private void Reset()
        {
            visualizador = GetComponent<VisualizadorMapa>();
        }

        private void Start()
        {
            Reconstruir();
        }

        private void Update()
        {
            if (_reconstrucaoPendente)
            {
                _reconstrucaoPendente = false;
                Reconstruir();
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                // A Tilemap não pode ser alterada dentro do OnValidate; adia para o Update.
                _reconstrucaoPendente = true;
            }
            else if (visualizador != null)
            {
                // Fora do Play Mode: só pré-visualização por gizmos (não altera a cena salva).
                visualizador.DefinirPreVisualizacao(ConstruirMapa(registrarErros: false));
            }
        }

        /// <summary>Constrói o mapa a partir da fonte escolhida e manda desenhá-lo.</summary>
        [ContextMenu("Reconstruir mapa")]
        public void Reconstruir()
        {
            var mapa = ConstruirMapa(registrarErros: true);
            if (mapa == null) return;

            MapaAtual = mapa;
            if (visualizador != null)
                visualizador.Desenhar(mapa);
            else
                Debug.LogWarning($"[{nameof(ControladorPrototipo)}] Nenhum VisualizadorMapa atribuído.", this);
        }

        private MapaTatico ConstruirMapa(bool registrarErros)
        {
            try
            {
                switch (fonte)
                {
                    case FonteMapa.Texto:
                        return ConversorMapaTexto.Ler(mapaEmTexto ?? string.Empty);
                    default:
                        return MapasDeExemplo.CriarDemonstracao(largura, altura);
                }
            }
            catch (Exception erro) when (erro is FormatException || erro is ArgumentException)
            {
                if (registrarErros)
                    Debug.LogWarning($"[{nameof(ControladorPrototipo)}] Mapa inválido: {erro.Message}", this);
                return null;
            }
        }
    }
}
