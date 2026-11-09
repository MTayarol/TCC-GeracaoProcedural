using System;
using AlgoritmoPCG.Nucleo;
using NUnit.Framework;

namespace AlgoritmoPCG.Testes
{
    /// <summary>
    /// Testes da regra geométrica de cobertura, com foco na mudança de regra que fez as
    /// paredes também funcionarem como cobertura.
    /// </summary>
    public class RegrasCoberturaTestes
    {
        private const double Tolerancia = 1e-9;

        // ------------------------------------------------------------------
        // Direção do atirador (8 setores de 45°)
        // ------------------------------------------------------------------

        [TestCase(5, 0, 1, 0)]    // Leste puro
        [TestCase(4, 1, 1, 0)]    // ≈14°: ainda Leste
        [TestCase(3, 2, 1, 1)]    // ≈34°: Nordeste
        [TestCase(1, 1, 1, 1)]    // 45°: Nordeste
        [TestCase(0, 3, 0, 1)]    // Norte
        [TestCase(-5, 1, -1, 0)]  // Oeste
        [TestCase(-2, -2, -1, -1)] // Sudoeste
        [TestCase(1, -4, 0, -1)]  // ≈ -76°: Sul
        [TestCase(2, -1, 1, -1)]  // ≈ -27°: Sudeste
        public void DirecaoDoAtirador_ArredondaParaOSetorMaisProximo(int dx, int dy, int esperadoX, int esperadoY)
        {
            var alvo = new Coordenada(10, 10);
            var atirador = alvo + new Coordenada(dx, dy);

            Assert.AreEqual(new Coordenada(esperadoX, esperadoY), RegrasCobertura.DirecaoDoAtirador(alvo, atirador));
        }

        [Test]
        public void DirecaoDoAtirador_NaMesmaCelula_LancaExcecao()
        {
            Assert.Throws<ArgumentException>(() => RegrasCobertura.DirecaoDoAtirador(Coordenada.Zero, Coordenada.Zero));
        }

        // ------------------------------------------------------------------
        // Parede como cobertura (mudança de regra)
        // ------------------------------------------------------------------

        [Test]
        public void ParedeAdjacenteAoAlvo_NaDirecaoDoAtirador_Protege()
        {
            // Alvo em (1,1), parede em (2,1), atirador em (5,1).
            var mapa = ConversorMapaTexto.Ler(@"
                .......
                ..#....
                .......");
            var alvo = new Coordenada(1, 1);
            var atirador = new Coordenada(5, 1);

            Assert.IsTrue(RegrasCobertura.TentarObterCoberturaProtetora(mapa, alvo, atirador, out var celula));
            Assert.AreEqual(new Coordenada(2, 1), celula);
            Assert.AreEqual(ParametrosCobertura.ReducaoParedePadrao,
                RegrasCobertura.ReducaoContra(mapa, alvo, atirador, ParametrosCobertura.Padrao), Tolerancia);
        }

        [Test]
        public void ParedeNaDiagonalDoAlvo_ProtegeContraAtiradorNaquelaDiagonal_MasNaoDeFlanco()
        {
            // Alvo em (2,2) e parede a nordeste dele, em (3,3).
            var mapa = ConversorMapaTexto.Ler(@"
                ......
                ...#..
                ......
                ......
                ......");
            var alvo = new Coordenada(2, 2);

            Assert.IsTrue(RegrasCobertura.TentarObterCoberturaProtetora(mapa, alvo, new Coordenada(4, 4), out _),
                "Atirador a nordeste: a parede está entre os dois.");
            Assert.IsFalse(RegrasCobertura.TentarObterCoberturaProtetora(mapa, alvo, new Coordenada(5, 2), out _),
                "Atirador a leste: flanco, a parede a nordeste não protege.");
        }

        [Test]
        public void ParedeDoLadoOpostoAoAtirador_NaoProtege()
        {
            // Parede em (0,1) atrás do alvo (1,1); atirador a leste, em (5,1).
            var mapa = ConversorMapaTexto.Ler(@"
                .......
                #......
                .......");
            var alvo = new Coordenada(1, 1);
            var atirador = new Coordenada(5, 1);

            Assert.IsFalse(RegrasCobertura.TentarObterCoberturaProtetora(mapa, alvo, atirador, out _));
            Assert.AreEqual(0.0, RegrasCobertura.ReducaoContra(mapa, alvo, atirador, ParametrosCobertura.Padrao), Tolerancia);
        }

        [Test]
        public void ParedeDaBordaDoMapa_NaoProtegeContraTirosVindosDeDentro()
        {
            // Alvo no canto interno (1,1), encostado em duas paredes da borda (oeste e sul,
            // além da quina sudoeste). Nenhum atirador dentro do mapa deve ter o tiro reduzido.
            var mapa = ConversorMapaTexto.Ler(@"
                #########
                #.......#
                #.......#
                #.......#
                #.......#
                #########");
            var alvo = new Coordenada(1, 1);

            foreach (var atirador in mapa.TodasAsCoordenadas())
            {
                if (atirador == alvo || !mapa.EhTransitavel(atirador)) continue;

                Assert.AreEqual(0.0, RegrasCobertura.ReducaoContra(mapa, alvo, atirador, ParametrosCobertura.Padrao), Tolerancia,
                    $"Tiro vindo de {atirador} não deveria ser reduzido pela borda.");
            }
        }

        [Test]
        public void ReducaoDaParede_EhConfiguravel_IndependenteDaCoberturaTotal()
        {
            // Alvo em (1,2): parede ao norte (1,3), cobertura total ao sul (1,1).
            var mapa = ConversorMapaTexto.Ler(@"
                .....
                .#...
                .....
                .H...
                .....");
            var alvo = new Coordenada(1, 2);
            var parametros = new ParametrosCobertura(reducaoMeiaCobertura: 0.2, reducaoCoberturaTotal: 0.6, reducaoParede: 0.35);

            Assert.AreEqual(0.35, RegrasCobertura.ReducaoContra(mapa, alvo, new Coordenada(1, 4), parametros), Tolerancia,
                "Tiro vindo do norte: protegido pela parede, com a redução própria da parede.");
            Assert.AreEqual(0.6, RegrasCobertura.ReducaoContra(mapa, alvo, new Coordenada(1, 0), parametros), Tolerancia,
                "Tiro vindo do sul: protegido pela cobertura total.");
        }

        // ------------------------------------------------------------------
        // Demais tipos e parâmetros
        // ------------------------------------------------------------------

        [TestCase(TipoCelula.MeiaCobertura, ParametrosCobertura.ReducaoMeiaCoberturaPadrao)]
        [TestCase(TipoCelula.CoberturaTotal, ParametrosCobertura.ReducaoCoberturaTotalPadrao)]
        [TestCase(TipoCelula.Parede, ParametrosCobertura.ReducaoParedePadrao)]
        public void CadaTipoDeCobertura_AplicaSuaReducaoPadrao(TipoCelula tipo, double esperada)
        {
            var mapa = new MapaTatico(6, 3);
            mapa.DefinirTipo(new Coordenada(2, 1), tipo);

            Assert.AreEqual(esperada,
                RegrasCobertura.ReducaoContra(mapa, new Coordenada(1, 1), new Coordenada(5, 1), ParametrosCobertura.Padrao), Tolerancia);
        }

        [Test]
        public void ValoresPadrao_TotalReduzMaisQueMeia_EParedeIgualATotal()
        {
            var p = ParametrosCobertura.Padrao;
            Assert.Greater(p.ReducaoCoberturaTotal, p.ReducaoMeiaCobertura);
            Assert.AreEqual(p.ReducaoCoberturaTotal, p.ReducaoParede, Tolerancia);
            Assert.AreEqual(0.0, p.ReducaoPara(TipoCelula.Piso), Tolerancia);
            Assert.AreEqual(0.0, p.ReducaoPara(TipoCelula.Spawn), Tolerancia);
        }

        [TestCase(-0.1)]
        [TestCase(1.5)]
        [TestCase(double.NaN)]
        public void ReducaoForaDoIntervalo_LancaExcecao(double valor)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ParametrosCobertura(reducaoParede: valor));
        }

        [Test]
        public void SemCoberturaAdjacente_AtiradorEmFlanco_NaoHaReducao()
        {
            var mapa = new MapaTatico(5, 5);
            Assert.AreEqual(0.0,
                RegrasCobertura.ReducaoContra(mapa, new Coordenada(2, 2), new Coordenada(4, 0), ParametrosCobertura.Padrao), Tolerancia);
        }
    }
}
