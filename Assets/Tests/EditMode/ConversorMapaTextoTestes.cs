using System;
using AlgoritmoPCG.Nucleo;
using NUnit.Framework;

namespace AlgoritmoPCG.Testes
{
    public class ConversorMapaTextoTestes
    {
        [Test]
        public void Ler_PrimeiraLinhaDoTextoEhOMaiorY()
        {
            var mapa = ConversorMapaTexto.Ler("#.\n..");

            Assert.AreEqual(2, mapa.Largura);
            Assert.AreEqual(2, mapa.Altura);
            Assert.AreEqual(TipoCelula.Parede, mapa.ObterTipo(new Coordenada(0, 1)));
            Assert.AreEqual(TipoCelula.Piso, mapa.ObterTipo(new Coordenada(0, 0)));
        }

        [Test]
        public void Ler_ReconheceTodosOsCaracteres()
        {
            var mapa = ConversorMapaTexto.Ler(".#hHAB");

            Assert.AreEqual(TipoCelula.Piso, mapa.ObterTipo(new Coordenada(0, 0)));
            Assert.AreEqual(TipoCelula.Parede, mapa.ObterTipo(new Coordenada(1, 0)));
            Assert.AreEqual(TipoCelula.MeiaCobertura, mapa.ObterTipo(new Coordenada(2, 0)));
            Assert.AreEqual(TipoCelula.CoberturaTotal, mapa.ObterTipo(new Coordenada(3, 0)));
            Assert.AreEqual(Lado.A, mapa.ObterLadoSpawn(new Coordenada(4, 0)));
            Assert.AreEqual(Lado.B, mapa.ObterLadoSpawn(new Coordenada(5, 0)));
        }

        [Test]
        public void Ler_IgnoraLinhasVaziasNasPontas_Indentacao_EQuebrasDeLinhaDoWindows()
        {
            var mapa = ConversorMapaTexto.Ler("\r\n\r\n    #.  \r\n    ..\r\n\r\n");

            Assert.AreEqual(2, mapa.Largura);
            Assert.AreEqual(2, mapa.Altura);
        }

        [Test]
        public void EscreverDepoisLer_PreservaOMapa()
        {
            const string texto = "#####\n#A.h#\n#.H.#\n#h.B#\n#####";

            Assert.AreEqual(texto, ConversorMapaTexto.Escrever(ConversorMapaTexto.Ler(texto)));
        }

        [Test]
        public void LinhasDeTamanhosDiferentes_LancamFormatException()
        {
            Assert.Throws<FormatException>(() => ConversorMapaTexto.Ler("...\n.."));
        }

        [Test]
        public void CaractereDesconhecido_LancaFormatExceptionIndicandoAPosicao()
        {
            var erro = Assert.Throws<FormatException>(() => ConversorMapaTexto.Ler("...\n.x."));
            StringAssert.Contains("linha 2, coluna 2", erro.Message);
        }

        [Test]
        public void TextoVazio_LancaFormatException()
        {
            Assert.Throws<FormatException>(() => ConversorMapaTexto.Ler("  \n\n"));
        }
    }
}
