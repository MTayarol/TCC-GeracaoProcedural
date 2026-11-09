using System.Collections.Generic;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Deslocamentos unitários usados nas consultas de vizinhança do grid.
    ///
    /// Os nomes cardeais são lógicos (Norte = +Y, Leste = +X); na tela isométrica eles
    /// aparecem girados 45°, mas o núcleo não precisa saber disso.
    /// </summary>
    public static class Direcoes
    {
        public static readonly Coordenada Norte = new Coordenada(0, 1);
        public static readonly Coordenada Nordeste = new Coordenada(1, 1);
        public static readonly Coordenada Leste = new Coordenada(1, 0);
        public static readonly Coordenada Sudeste = new Coordenada(1, -1);
        public static readonly Coordenada Sul = new Coordenada(0, -1);
        public static readonly Coordenada Sudoeste = new Coordenada(-1, -1);
        public static readonly Coordenada Oeste = new Coordenada(-1, 0);
        public static readonly Coordenada Noroeste = new Coordenada(-1, 1);

        /// <summary>As 4 direções ortogonais, em sentido horário a partir do Norte.</summary>
        public static readonly IReadOnlyList<Coordenada> Ortogonais = new[] { Norte, Leste, Sul, Oeste };

        /// <summary>As 4 direções diagonais, em sentido horário a partir do Nordeste.</summary>
        public static readonly IReadOnlyList<Coordenada> Diagonais = new[] { Nordeste, Sudeste, Sudoeste, Noroeste };

        /// <summary>As 8 direções (vizinhança de Moore), em sentido horário a partir do Norte.</summary>
        public static readonly IReadOnlyList<Coordenada> Oito = new[]
        {
            Norte, Nordeste, Leste, Sudeste, Sul, Sudoeste, Oeste, Noroeste
        };

        /// <summary>Verdadeiro se o deslocamento anda nos dois eixos ao mesmo tempo.</summary>
        public static bool EhDiagonal(Coordenada deslocamento)
        {
            return deslocamento.X != 0 && deslocamento.Y != 0;
        }
    }
}
