using System;
using System.Collections.Generic;
using System.Text;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Converte mapas para texto ASCII e vice-versa. Serve para desenhar mapas de teste
    /// "à mão" (nos testes automatizados e no Inspector) e para imprimir mapas gerados
    /// em logs, de forma legível.
    ///
    /// Legenda:
    /// <code>
    ///   .  piso            h  meia cobertura    A  spawn do lado A
    ///   #  parede          H  cobertura total   B  spawn do lado B
    /// </code>
    ///
    /// Orientação: a PRIMEIRA linha do texto é a linha de MAIOR Y, e a primeira coluna é
    /// X = 0 — o texto se lê como um gráfico cartesiano (Y cresce para cima).
    /// </summary>
    public static class ConversorMapaTexto
    {
        public const char CaracterePiso = '.';
        public const char CaracterParede = '#';
        public const char CaractereMeiaCobertura = 'h';
        public const char CaractereCoberturaTotal = 'H';
        public const char CaractereSpawnA = 'A';
        public const char CaractereSpawnB = 'B';

        /// <summary>
        /// Lê um mapa a partir de texto. Linhas vazias no início e no fim e espaços nas
        /// pontas de cada linha são ignorados; as linhas restantes devem ter o mesmo tamanho.
        /// </summary>
        /// <exception cref="FormatException">Texto vazio, linhas de tamanhos diferentes ou caractere desconhecido.</exception>
        public static MapaTatico Ler(string texto)
        {
            if (texto == null) throw new ArgumentNullException(nameof(texto));

            List<string> linhas = ExtrairLinhas(texto);
            if (linhas.Count == 0)
                throw new FormatException("O texto do mapa está vazio.");

            int largura = linhas[0].Length;
            int altura = linhas.Count;

            for (int i = 0; i < linhas.Count; i++)
            {
                if (linhas[i].Length != largura)
                    throw new FormatException(
                        $"Linha {i + 1} do texto tem {linhas[i].Length} caracteres, mas a primeira tem {largura}. O mapa deve ser retangular.");
            }

            var mapa = new MapaTatico(largura, altura);
            for (int i = 0; i < altura; i++)
            {
                int y = altura - 1 - i; // primeira linha do texto = maior Y
                for (int x = 0; x < largura; x++)
                    AplicarCaractere(mapa, new Coordenada(x, y), linhas[i][x], i + 1, x + 1);
            }

            return mapa;
        }

        /// <summary>Converte o mapa em texto, uma linha por valor de Y (do maior para o menor).</summary>
        public static string Escrever(MapaTatico mapa)
        {
            if (mapa == null) throw new ArgumentNullException(nameof(mapa));

            var sb = new StringBuilder(mapa.TotalCelulas + mapa.Altura);
            for (int y = mapa.Altura - 1; y >= 0; y--)
            {
                for (int x = 0; x < mapa.Largura; x++)
                    sb.Append(CaractereDe(mapa, new Coordenada(x, y)));
                if (y > 0) sb.Append('\n');
            }
            return sb.ToString();
        }

        private static List<string> ExtrairLinhas(string texto)
        {
            var linhas = new List<string>();
            foreach (var bruta in texto.Replace("\r", "").Split('\n'))
                linhas.Add(bruta.Trim()); // espaço não é caractere de mapa: permite indentar o texto

            while (linhas.Count > 0 && linhas[0].Length == 0) linhas.RemoveAt(0);
            while (linhas.Count > 0 && linhas[linhas.Count - 1].Length == 0) linhas.RemoveAt(linhas.Count - 1);
            return linhas;
        }

        private static void AplicarCaractere(MapaTatico mapa, Coordenada c, char caractere, int linha, int coluna)
        {
            switch (caractere)
            {
                case CaracterePiso: mapa.DefinirTipo(c, TipoCelula.Piso); break;
                case CaracterParede: mapa.DefinirTipo(c, TipoCelula.Parede); break;
                case CaractereMeiaCobertura: mapa.DefinirTipo(c, TipoCelula.MeiaCobertura); break;
                case CaractereCoberturaTotal: mapa.DefinirTipo(c, TipoCelula.CoberturaTotal); break;
                case CaractereSpawnA: mapa.DefinirSpawn(c, Lado.A); break;
                case CaractereSpawnB: mapa.DefinirSpawn(c, Lado.B); break;
                default:
                    throw new FormatException(
                        $"Caractere '{caractere}' desconhecido na linha {linha}, coluna {coluna}. Use . # h H A B.");
            }
        }

        private static char CaractereDe(MapaTatico mapa, Coordenada c)
        {
            switch (mapa.ObterTipo(c))
            {
                case TipoCelula.Parede: return CaracterParede;
                case TipoCelula.MeiaCobertura: return CaractereMeiaCobertura;
                case TipoCelula.CoberturaTotal: return CaractereCoberturaTotal;
                case TipoCelula.Spawn: return mapa.ObterLadoSpawn(c) == Lado.A ? CaractereSpawnA : CaractereSpawnB;
                default: return CaracterePiso;
            }
        }
    }
}
