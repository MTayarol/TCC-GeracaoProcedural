namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Lado (jogador/equipe) em uma partida. Usado para os spawns do mapa e, nas
    /// próximas etapas, para as unidades.
    /// </summary>
    public enum Lado : byte
    {
        /// <summary>Nenhum lado (ex.: célula que não é spawn).</summary>
        Nenhum = 0,
        A = 1,
        B = 2,
    }

    public static class LadoExtensoes
    {
        /// <summary>Lado adversário. O adversário de <see cref="Lado.Nenhum"/> é ele mesmo.</summary>
        public static Lado Oponente(this Lado lado)
        {
            switch (lado)
            {
                case Lado.A: return Lado.B;
                case Lado.B: return Lado.A;
                default: return Lado.Nenhum;
            }
        }
    }
}
