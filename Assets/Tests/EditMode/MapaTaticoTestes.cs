using System;
using System.Collections.Generic;
using System.Linq;
using AlgoritmoPCG.Nucleo;
using NUnit.Framework;

namespace AlgoritmoPCG.Testes
{
    public class MapaTaticoTestes
    {
        // ------------------------------------------------------------------
        // Criação e leitura/escrita
        // ------------------------------------------------------------------

        [Test]
        public void MapaNovo_TemDimensoesInformadas_ETodasAsCelulasSaoPiso()
        {
            var mapa = new MapaTatico(5, 3);

            Assert.AreEqual(5, mapa.Largura);
            Assert.AreEqual(3, mapa.Altura);
            Assert.AreEqual(15, mapa.TotalCelulas);
            Assert.AreEqual(15, mapa.Contar(TipoCelula.Piso));
            Assert.IsTrue(mapa.TodasAsCoordenadas().All(c => mapa.ObterLadoSpawn(c) == Lado.Nenhum));
        }

        [TestCase(0, 5)]
        [TestCase(5, 0)]
        [TestCase(-1, 3)]
        public void DimensoesInvalidas_LancamExcecao(int largura, int altura)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new MapaTatico(largura, altura));
        }

        [TestCase(0, 0, true)]
        [TestCase(4, 2, true)]
        [TestCase(5, 2, false)]
        [TestCase(4, 3, false)]
        [TestCase(-1, 0, false)]
        public void DentroDosLimites_RespeitaAsBordas(int x, int y, bool esperado)
        {
            Assert.AreEqual(esperado, new MapaTatico(5, 3).DentroDosLimites(new Coordenada(x, y)));
        }

        [Test]
        public void AcessoForaDoMapa_LancaExcecao()
        {
            var mapa = new MapaTatico(5, 3);
            Assert.Throws<ArgumentOutOfRangeException>(() => mapa.ObterTipo(new Coordenada(5, 0)));
            Assert.Throws<ArgumentOutOfRangeException>(() => mapa.DefinirTipo(new Coordenada(0, -1), TipoCelula.Parede));
        }

        [Test]
        public void DefinirTipo_AlteraSomenteACelulaIndicada()
        {
            var mapa = new MapaTatico(4, 4);
            mapa.DefinirTipo(new Coordenada(2, 1), TipoCelula.CoberturaTotal);

            Assert.AreEqual(TipoCelula.CoberturaTotal, mapa.ObterTipo(new Coordenada(2, 1)));
            Assert.AreEqual(TipoCelula.Piso, mapa.ObterTipo(new Coordenada(1, 2)), "X e Y não podem estar trocados.");
            Assert.AreEqual(15, mapa.Contar(TipoCelula.Piso));
        }

        [Test]
        public void DefinirTipoComoSpawn_LancaExcecao_PoisSpawnPrecisaDeLado()
        {
            var mapa = new MapaTatico(3, 3);
            Assert.Throws<ArgumentException>(() => mapa.DefinirTipo(Coordenada.Zero, TipoCelula.Spawn));
            Assert.Throws<ArgumentException>(() => mapa.DefinirSpawn(Coordenada.Zero, Lado.Nenhum));
        }

        [Test]
        public void DefinirSpawn_GuardaOLado_ESobrescreverApagaOLado()
        {
            var mapa = new MapaTatico(3, 3);
            var c = new Coordenada(1, 1);

            mapa.DefinirSpawn(c, Lado.B);
            Assert.AreEqual(TipoCelula.Spawn, mapa.ObterTipo(c));
            Assert.AreEqual(Lado.B, mapa.ObterLadoSpawn(c));

            mapa.DefinirTipo(c, TipoCelula.Piso);
            Assert.AreEqual(Lado.Nenhum, mapa.ObterLadoSpawn(c));
        }

        // ------------------------------------------------------------------
        // Regras de terreno
        // ------------------------------------------------------------------

        [TestCase(TipoCelula.Piso, true, false)]
        [TestCase(TipoCelula.Spawn, true, false)]
        [TestCase(TipoCelula.Parede, false, true)] // parede também é cobertura
        [TestCase(TipoCelula.MeiaCobertura, false, true)]
        [TestCase(TipoCelula.CoberturaTotal, false, true)]
        public void RegrasDoTipo_TransitavelECobertura(TipoCelula tipo, bool transitavel, bool cobertura)
        {
            Assert.AreEqual(transitavel, tipo.EhTransitavel());
            Assert.AreEqual(!transitavel, tipo.BloqueiaMovimento());
            Assert.AreEqual(cobertura, tipo.EhCobertura());
        }

        [Test]
        public void EhTransitavel_ForaDoMapa_EhFalso()
        {
            Assert.IsFalse(new MapaTatico(3, 3).EhTransitavel(new Coordenada(-1, 0)));
        }

        // ------------------------------------------------------------------
        // Vizinhança
        // ------------------------------------------------------------------

        [TestCase(2, 2, 8)] // centro
        [TestCase(0, 0, 3)] // canto
        [TestCase(2, 0, 5)] // borda
        public void Vizinhos8_ConsideraOsLimitesDoMapa(int x, int y, int esperado)
        {
            Assert.AreEqual(esperado, new MapaTatico(5, 5).Vizinhos8(new Coordenada(x, y)).Count());
        }

        [Test]
        public void Vizinhos8_IncluiParedesECoberturas()
        {
            var mapa = ConversorMapaTexto.Ler(@"
                ###
                #.H
                hh#");

            Assert.AreEqual(8, mapa.Vizinhos8(new Coordenada(1, 1)).Count());
        }

        [Test]
        public void VizinhosTransitaveis_EmCampoAberto_SaoOs8()
        {
            Assert.AreEqual(8, new MapaTatico(3, 3).VizinhosTransitaveis(new Coordenada(1, 1)).Count());
        }

        [Test]
        public void VizinhosTransitaveis_ExcluemParedesECoberturas()
        {
            var mapa = ConversorMapaTexto.Ler(@"
                .#.
                h.H
                ...");
            var vizinhos = new HashSet<Coordenada>(mapa.VizinhosTransitaveis(new Coordenada(1, 1)));

            Assert.IsFalse(vizinhos.Contains(new Coordenada(1, 2)), "Parede bloqueia.");
            Assert.IsFalse(vizinhos.Contains(new Coordenada(0, 1)), "Meia cobertura bloqueia.");
            Assert.IsFalse(vizinhos.Contains(new Coordenada(2, 1)), "Cobertura total bloqueia.");
            // Todas as diagonais contornam alguma das células bloqueadas: nenhuma é permitida.
            // Ex.: ir para (0,0) passaria rente à meia cobertura em (0,1).
            Assert.IsFalse(vizinhos.Contains(new Coordenada(0, 2)));
            Assert.IsFalse(vizinhos.Contains(new Coordenada(2, 2)));
            Assert.IsFalse(vizinhos.Contains(new Coordenada(0, 0)));
            Assert.IsFalse(vizinhos.Contains(new Coordenada(2, 0)));
            CollectionAssert.AreEquivalent(new[] { new Coordenada(1, 0) }, vizinhos);
        }

        [Test]
        public void Diagonal_NaoCortaQuinaDeParede()
        {
            // Origem (0,0); destino diagonal (1,1); a parede está em (1,0).
            var mapa = ConversorMapaTexto.Ler(@"
                ..
                .#");

            Assert.IsFalse(mapa.PodeMoverEntreAdjacentes(new Coordenada(0, 0), new Coordenada(1, 1)));
            Assert.IsTrue(mapa.PodeMoverEntreAdjacentes(new Coordenada(0, 0), new Coordenada(0, 1)));
        }

        [Test]
        public void Diagonal_NaoPassaEspremidaEntreDoisObstaculos()
        {
            var mapa = ConversorMapaTexto.Ler(@"
                #.
                .h");

            Assert.IsFalse(mapa.PodeMoverEntreAdjacentes(new Coordenada(0, 0), new Coordenada(1, 1)));
        }

        [Test]
        public void Diagonal_LivreEhPermitida_NosDoisSentidos()
        {
            var mapa = new MapaTatico(2, 2);
            Assert.IsTrue(mapa.PodeMoverEntreAdjacentes(new Coordenada(0, 0), new Coordenada(1, 1)));
            Assert.IsTrue(mapa.PodeMoverEntreAdjacentes(new Coordenada(1, 1), new Coordenada(0, 0)));
        }

        [Test]
        public void PodeMoverEntreAdjacentes_RejeitaCelulasNaoAdjacentesOuIguais()
        {
            var mapa = new MapaTatico(4, 4);
            Assert.IsFalse(mapa.PodeMoverEntreAdjacentes(new Coordenada(0, 0), new Coordenada(2, 0)));
            Assert.IsFalse(mapa.PodeMoverEntreAdjacentes(new Coordenada(1, 1), new Coordenada(1, 1)));
        }

        // ------------------------------------------------------------------
        // Enumeração, contagem e cópia
        // ------------------------------------------------------------------

        [Test]
        public void ContagemEEnumeracao_PorTipoEPorLado()
        {
            var mapa = ConversorMapaTexto.Ler(@"
                A.hB
                A#HB");

            Assert.AreEqual(4, mapa.Contar(TipoCelula.Spawn));
            Assert.AreEqual(1, mapa.Contar(TipoCelula.Parede));
            CollectionAssert.AreEquivalent(new[] { new Coordenada(0, 0), new Coordenada(0, 1) }, mapa.SpawnsDoLado(Lado.A));
            CollectionAssert.AreEquivalent(new[] { new Coordenada(3, 0), new Coordenada(3, 1) }, mapa.SpawnsDoLado(Lado.B));
            CollectionAssert.AreEqual(new[] { new Coordenada(2, 1) }, mapa.CoordenadasDoTipo(TipoCelula.MeiaCobertura));
            Assert.AreEqual(mapa.TotalCelulas, mapa.TodasAsCoordenadas().Distinct().Count());
        }

        [Test]
        public void Clonar_GeraCopiaIndependente()
        {
            var original = ConversorMapaTexto.Ler("A.#");
            var copia = original.Clonar();

            copia.DefinirTipo(new Coordenada(1, 0), TipoCelula.CoberturaTotal);
            copia.DefinirTipo(new Coordenada(0, 0), TipoCelula.Piso);

            Assert.AreEqual("A.#", ConversorMapaTexto.Escrever(original));
            Assert.AreEqual(".H#", ConversorMapaTexto.Escrever(copia));
        }

        [Test]
        public void LadoOponente_TrocaAEB()
        {
            Assert.AreEqual(Lado.B, Lado.A.Oponente());
            Assert.AreEqual(Lado.A, Lado.B.Oponente());
            Assert.AreEqual(Lado.Nenhum, Lado.Nenhum.Oponente());
        }

        [Test]
        public void Direcoes_Oito_SaoUnicasEUnitarias()
        {
            Assert.AreEqual(8, Direcoes.Oito.Distinct().Count());
            Assert.IsTrue(Direcoes.Oito.All(d => d.DistanciaChebyshev(Coordenada.Zero) == 1));
            Assert.AreEqual(4, Direcoes.Oito.Count(Direcoes.EhDiagonal));
            CollectionAssert.AreEquivalent(Direcoes.Oito, Direcoes.Ortogonais.Concat(Direcoes.Diagonais));
        }
    }
}
