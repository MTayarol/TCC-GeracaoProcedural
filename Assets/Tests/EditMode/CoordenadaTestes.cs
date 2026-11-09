using System.Collections.Generic;
using AlgoritmoPCG.Nucleo;
using NUnit.Framework;

namespace AlgoritmoPCG.Testes
{
    /// <summary>
    /// Testes do tipo básico <see cref="Coordenada"/>. Nesta primeira etapa servem
    /// também para validar que a infraestrutura de testes (asmdefs + Test Runner)
    /// está corretamente configurada.
    /// </summary>
    public class CoordenadaTestes
    {
        [Test]
        public void CoordenadasComMesmosValores_SaoIguais()
        {
            var a = new Coordenada(3, 7);
            var b = new Coordenada(3, 7);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
            Assert.AreEqual(a, b);
            Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
        }

        [Test]
        public void CoordenadasDiferentes_NaoSaoIguais()
        {
            Assert.AreNotEqual(new Coordenada(3, 7), new Coordenada(7, 3));
        }

        [Test]
        public void Coordenada_FuncionaComoChaveDeDicionario()
        {
            var conjunto = new HashSet<Coordenada> { new Coordenada(1, 2), new Coordenada(1, 2), new Coordenada(2, 1) };

            Assert.AreEqual(2, conjunto.Count);
            Assert.IsTrue(conjunto.Contains(new Coordenada(2, 1)));
        }

        [Test]
        public void SomaESubtracao_OperamPorComponente()
        {
            var a = new Coordenada(5, -2);
            var b = new Coordenada(1, 4);

            Assert.AreEqual(new Coordenada(6, 2), a + b);
            Assert.AreEqual(new Coordenada(4, -6), a - b);
        }

        [TestCase(0, 0, 0, 0, 0)]
        [TestCase(0, 0, 3, 4, 7)]
        [TestCase(2, 5, -1, 1, 7)]
        public void DistanciaManhattan_SomaDiferencasAbsolutas(int x1, int y1, int x2, int y2, int esperada)
        {
            var a = new Coordenada(x1, y1);
            var b = new Coordenada(x2, y2);

            Assert.AreEqual(esperada, a.DistanciaManhattan(b));
            Assert.AreEqual(esperada, b.DistanciaManhattan(a), "A distância deve ser simétrica.");
        }

        [TestCase(0, 0, 0, 0, 0)]
        [TestCase(0, 0, 3, 4, 4)]
        [TestCase(2, 5, -1, 1, 4)]
        public void DistanciaChebyshev_UsaMaiorDiferenca(int x1, int y1, int x2, int y2, int esperada)
        {
            var a = new Coordenada(x1, y1);
            var b = new Coordenada(x2, y2);

            Assert.AreEqual(esperada, a.DistanciaChebyshev(b));
            Assert.AreEqual(esperada, b.DistanciaChebyshev(a), "A distância deve ser simétrica.");
        }

        [Test]
        public void ToString_FormataComoPar()
        {
            Assert.AreEqual("(3, -1)", new Coordenada(3, -1).ToString());
        }
    }
}
