using System;
using AlgoritmoPCG.Nucleo;
using UnityEngine;

namespace AlgoritmoPCG.Visualizacao
{
    /// <summary>
    /// Cores placeholder de cada tipo de célula, editáveis no Inspector.
    /// Spawns têm uma cor por lado para que fique claro de quem é cada ponto de nascimento.
    /// </summary>
    [Serializable]
    public class PaletaCores
    {
        public Color piso = new Color(0.62f, 0.68f, 0.58f);
        public Color parede = new Color(0.22f, 0.23f, 0.27f);
        public Color meiaCobertura = new Color(0.95f, 0.80f, 0.30f);
        public Color coberturaTotal = new Color(0.85f, 0.45f, 0.18f);
        public Color spawnLadoA = new Color(0.30f, 0.55f, 0.95f);
        public Color spawnLadoB = new Color(0.90f, 0.32f, 0.35f);

        /// <summary>Quantidade de categorias visuais distintas (4 tipos + spawn A + spawn B).</summary>
        public const int TotalCategorias = 6;

        /// <summary>
        /// Índice da categoria visual de uma célula. Spawn é dividido em duas categorias
        /// (uma por lado); os demais tipos usam o próprio valor do enum.
        /// </summary>
        public static int Categoria(TipoCelula tipo, Lado ladoSpawn)
        {
            if (tipo == TipoCelula.Spawn)
                return ladoSpawn == Lado.B ? 5 : 4;
            return (int)tipo; // Piso=0, Parede=1, MeiaCobertura=2, CoberturaTotal=3
        }

        public Color CorDaCategoria(int categoria)
        {
            switch (categoria)
            {
                case 0: return piso;
                case 1: return parede;
                case 2: return meiaCobertura;
                case 3: return coberturaTotal;
                case 4: return spawnLadoA;
                case 5: return spawnLadoB;
                default: throw new ArgumentOutOfRangeException(nameof(categoria));
            }
        }

        public Color CorDe(TipoCelula tipo, Lado ladoSpawn) => CorDaCategoria(Categoria(tipo, ladoSpawn));
    }
}
