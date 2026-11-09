using System;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Regra geométrica da cobertura: QUAL célula pode proteger um alvo contra um
    /// atirador. A visão não é bloqueada por nada (só o alcance importa); a cobertura
    /// apenas reduz a chance de acerto.
    ///
    /// Regra: a direção do alvo para o atirador é arredondada para uma das 8 direções
    /// (setores de 45°). O alvo está protegido se a célula vizinha a ele NESSA direção for
    /// cobertura (meia, total ou parede). Caso contrário, o atirador está em flanco e não
    /// há redução.
    ///
    /// Exemplos (alvo em X, atirador em S; o vizinho de X na direção de S está marcado com *):
    /// <code>
    ///   . . . . .          . . . . .
    ///   X * . . S          . . . . S
    ///   . . . . .          X * . . .     ← S 4 à direita e 1 acima (≈14°): setor Leste
    /// </code>
    /// </summary>
    public static class RegrasCobertura
    {
        // Ordem dos setores no sentido anti-horário a partir do Leste (ângulo 0).
        private static readonly Coordenada[] DirecoesPorSetor =
        {
            Direcoes.Leste, Direcoes.Nordeste, Direcoes.Norte, Direcoes.Noroeste,
            Direcoes.Oeste, Direcoes.Sudoeste, Direcoes.Sul, Direcoes.Sudeste,
        };

        /// <summary>
        /// Direção (uma das 8) que aponta do alvo para o atirador, arredondando o ângulo
        /// para o setor de 45° mais próximo. Com coordenadas inteiras nunca há empate
        /// entre setores (as fronteiras em 22,5°, 67,5°... têm tangente irracional).
        /// </summary>
        /// <exception cref="ArgumentException">Alvo e atirador na mesma célula.</exception>
        public static Coordenada DirecaoDoAtirador(Coordenada alvo, Coordenada atirador)
        {
            var delta = atirador - alvo;
            if (delta == Coordenada.Zero)
                throw new ArgumentException("Alvo e atirador não podem estar na mesma célula.");

            double angulo = Math.Atan2(delta.Y, delta.X);              // (-π, π]
            int setor = (int)Math.Round(angulo / (Math.PI / 4.0));     // -4..4
            return DirecoesPorSetor[((setor % 8) + 8) % 8];
        }

        /// <summary>
        /// Célula que protege o alvo contra o atirador, se houver: o vizinho do alvo na
        /// direção do atirador, desde que exista no mapa e seja cobertura.
        /// </summary>
        public static bool TentarObterCoberturaProtetora(
            MapaTatico mapa, Coordenada alvo, Coordenada atirador, out Coordenada celulaCobertura)
        {
            if (mapa == null) throw new ArgumentNullException(nameof(mapa));

            celulaCobertura = alvo + DirecaoDoAtirador(alvo, atirador);
            return mapa.EhCobertura(celulaCobertura); // falso também se estiver fora do mapa
        }

        /// <summary>
        /// Fração de redução da chance de acerto que o alvo recebe contra esse atirador
        /// (0 quando o atirador está em flanco).
        /// </summary>
        public static double ReducaoContra(
            MapaTatico mapa, Coordenada alvo, Coordenada atirador, ParametrosCobertura parametros)
        {
            if (parametros == null) throw new ArgumentNullException(nameof(parametros));

            return TentarObterCoberturaProtetora(mapa, alvo, atirador, out var celula)
                ? parametros.ReducaoPara(mapa.ObterTipo(celula))
                : 0.0;
        }
    }
}
