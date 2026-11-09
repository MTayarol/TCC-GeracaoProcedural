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

---

## Revisão do escopo (2026-11-09, antes da Etapa 2)

O escopo do TCC foi detalhado: além das métricas topológicas, o mapa também será avaliado por
**simulação de partidas** entre duas IAs idênticas, e o experimento final compara coberturas
**aleatórias × guiadas pelas métricas**. As regras do jogo simulado (2 PA, 8 direções, tiro
sem bloqueio de visão, redução por cobertura adjacente ao alvo, turnos alternados, seeds) e o
novo roteiro de 9 etapas foram registrados em `docs/TCC_CONTEXTO.md` e `CLAUDE.md`.

**Regra assumida — visão sem bloqueio:** paredes e coberturas *não* bloqueiam o tiro; a visão
é limitada só pelo alcance. A regra pretendida é inspirada em *Rebel Cops*, mas não foi
possível confirmar as regras exatas desse jogo, então ela é tratada aqui como **regra
assumida** do protótipo. Consequência para o TCC: o valor tático de uma posição vem só da
cobertura adjacente e da distância, e não de esconder-se atrás de paredes. Isso simplifica a
análise e deixa o efeito da *distribuição de coberturas* (a variável do experimento) mais
isolado.

---

## Etapa 2 — Representação lógica do mapa (2026-11-09)

### O que foi feito

- **Núcleo**
  - `TipoCelula` (Piso, Parede, MeiaCobertura, CoberturaTotal, Spawn) + extensões
    `BloqueiaMovimento`, `EhTransitavel`, `EhCobertura`.
  - `Lado` (Nenhum, A, B) + `Oponente()`.
  - `Direcoes`: as 8 direções (Norte = +Y, Leste = +X), ortogonais e diagonais.
  - `MapaTatico`: grid Largura × Altura; leitura/escrita de células; spawns com lado;
    `Vizinhos8`, `VizinhosTransitaveis`, `PodeMoverEntreAdjacentes`; enumeração/contagem por
    tipo e por lado; `Clonar()`.
  - `ConversorMapaTexto`: mapas em ASCII (`. # h H A B`) nos dois sentidos.
  - `MapasDeExemplo`: mapa de demonstração paramétrico (qualquer tamanho ≥ 8×6) e um mapa de
    teste 14×9 feito à mão, ambos com todos os tipos de célula e simetria de rotação de 180°.
- **Visualização**
  - `VisualizadorGrid` renomeado para `VisualizadorMapa` (mesmo `.meta`/GUID): agora desenha um
    `MapaTatico` recebido, com uma cor por tipo (spawn A azul, spawn B vermelho) definida em
    `PaletaCores`, editável no Inspector.
  - `ControladorPrototipo`: escolhe a origem do mapa (Demonstração com tamanho configurável,
    ou Texto ASCII digitado no Inspector) e entrega ao visualizador.
  - Pré-visualização na Scene view **fora do Play Mode** (gizmos pintam as células), sem
    alterar a Tilemap salva na cena.
- **Testes:** `MapaTaticoTestes`, `ConversorMapaTextoTestes`, `MapasDeExemploTestes`.

### Decisões e justificativas

1. **Armazenamento em vetor 1D (`y * Largura + x`)** em vez de `TipoCelula[,]` ou de objetos
   por célula. É compacto (1 byte por célula), rápido de copiar e amigável ao cache. Pesa nas
   Etapas 8–9, em que milhares de mapas serão gerados e simulados em lote.
2. **Spawn é um tipo de célula, e o lado fica num vetor paralelo.** O pedido lista "spawn"
   como tipo de célula, mas dois lados precisam de spawns distintos. Em vez de criar `SpawnA` e
   `SpawnB` como tipos, o lado fica guardado à parte, e a API obriga a informá-lo
   (`DefinirSpawn(c, lado)`; `DefinirTipo(c, Spawn)` lança exceção). Assim, regras que só
   perguntam "é transitável?" tratam qualquer spawn igual, e não existe spawn sem lado.
3. **Spawn é transitável** (funciona como piso para o movimento) e **não é cobertura**.
4. ~~**Parede não conta como cobertura.**~~ *(Revogada logo após a etapa; ver
   "Mudança de regra: parede como cobertura".)* Pelas regras, coberturas reduzem a chance de acerto e
   paredes só bloqueiam o movimento (e, pela regra assumida, nem o tiro). Logo, uma unidade
   colada a uma parede não ganha proteção. Os dois papéis ficam separados de propósito, para
   que a métrica de cobertura conte só o que o gerador posicionou como cobertura.
5. **Diagonais perto de paredes: "não cortar quina".** Um passo diagonal só é permitido se as
   **duas** células ortogonais que ele contorna forem transitáveis. Exemplos: ir de (0,0) para
   (1,1) exige que (1,0) e (0,1) estejam livres; uma unidade não atravessa a quina de uma
   parede nem passa espremida entre dois obstáculos que se tocam pela diagonal. Coberturas
   contam como obstáculo nessa regra, porque também bloqueiam o movimento.
   - *Alternativas consideradas:* (a) permitir qualquer diagonal livre, o que deixaria as
     unidades "atravessarem" paredes diagonais finas e quebraria a noção de corredor/sala
     que a partição espacial vai criar; (b) bloquear só quando os **dois** ortogonais estão
     bloqueados, um meio-termo que ainda deixa cortar quinas.
   - *Motivo:* é a regra mais restritiva e mais intuitiva visualmente. Ela garante que
     conectividade em 8 direções implique um caminho "real", sem frestas diagonais. Nas
     métricas de conectividade (Etapa 6), basta usar a mesma função de vizinhança.
   - *Custo:* um passo diagonal sempre custa 1 passo (distância de Chebyshev). O custo do
     movimento é decidido na Etapa 3.
6. **Mapa em texto ASCII** como formato de teste e depuração: os testes descrevem situações
   espaciais de forma legível (o próprio teste "desenha" a parede e a cobertura), e mapas
   gerados poderão ser impressos em logs e anexados ao TCC. Convenção: a primeira linha do
   texto é a de maior Y (leitura cartesiana).
7. **Mapas de exemplo com simetria de rotação de 180°.** Servem de referência de mapa "justo
   por construção", isto é, com simetria estrutural no estilo do PSMAGE. Nas Etapas 7–9, um
   mapa simétrico deve dar ~50% de vitórias na simulação, o que serve de teste de sanidade
   para as métricas e as IAs. A simetria é verificada por teste automatizado.
8. **Produtor ≠ desenhista.** `ControladorPrototipo` decide *qual* mapa existe;
   `VisualizadorMapa` só o desenha. Na Etapa 6, o gerador procedural entra como mais uma
   "fonte" no controlador, sem tocar no visualizador.
9. **Pré-visualização por gizmos** em vez de escrever na Tilemap fora do Play Mode: preencher
   a Tilemap no editor "sujaria" a cena salva com tiles temporários (que não existem em disco).

### Valores padrão escolhidos

- Demonstração: 16 × 12 (mínimo 8 × 6), 3 spawns por lado.
- Cores: piso cinza-esverdeado, parede grafite, meia cobertura amarela, cobertura total
  laranja, spawn A azul, spawn B vermelho.

### Verificação

- 66 testes EditMode passando (batchmode, cópia do projeto); compilação sem avisos.
- O Núcleo também foi compilado fora da Unity (`dotnet`, .NET 10), o que confirma que não
  depende do motor.
- Cena `Prototipo.unity` regenerada pelo menu PCG com `VisualizadorMapa` + `ControladorPrototipo`.

---

## Mudança de regra: parede como cobertura (2026-11-09, após a Etapa 2)

### O que mudou

- **Paredes passam a funcionar como cobertura.** Continuam bloqueando o movimento e continuam
  **não** bloqueando o tiro (a visão segue limitada só pelo alcance).
- A parede protege **nas mesmas condições** das outras coberturas: só quando está adjacente ao
  alvo, na direção do atirador (8 direções).
- A redução da parede é um **parâmetro próprio** (`ParametrosCobertura.ReducaoParede`), separado
  da cobertura total, embora o padrão seja igual (parede protege como cobertura total).
- Não foi criado tipo de célula novo: `TipoCelulaExtensoes.EhCobertura` passou a incluir
  `Parede`, e todo código que pergunta "é cobertura?" já herda a regra nova.
- A decisão 4 da Etapa 2 ("parede não conta como cobertura") ficou **revogada**.

### Motivo

Em jogos táticos isométricos, encostar numa parede para se proteger é o comportamento que o
jogador espera, e ignorar isso deixaria o mapa simulado pouco fiel ao gênero. Como a geração vai
usar partição espacial (salas e corredores delimitados por paredes), uma parcela grande das
posições protegidas do mapa vem justamente das paredes. Deixar essas posições fora da análise
distorceria tanto as métricas quanto a simulação.

### Regra assumida

Assim como a visão sem bloqueio, esta é uma **regra assumida** do protótipo, inspirada em
*Rebel Cops* sem confirmação das regras exatas do jogo. Os valores padrão também são assumidos:

| Cobertura | Redução padrão |
|---|---|
| Meia cobertura | 0,25 |
| Cobertura total | 0,50 |
| Parede | 0,50 (= total) |

A intenção para a Etapa 4 é aplicar a redução de forma **multiplicativa**:
`chanceFinal = chanceBase(distância) × (1 − redução)`. Assim a chance fica sempre em [0, 1]
e a cobertura tem efeito proporcional à distância.

Manter a redução da parede como parâmetro separado também serve ao experimento: dá para rodar
uma variante com `ReducaoParede = 0` e medir quanto do equilíbrio de um mapa vem das paredes
(estrutura gerada pela partição) e quanto vem das coberturas posicionadas.

### Direção do atirador (antecipado da Etapa 4)

Os testes pedidos precisavam da regra geométrica "qual vizinho do alvo fica na direção do
atirador", então ela foi antecipada em `RegrasCobertura` (sem a chance de acerto, que continua
na Etapa 4):

- O vetor alvo→atirador é convertido em ângulo (`atan2`) e arredondado para o **setor de 45°
  mais próximo**, que dá uma das 8 direções. Com coordenadas inteiras nunca há empate entre
  setores, porque as fronteiras (22,5°, 67,5°...) têm tangente irracional.
- O alvo está protegido se o vizinho nessa direção existir no mapa e `EhCobertura` for
  verdadeiro. Caso contrário, é flanco (redução 0).
- *Alternativa considerada:* usar o sinal de dx e dy (`sign(dx), sign(dy)`). Foi descartada
  porque transforma quase todo tiro em diagonal. Por exemplo, um atirador 10 casas a leste e 1
  ao norte seria tratado como "nordeste".

### Consequência para as métricas (Etapas 6 a 9)

As métricas de **acesso a cobertura** (distância até a cobertura mais próxima, zonas expostas,
cobertura por área, equidade entre os lados) **devem contar as paredes** como cobertura. Para
garantir isso, o código das métricas deve sempre usar `EhCobertura`/`RegrasCobertura`, e nunca
listar tipos de célula à mão.

Efeito colateral a observar: a **borda de paredes** do mapa conta como cobertura, mas, pela
regra direcional, ela só protegeria contra tiros vindos de fora do mapa, o que nunca acontece.
Um teste confirma que um alvo encostado na borda não recebe redução contra nenhum atirador
interno. Nas métricas, será preciso decidir se "célula adjacente a cobertura" conta paredes
de borda. A tendência é contar só coberturas que protejam contra alguma direção de onde um
inimigo possa atirar.

### Testes

- Atualizado: `RegrasDoTipo_TransitavelECobertura` (Parede → cobertura = verdadeiro).
- Novos, em `RegrasCoberturaTestes`: arredondamento da direção em 9 casos; parede adjacente na
  direção do atirador protege; parede na diagonal protege contra tiro diagonal mas não de
  flanco; parede do lado oposto não protege; parede da borda não protege contra nenhum tiro
  vindo de dentro; redução da parede configurável e independente da cobertura total; redução
  padrão de cada tipo; validação dos parâmetros.
