using System;
using System.Linq;
using AlgoritmoPCG.Nucleo;
using NUnit.Framework;

namespace AlgoritmoPCG.Testes
{
    public class MapasDeExemploTestes
    {
        private static readonly object[] Tamanhos =
        {
            new object[] { 8, 6 },
            new object[] { 9, 7 },
            new object[] { 16, 12 },
            new object[] { 31, 20 },
        };

        [TestCaseSource(nameof(Tamanhos))]
        public void Demonstracao_TemODimensionamentoPedido_ETodosOsTipos(int largura, int altura)
        {
            var mapa = MapasDeExemplo.CriarDemonstracao(largura, altura);

            Assert.AreEqual(largura, mapa.Largura);
            Assert.AreEqual(altura, mapa.Altura);
            foreach (TipoCelula tipo in Enum.GetValues(typeof(TipoCelula)))
                Assert.Greater(mapa.Contar(tipo), 0, $"O mapa {largura}x{altura} deveria conter {tipo}.");
        }

        [TestCaseSource(nameof(Tamanhos))]
        public void Demonstracao_TemSimetriaDeRotacao180_ComLadosTrocados(int largura, int altura)
        {
            var mapa = MapasDeExemplo.CriarDemonstracao(largura, altura);

            foreach (var c in mapa.TodasAsCoordenadas())
            {
                var espelho = MapasDeExemplo.Espelhar(mapa, c);
                Assert.AreEqual(mapa.ObterTipo(c), mapa.ObterTipo(espelho), $"Tipos diferentes em {c} e {espelho}.");
                Assert.AreEqual(mapa.ObterLadoSpawn(c).Oponente(), mapa.ObterLadoSpawn(espelho), $"Lados não trocados em {c}.");
            }
        }

        [TestCaseSource(nameof(Tamanhos))]
        public void Demonstracao_TemBordaDeParedes_EMesmoNumeroDeSpawnsPorLado(int largura, int altura)
        {
            var mapa = MapasDeExemplo.CriarDemonstracao(largura, altura);

            var borda = mapa.TodasAsCoordenadas()
                .Where(c => c.X == 0 || c.Y == 0 || c.X == largura - 1 || c.Y == altura - 1);
            Assert.IsTrue(borda.All(c => mapa.ObterTipo(c) == TipoCelula.Parede));

            Assert.AreEqual(3, mapa.SpawnsDoLado(Lado.A).Count());
            Assert.AreEqual(3, mapa.SpawnsDoLado(Lado.B).Count());
        }

        [Test]
        public void MapaDeTesteEmTexto_EhValido_TemTodosOsTipos_ESimetria180()
        {
            var mapa = ConversorMapaTexto.Ler(MapasDeExemplo.TextoMapaDeTeste);

            Assert.AreEqual(14, mapa.Largura);
            Assert.AreEqual(9, mapa.Altura);
            foreach (TipoCelula tipo in Enum.GetValues(typeof(TipoCelula)))
                Assert.Greater(mapa.Contar(tipo), 0, $"O mapa de teste deveria conter {tipo}.");
            Assert.AreEqual(mapa.SpawnsDoLado(Lado.A).Count(), mapa.SpawnsDoLado(Lado.B).Count());

            foreach (var c in mapa.TodasAsCoordenadas())
            {
                var espelho = MapasDeExemplo.Espelhar(mapa, c);
                Assert.AreEqual(mapa.ObterTipo(c), mapa.ObterTipo(espelho), $"Tipos diferentes em {c} e {espelho}.");
                Assert.AreEqual(mapa.ObterLadoSpawn(c).Oponente(), mapa.ObterLadoSpawn(espelho));
            }
        }

        [TestCase(7, 6)]
        [TestCase(8, 5)]
        public void Demonstracao_MenorQueOMinimo_LancaExcecao(int largura, int altura)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => MapasDeExemplo.CriarDemonstracao(largura, altura));
        }
    }
}
