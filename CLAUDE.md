# Instruções para sessões de desenvolvimento

- Contexto acadêmico completo: [docs/TCC_CONTEXTO.md](docs/TCC_CONTEXTO.md). Histórico de decisões: [DEVLOG.md](DEVLOG.md). Leia os dois antes de mudar qualquer coisa e não refaça o que já existe.
- Unity 6000.3.24f1 (URP 2D). Código, nomes e comentários em **português**.

## Arquitetura (manter)

- `Assets/Scripts/Nucleo` (`AlgoritmoPCG.Nucleo`): C# puro, `noEngineReferences: true`. **Toda** a lógica: mapa, unidades, regras, geração, métricas, IAs, simulação.
- `Assets/Scripts/Visualizacao` (`AlgoritmoPCG.Visualizacao`): MonoBehaviours que só *leem* o núcleo e desenham na Tilemap.
- `Assets/Scripts/Editor` (`AlgoritmoPCG.Ferramentas`): utilitários de editor (menu **PCG**).
- `Assets/Tests/EditMode` (`AlgoritmoPCG.Testes.EditMode`): testes NUnit do núcleo.
- Parâmetros sempre configuráveis (Inspector / objetos de configuração), nunca fixos no código.

## Regras do jogo simulado

- Unidades: 2 PA por turno; cada PA = andar ou atacar. Alcance de movimento por PA, alcance de ataque, precisão e nº de unidades por lado configuráveis. Atributos idênticos nos dois lados. Tiro que acerta elimina.
- Movimento em 8 direções. Diagonal não corta canto (ver DEVLOG, Etapa 2).
- Células: piso, parede, meia cobertura, cobertura total, spawn (com lado A/B). Parede e coberturas bloqueiam movimento.
- Visão só por alcance: paredes/coberturas NÃO bloqueiam tiro (regra assumida).
- Chance base decai com a distância (função configurável). Cobertura só reduz se adjacente ao alvo na direção do atirador (8 direções); total reduz mais que meia. Flanco = sem redução.
- Turnos alternados unidade a unidade. Fim por eliminação total ou limite de turnos (empate).
- IAs heurísticas idênticas (aproximar, buscar cobertura, atacar). Todo sorteio com seed configurável.

## Roteiro

1. ✅ Estrutura + grid isométrico. 2. Representação lógica do mapa. 3. Unidades e PA. 4. Chance de acerto e cobertura. 5. Unidades na Unity. 6. Gerador procedural (partição espacial). 7. Coberturas aleatória × guiada. 8. Turnos e IAs (lote sem Unity). 9. Métricas, experimentos e CSV.

## Regras de trabalho

- Etapas pequenas e incrementais, uma por vez, aguardando confirmação do usuário.
- Commit em português ao final de cada etapa. Registrar decisões, regras assumidas e valores padrão no DEVLOG.md.
- Explicação detalhada e didática ao final de cada etapa. Não fazer perguntas bloqueantes; checklist `- [ ]` só quando houver algo relevante.
- Rodar testes sem abrir o editor (o editor costuma estar aberto no projeto, então use uma cópia sem `Library/`):
  `Unity.exe -batchmode -projectPath <copia> -runTests -testPlatform EditMode -testResults <arquivo.xml>`
  A cena do protótipo pode ser regenerada com `-executeMethod AlgoritmoPCG.Ferramentas.ConfiguradorCenaPrototipo.CriarCenaPrototipo -quit`.
