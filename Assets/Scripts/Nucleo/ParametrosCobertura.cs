using System;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Quanto cada tipo de cobertura reduz a chance de acerto de um tiro contra a unidade
    /// que ela protege. Os valores são frações em [0, 1] (0 = não protege; 1 = protege
    /// totalmente).
    ///
    /// A parede tem parâmetro PRÓPRIO, separado da cobertura total, embora por padrão
    /// proteja o mesmo tanto. Isso permite testar no experimento se tratar paredes como
    /// cobertura muda o equilíbrio dos mapas, sem misturar os dois efeitos.
    ///
    /// A fórmula que aplica a redução sobre a chance base (que decai com a distância)
    /// será definida na Etapa 4; a intenção é multiplicativa:
    /// chanceFinal = chanceBase × (1 − redução).
    /// </summary>
    public sealed class ParametrosCobertura
    {
        public const double ReducaoMeiaCoberturaPadrao = 0.25;
        public const double ReducaoCoberturaTotalPadrao = 0.50;
        public const double ReducaoParedePadrao = ReducaoCoberturaTotalPadrao; // parede = cobertura total por padrão

        public double ReducaoMeiaCobertura { get; }
        public double ReducaoCoberturaTotal { get; }
        public double ReducaoParede { get; }

        public ParametrosCobertura(
            double reducaoMeiaCobertura = ReducaoMeiaCoberturaPadrao,
            double reducaoCoberturaTotal = ReducaoCoberturaTotalPadrao,
            double reducaoParede = ReducaoParedePadrao)
        {
            ReducaoMeiaCobertura = ValidarFracao(reducaoMeiaCobertura, nameof(reducaoMeiaCobertura));
            ReducaoCoberturaTotal = ValidarFracao(reducaoCoberturaTotal, nameof(reducaoCoberturaTotal));
            ReducaoParede = ValidarFracao(reducaoParede, nameof(reducaoParede));
        }

        /// <summary>Parâmetros com os valores padrão.</summary>
        public static ParametrosCobertura Padrao { get; } = new ParametrosCobertura();

        /// <summary>Redução oferecida por uma célula do tipo informado (0 se não for cobertura).</summary>
        public double ReducaoPara(TipoCelula tipo)
        {
            switch (tipo)
            {
                case TipoCelula.MeiaCobertura: return ReducaoMeiaCobertura;
                case TipoCelula.CoberturaTotal: return ReducaoCoberturaTotal;
                case TipoCelula.Parede: return ReducaoParede;
                default: return 0.0;
            }
        }

        private static double ValidarFracao(double valor, string nome)
        {
            if (double.IsNaN(valor) || valor < 0.0 || valor > 1.0)
                throw new ArgumentOutOfRangeException(nome, valor, "A redução deve estar entre 0 e 1.");
            return valor;
        }
    }
}
