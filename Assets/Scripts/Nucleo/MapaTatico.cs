using System;
using System.Collections.Generic;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Representação lógica do mapa tático: um grid retangular de Largura x Altura
    /// células, cada uma com um <see cref="TipoCelula"/>. Células do tipo Spawn guardam
    /// também o <see cref="Lado"/> a que pertencem.
    ///
    /// É a estrutura que o gerador procedural escreve e que as regras do jogo, as
    /// métricas táticas e a simulação leem. Não conhece nada da Unity.
    ///
    /// Armazenamento: vetores unidimensionais em ordem de linha (índice = y * Largura + x).
    /// Isso é compacto, rápido de copiar (<see cref="Clonar"/>) e amigável ao cache — o
    /// que importa quando, mais adiante, milhares de mapas forem gerados e simulados em lote.
    /// </summary>
    public sealed class MapaTatico
    {
        private readonly TipoCelula[] _tipos;
        private readonly Lado[] _ladosSpawn;

        public int Largura { get; }
        public int Altura { get; }
        public int TotalCelulas => _tipos.Length;

        /// <summary>Cria um mapa com todas as células como <see cref="TipoCelula.Piso"/>.</summary>
        public MapaTatico(int largura, int altura)
        {
            if (largura < 1) throw new ArgumentOutOfRangeException(nameof(largura), largura, "A largura deve ser pelo menos 1.");
            if (altura < 1) throw new ArgumentOutOfRangeException(nameof(altura), altura, "A altura deve ser pelo menos 1.");

            Largura = largura;
            Altura = altura;
            _tipos = new TipoCelula[largura * altura]; // o valor padrão do enum é Piso
            _ladosSpawn = new Lado[largura * altura];  // o valor padrão é Lado.Nenhum
        }

        private MapaTatico(MapaTatico origem)
        {
            Largura = origem.Largura;
            Altura = origem.Altura;
            _tipos = (TipoCelula[])origem._tipos.Clone();
            _ladosSpawn = (Lado[])origem._ladosSpawn.Clone();
        }

        // ------------------------------------------------------------------
        // Leitura e escrita de células
        // ------------------------------------------------------------------

        public bool DentroDosLimites(Coordenada c)
        {
            return c.X >= 0 && c.X < Largura && c.Y >= 0 && c.Y < Altura;
        }

        public TipoCelula ObterTipo(Coordenada c) => _tipos[Indice(c)];

        /// <summary>Lado dono do spawn na célula, ou <see cref="Lado.Nenhum"/> se não for spawn.</summary>
        public Lado ObterLadoSpawn(Coordenada c) => _ladosSpawn[Indice(c)];

        /// <summary>
        /// Define o tipo de uma célula. Para criar spawns use <see cref="DefinirSpawn"/>,
        /// que exige informar o lado. Sobrescrever um spawn apaga o lado associado.
        /// </summary>
        public void DefinirTipo(Coordenada c, TipoCelula tipo)
        {
            if (tipo == TipoCelula.Spawn)
                throw new ArgumentException("Use DefinirSpawn para criar spawns (é preciso informar o lado).", nameof(tipo));

            int i = Indice(c);
            _tipos[i] = tipo;
            _ladosSpawn[i] = Lado.Nenhum;
        }

        /// <summary>Transforma a célula em spawn do lado indicado (A ou B).</summary>
        public void DefinirSpawn(Coordenada c, Lado lado)
        {
            if (lado == Lado.Nenhum)
                throw new ArgumentException("Um spawn precisa pertencer ao lado A ou B.", nameof(lado));

            int i = Indice(c);
            _tipos[i] = TipoCelula.Spawn;
            _ladosSpawn[i] = lado;
        }

        // ------------------------------------------------------------------
        // Consultas de terreno
        // ------------------------------------------------------------------

        /// <summary>Verdadeiro se a célula existe e pode ser ocupada por uma unidade.</summary>
        public bool EhTransitavel(Coordenada c)
        {
            return DentroDosLimites(c) && _tipos[Indice(c)].EhTransitavel();
        }

        /// <summary>Verdadeiro se a célula existe e é cobertura (meia, total ou parede).</summary>
        public bool EhCobertura(Coordenada c)
        {
            return DentroDosLimites(c) && _tipos[Indice(c)].EhCobertura();
        }

        // ------------------------------------------------------------------
        // Vizinhança
        // ------------------------------------------------------------------

        /// <summary>
        /// As até 8 células vizinhas (vizinhança de Moore) que existem no mapa,
        /// independentemente do tipo. Usada, por exemplo, para procurar coberturas
        /// adjacentes a uma unidade.
        /// </summary>
        public IEnumerable<Coordenada> Vizinhos8(Coordenada c)
        {
            foreach (var d in Direcoes.Oito)
            {
                var vizinho = c + d;
                if (DentroDosLimites(vizinho))
                    yield return vizinho;
            }
        }

        /// <summary>
        /// Vizinhos para os quais uma unidade em <paramref name="c"/> pode se mover em um
        /// passo, respeitando a regra de diagonais de <see cref="PodeMoverEntreAdjacentes"/>.
        /// </summary>
        public IEnumerable<Coordenada> VizinhosTransitaveis(Coordenada c)
        {
            foreach (var d in Direcoes.Oito)
            {
                var vizinho = c + d;
                if (PassoPermitido(c, d))
                    yield return vizinho;
            }
        }

        /// <summary>
        /// Indica se uma unidade pode dar um passo de <paramref name="origem"/> para
        /// <paramref name="destino"/>, que devem ser células adjacentes (8 direções).
        ///
        /// Regra das diagonais ("sem cortar quina"): um passo diagonal só é permitido se as
        /// DUAS células ortogonais que ele contorna também forem transitáveis. Assim uma
        /// unidade não atravessa a quina de uma parede nem passa espremida entre dois
        /// obstáculos que se tocam pela diagonal.
        /// </summary>
        public bool PodeMoverEntreAdjacentes(Coordenada origem, Coordenada destino)
        {
            var d = destino - origem;
            if (d == Coordenada.Zero || Math.Abs(d.X) > 1 || Math.Abs(d.Y) > 1)
                return false;
            return PassoPermitido(origem, d);
        }

        private bool PassoPermitido(Coordenada origem, Coordenada d)
        {
            if (!EhTransitavel(origem + d))
                return false;

            if (!Direcoes.EhDiagonal(d))
                return true;

            return EhTransitavel(new Coordenada(origem.X + d.X, origem.Y))
                && EhTransitavel(new Coordenada(origem.X, origem.Y + d.Y));
        }

        // ------------------------------------------------------------------
        // Enumeração e contagem
        // ------------------------------------------------------------------

        /// <summary>Todas as coordenadas do mapa, linha por linha (y = 0 primeiro).</summary>
        public IEnumerable<Coordenada> TodasAsCoordenadas()
        {
            for (int y = 0; y < Altura; y++)
                for (int x = 0; x < Largura; x++)
                    yield return new Coordenada(x, y);
        }

        public IEnumerable<Coordenada> CoordenadasDoTipo(TipoCelula tipo)
        {
            for (int i = 0; i < _tipos.Length; i++)
                if (_tipos[i] == tipo)
                    yield return CoordenadaDoIndice(i);
        }

        public IEnumerable<Coordenada> SpawnsDoLado(Lado lado)
        {
            for (int i = 0; i < _tipos.Length; i++)
                if (_tipos[i] == TipoCelula.Spawn && _ladosSpawn[i] == lado)
                    yield return CoordenadaDoIndice(i);
        }

        public int Contar(TipoCelula tipo)
        {
            int total = 0;
            foreach (var t in _tipos)
                if (t == tipo) total++;
            return total;
        }

        /// <summary>
        /// Cópia independente do mapa. Será usada, por exemplo, para aplicar as duas
        /// estratégias de cobertura (aleatória e guiada) sobre a mesma estrutura base.
        /// </summary>
        public MapaTatico Clonar() => new MapaTatico(this);

        // ------------------------------------------------------------------
        // Índices
        // ------------------------------------------------------------------

        private int Indice(Coordenada c)
        {
            if (!DentroDosLimites(c))
                throw new ArgumentOutOfRangeException(nameof(c), c, $"Coordenada fora do mapa {Largura}x{Altura}.");
            return c.Y * Largura + c.X;
        }

        private Coordenada CoordenadaDoIndice(int i) => new Coordenada(i % Largura, i / Largura);
    }
}
