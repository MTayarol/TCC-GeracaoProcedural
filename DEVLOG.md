# DEVLOG — Gerador Procedimental de Mapas Táticos

Registro cronológico das decisões técnicas do protótipo, com motivações e trade-offs, para
referência na escrita do TCC. Contexto acadêmico: [docs/TCC_CONTEXTO.md](docs/TCC_CONTEXTO.md).

---

## Etapa 1 — Estrutura inicial do projeto e grid isométrico (2026-11-09)

### O que foi feito

- Repositório Git inicializado com `.gitignore` para Unity (ignora `Library/`, `Temp/`, `Logs/`,
  `UserSettings/`, arquivos de IDE) e `.gitattributes` (normalização de fim de linha; arquivos
  YAML da Unity com LF).
- Código organizado em **quatro assemblies** (`.asmdef`):

  | Assembly | Pasta | Depende de | Papel |
  |---|---|---|---|
  | `AlgoritmoPCG.Nucleo` | `Assets/Scripts/Nucleo` | nada (`noEngineReferences: true`) | dados do mapa, geração, métricas |
  | `AlgoritmoPCG.Visualizacao` | `Assets/Scripts/Visualizacao` | Nucleo + Unity | desenhar o mapa na Tilemap |
  | `AlgoritmoPCG.Ferramentas` | `Assets/Scripts/Editor` | Nucleo + Visualizacao (só Editor) | menu **PCG** para montar a cena |
  | `AlgoritmoPCG.Testes.EditMode` | `Assets/Tests/EditMode` | Nucleo + NUnit | testes automatizados |

- Tipo `Coordenada` (X, Y) no núcleo, com igualdade, hash e distâncias de Manhattan e Chebyshev.
- `VisualizadorGrid`: desenha um grid Largura × Altura (configurável no Inspector) em uma Tilemap
  isométrica, enquadra a câmera automaticamente e mostra gizmos de depuração (contorno do mapa,
  eixos X/Y e rótulos de coordenada) na Scene view.
- `FabricaTilesPlaceholder`: gera por código um sprite de losango 64×32 px e tiles coloridos a
  partir dele.
- Menu **PCG → Criar Cena do Protótipo**, que monta a cena `Assets/Scenes/Prototipo.unity`.
- Testes EditMode de `Coordenada` (também validam a infraestrutura de testes).

### Decisões e justificativas

1. **Núcleo em C# puro, isolado por assembly.** O requisito de separar representação lógica e
   camada visual foi imposto pelo *compilador*, não apenas por convenção: o assembly `Nucleo`
   tem `noEngineReferences: true`, então qualquer tentativa de usar `UnityEngine` ali gera erro
   de compilação. Benefícios para o TCC: (a) o algoritmo de PCG e as métricas ficam testáveis
   isoladamente e rapidamente (sem cena, sem Play Mode); (b) a contribuição científica — geração
   + avaliação — fica independente do motor e poderia ser reaproveitada em outro contexto (ex.:
   rodar milhares de gerações em lote para coletar estatísticas). Isso corresponde, no quadro de
   Togelius et al. (2011), a separar claramente a *representação* do conteúdo do *método de
   avaliação* e da *apresentação*.
2. **Grid isométrico apenas como projeção.** O núcleo trabalha com coordenadas lógicas (X, Y)
   inteiras; a isometria existe só na camada visual (`Grid` com `Cell Layout = Isometric` e
   `Cell Size = (1; 0,5)`, proporção 2:1). A tradução lógica → visual está concentrada em um
   único método (`VisualizadorGrid.ParaCelula`). Assim, cálculos como linha de visão e distância
   são feitos num grid ortogonal simples, sem distorções da projeção.
3. **Tiles placeholder gerados em tempo de execução** (textura + `Sprite.Create` +
   `ScriptableObject.CreateInstance<Tile>()`), com `HideFlags.DontSave`. Evita depender de
   assets artísticos e permite criar novos "tipos" de célula (parede, cobertura, spawn) apenas
   escolhendo uma cor. Trade-off: os tiles não existem como arquivos no projeto, então o mapa só
   aparece no Play Mode (a Scene view mostra o contorno/coordenadas via gizmos). Considerado
   aceitável porque o conteúdo é procedural por natureza.
4. **Material `Sprite-Unlit-Default` do URP.** O projeto usa o Renderer 2D do URP; com o material
   *lit* padrão, as cores dependeriam de luzes 2D. Como as cores codificam informação (tipo de
   célula, e futuramente métricas), elas devem aparecer exatamente como definidas.
5. **Cena montada por código** (menu PCG). Torna a configuração reproduzível e documentada,
   em vez de depender de passos manuais no editor.
6. **`SetTiles` em lote** em vez de `SetTile` célula a célula, para que mapas grandes
   (ex.: 100×100) sejam redesenhados rapidamente ao alterar o tamanho no Inspector.
7. Removido o script vazio `NewMonoBehaviourScript.cs` (template padrão, sem referências).
   A cena `SampleScene` e a paleta `PaletaTeste` existentes foram mantidas intactas.

### Verificação

- Testes EditMode executados em batchmode (`Unity.exe -runTests -testPlatform EditMode`) numa
  cópia do projeto.
- Cena criada via `-executeMethod AlgoritmoPCG.Ferramentas.ConfiguradorCenaPrototipo.CriarCenaPrototipo`.
