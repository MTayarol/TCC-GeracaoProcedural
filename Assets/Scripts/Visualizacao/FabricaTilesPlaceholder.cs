using UnityEngine;
using UnityEngine.Tilemaps;

namespace AlgoritmoPCG.Visualizacao
{
    /// <summary>
    /// Cria, por código, os tiles "placeholder" usados para desenhar o mapa: um losango
    /// branco com borda cinza, que é tingido com a cor de cada tipo de célula.
    ///
    /// Gerar o sprite em tempo de execução evita depender de qualquer asset artístico
    /// (requisito do projeto) e permite criar quantas cores forem necessárias nas
    /// próximas etapas (piso, parede, cobertura, spawn...) sem mexer em arquivos.
    /// </summary>
    public static class FabricaTilesPlaceholder
    {
        // Proporção 2:1 — a mesma do Cell Size (1 x 0,5) do Grid isométrico.
        private const int LarguraTexturaPx = 64;
        private const int AlturaTexturaPx = 32;

        // A partir desta "distância normalizada" do centro do losango, o pixel é borda.
        private const float InicioBorda = 0.88f;

        private static Sprite _spriteLosango;

        /// <summary>
        /// Sprite de losango compartilhado por todos os tiles placeholder.
        /// É recriado se tiver sido destruído (ex.: ao sair do Play Mode).
        /// </summary>
        public static Sprite SpriteLosango
        {
            get
            {
                if (_spriteLosango == null)
                    _spriteLosango = CriarSpriteLosango();
                return _spriteLosango;
            }
        }

        /// <summary>
        /// Cria um tile isométrico placeholder com a cor informada.
        /// O tile não é salvo em disco (HideFlags.DontSave); quem o cria é responsável
        /// por destruí-lo quando não for mais necessário.
        /// </summary>
        public static Tile CriarTile(Color cor, string nome)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.name = nome;
            tile.sprite = SpriteLosango;
            tile.color = cor;
            tile.colliderType = Tile.ColliderType.None;
            tile.hideFlags = HideFlags.DontSave;
            return tile;
        }

        private static Sprite CriarSpriteLosango()
        {
            var textura = new Texture2D(LarguraTexturaPx, AlturaTexturaPx, TextureFormat.RGBA32, false)
            {
                name = "LosangoPlaceholder",
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };

            var transparente = new Color32(0, 0, 0, 0);
            var borda = new Color32(140, 140, 140, 255);
            var interior = new Color32(255, 255, 255, 255);

            float meiaLargura = LarguraTexturaPx / 2f;
            float meiaAltura = AlturaTexturaPx / 2f;
            var pixels = new Color32[LarguraTexturaPx * AlturaTexturaPx];

            for (int y = 0; y < AlturaTexturaPx; y++)
            {
                for (int x = 0; x < LarguraTexturaPx; x++)
                {
                    // Um ponto está dentro do losango quando |dx|/meiaLargura + |dy|/meiaAltura <= 1.
                    float dx = Mathf.Abs(x + 0.5f - meiaLargura) / meiaLargura;
                    float dy = Mathf.Abs(y + 0.5f - meiaAltura) / meiaAltura;
                    float distancia = dx + dy;

                    Color32 cor;
                    if (distancia > 1f) cor = transparente;
                    else if (distancia > InicioBorda) cor = borda;
                    else cor = interior;

                    pixels[y * LarguraTexturaPx + x] = cor;
                }
            }

            textura.SetPixels32(pixels);
            textura.Apply();

            // pixelsPerUnit = largura em pixels => o sprite ocupa exatamente 1 x 0,5 unidades,
            // casando com o tamanho de célula do Grid isométrico.
            var sprite = Sprite.Create(
                textura,
                new Rect(0, 0, LarguraTexturaPx, AlturaTexturaPx),
                new Vector2(0.5f, 0.5f),
                LarguraTexturaPx);
            sprite.name = "LosangoPlaceholder";
            sprite.hideFlags = HideFlags.DontSave;
            return sprite;
        }
    }
}
