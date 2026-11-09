using System;

namespace AlgoritmoPCG.Nucleo
{
    /// <summary>
    /// Mapas construídos "à mão" (por regras fixas, sem sorteio) para testar e
    /// demonstrar a representação do mapa antes de existir o gerador procedural.
    /// </summary>
    public static class MapasDeExemplo
    {
        public const int LarguraMinimaDemonstracao = 8;
        public const int AlturaMinimaDemonstracao = 6;

        /// <summary>
        /// Mapa de teste 14x9 desenhado à mão em ASCII (legenda em <see cref="ConversorMapaTexto"/>),
        /// com todos os tipos de célula e simetria de rotação de 180°: cada linha de baixo
        /// é a linha correspondente de cima invertida, com A e B trocados.
        /// </summary>
        public const string TextoMapaDeTeste =
            "##############\n" +
            "#.....#......#\n" +
            "#..h..#...H..#\n" +
            "#A..H....h...#\n" +
            "#A....##....B#\n" +
            "#...h....H..B#\n" +
            "#..H...#..h..#\n" +
            "#......#.....#\n" +
            "##############";

        /// <summary>
        /// Mapa de demonstração com todos os tipos de célula, que se adapta a qualquer
        /// tamanho a partir de 8x6. Tem borda de paredes, spawns do lado A à esquerda e do
        /// lado B à direita, um par de paredes centrais e algumas coberturas.
        ///
        /// O mapa tem SIMETRIA DE ROTAÇÃO de 180° em torno do centro: a célula (x, y) é do
        /// mesmo tipo que (L-1-x, A-1-y), e os spawns trocam de lado. É o tipo de simetria
        /// estrutural usado por geradores como o PSMAGE — útil como referência de mapa
        /// "justo por construção" quando as métricas táticas forem implementadas.
        /// </summary>
        public static MapaTatico CriarDemonstracao(int largura, int altura)
        {
            if (largura < LarguraMinimaDemonstracao || altura < AlturaMinimaDemonstracao)
                throw new ArgumentOutOfRangeException(nameof(largura),
                    $"O mapa de demonstração precisa ter pelo menos {LarguraMinimaDemonstracao}x{AlturaMinimaDemonstracao} células (recebido {largura}x{altura}).");

            var mapa = new MapaTatico(largura, altura);
            int meioX = largura / 2;
            int meioY = altura / 2;

            // Borda de paredes.
            for (int x = 0; x < largura; x++)
            {
                mapa.DefinirTipo(new Coordenada(x, 0), TipoCelula.Parede);
                mapa.DefinirTipo(new Coordenada(x, altura - 1), TipoCelula.Parede);
            }
            for (int y = 0; y < altura; y++)
            {
                mapa.DefinirTipo(new Coordenada(0, y), TipoCelula.Parede);
                mapa.DefinirTipo(new Coordenada(largura - 1, y), TipoCelula.Parede);
            }

            // Parede central: segmento vertical que sai da borda inferior (e o espelho sai
            // da superior), deixando o meio do mapa aberto.
            int comprimentoParede = Math.Max(1, altura / 3);
            for (int y = 1; y <= comprimentoParede; y++)
                ColocarEspelhado(mapa, new Coordenada(meioX - 1, y), TipoCelula.Parede);

            // Meia cobertura encostada na ponta da parede central.
            ColocarEspelhado(mapa, new Coordenada(meioX - 2, comprimentoParede), TipoCelula.MeiaCobertura);

            // Coberturas na frente dos spawns.
            int xFrente = Math.Max(3, largura / 4);
            ColocarEspelhado(mapa, new Coordenada(xFrente, meioY + 1), TipoCelula.CoberturaTotal);
            ColocarEspelhado(mapa, new Coordenada(xFrente, meioY - 2), TipoCelula.MeiaCobertura);

            // Spawns por último (3 células por lado, centralizadas na vertical).
            for (int dy = -1; dy <= 1; dy++)
            {
                var c = new Coordenada(1, meioY + dy);
                mapa.DefinirSpawn(c, Lado.A);
                mapa.DefinirSpawn(Espelhar(mapa, c), Lado.B);
            }

            return mapa;
        }

        /// <summary>Ponto correspondente pela rotação de 180° em torno do centro do mapa.</summary>
        public static Coordenada Espelhar(MapaTatico mapa, Coordenada c)
        {
            return new Coordenada(mapa.Largura - 1 - c.X, mapa.Altura - 1 - c.Y);
        }

        private static void ColocarEspelhado(MapaTatico mapa, Coordenada c, TipoCelula tipo)
        {
            if (!EhInterior(mapa, c)) return;
            mapa.DefinirTipo(c, tipo);
            mapa.DefinirTipo(Espelhar(mapa, c), tipo);
        }

        private static bool EhInterior(MapaTatico mapa, Coordenada c)
        {
            return c.X >= 1 && c.X <= mapa.Largura - 2 && c.Y >= 1 && c.Y <= mapa.Altura - 2;
        }
    }
}
