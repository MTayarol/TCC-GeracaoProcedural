namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Tipo de terreno de uma célula do mapa tático.
    /// </summary>
    public enum TipoCelula : byte
    {
        /// <summary>Chão livre: pode ser ocupado e atravessado.</summary>
        Piso = 0,

        /// <summary>Obstáculo estrutural: bloqueia o movimento e não oferece cobertura.</summary>
        Parede = 1,

        /// <summary>Obstáculo baixo: bloqueia o movimento e reduz parcialmente a chance de acerto.</summary>
        MeiaCobertura = 2,

        /// <summary>Obstáculo alto: bloqueia o movimento e reduz mais a chance de acerto.</summary>
        CoberturaTotal = 3,

        /// <summary>
        /// Ponto de nascimento de uma unidade. Funciona como piso para o movimento.
        /// O lado (A ou B) dono do spawn é guardado separadamente no <see cref="MapaTatico"/>.
        /// </summary>
        Spawn = 4,
    }

    /// <summary>
    /// Regras derivadas do tipo de célula. Ficam centralizadas aqui para que movimento,
    /// geração e métricas usem exatamente a mesma definição de "bloqueio" e "cobertura".
    /// </summary>
    public static class TipoCelulaExtensoes
    {
        /// <summary>Parede e os dois tipos de cobertura impedem que uma unidade entre na célula.</summary>
        public static bool BloqueiaMovimento(this TipoCelula tipo)
        {
            return tipo == TipoCelula.Parede
                || tipo == TipoCelula.MeiaCobertura
                || tipo == TipoCelula.CoberturaTotal;
        }

        /// <summary>Células que uma unidade pode ocupar (piso e spawn).</summary>
        public static bool EhTransitavel(this TipoCelula tipo)
        {
            return !tipo.BloqueiaMovimento();
        }

        /// <summary>Meia cobertura ou cobertura total. Parede NÃO conta como cobertura.</summary>
        public static bool EhCobertura(this TipoCelula tipo)
        {
            return tipo == TipoCelula.MeiaCobertura || tipo == TipoCelula.CoberturaTotal;
        }
    }
}
