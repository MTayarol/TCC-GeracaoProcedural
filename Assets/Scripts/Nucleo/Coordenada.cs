using System;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Posição lógica (X, Y) de uma célula no grid do mapa tático.
    ///
    /// É a "moeda comum" entre o núcleo (dados, geração, métricas) e a camada visual:
    /// o núcleo raciocina apenas em coordenadas inteiras, sem saber que o mapa será
    /// desenhado em perspectiva isométrica. A conversão para posição no mundo da Unity
    /// acontece exclusivamente na camada de visualização.
    ///
    /// Convenção: X cresce no sentido das colunas e Y no sentido das linhas, com a
    /// origem (0, 0) em um canto do mapa. Na Tilemap isométrica da Unity, o eixo X
    /// aparece na diagonal "direita-cima" da tela e o eixo Y na diagonal "esquerda-cima".
    /// </summary>
    public readonly struct Coordenada : IEquatable<Coordenada>
    {
        /// <summary>Coluna da célula.</summary>
        public readonly int X;

        /// <summary>Linha da célula.</summary>
        public readonly int Y;

        public static readonly Coordenada Zero = new Coordenada(0, 0);

        public Coordenada(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Distância de Manhattan (|dx| + |dy|): número mínimo de passos entre duas células
        /// quando só se anda nas 4 direções ortogonais (sem diagonais).
        /// </summary>
        public int DistanciaManhattan(Coordenada outra)
        {
            return Math.Abs(X - outra.X) + Math.Abs(Y - outra.Y);
        }

        /// <summary>
        /// Distância de Chebyshev (max(|dx|, |dy|)): número mínimo de passos entre duas
        /// células quando também se pode andar nas diagonais (vizinhança de 8).
        /// </summary>
        public int DistanciaChebyshev(Coordenada outra)
        {
            return Math.Max(Math.Abs(X - outra.X), Math.Abs(Y - outra.Y));
        }

        public static Coordenada operator +(Coordenada a, Coordenada b) => new Coordenada(a.X + b.X, a.Y + b.Y);
        public static Coordenada operator -(Coordenada a, Coordenada b) => new Coordenada(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(Coordenada a, Coordenada b) => a.Equals(b);
        public static bool operator !=(Coordenada a, Coordenada b) => !a.Equals(b);

        public bool Equals(Coordenada outra) => X == outra.X && Y == outra.Y;
        public override bool Equals(object obj) => obj is Coordenada outra && Equals(outra);
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }
}
