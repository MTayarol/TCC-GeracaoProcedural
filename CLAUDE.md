# Instruções para sessões de desenvolvimento

- Contexto acadêmico completo: [docs/TCC_CONTEXTO.md](docs/TCC_CONTEXTO.md). Histórico de decisões: [DEVLOG.md](DEVLOG.md).
- Unity 6000.3.24f1 (URP 2D). Código, nomes e comentários em **português**.
- Arquitetura em assemblies:
  - `Assets/Scripts/Nucleo` (`AlgoritmoPCG.Nucleo`): C# puro, `noEngineReferences: true`. Dados do mapa, geração e métricas.
  - `Assets/Scripts/Visualizacao` (`AlgoritmoPCG.Visualizacao`): MonoBehaviours que só *leem* o núcleo e desenham na Tilemap.
  - `Assets/Scripts/Editor` (`AlgoritmoPCG.Ferramentas`): utilitários de editor (menu **PCG**).
  - `Assets/Tests/EditMode` (`AlgoritmoPCG.Testes.EditMode`): testes NUnit do núcleo.
- Regras de trabalho: etapas pequenas e incrementais, uma por vez, aguardando confirmação do usuário; commit em português ao final de cada etapa; registrar decisões no DEVLOG.md; explicação didática ao final de cada etapa; não fazer perguntas bloqueantes.
- Rodar testes sem abrir o editor (o editor costuma estar aberto no projeto, então use uma cópia):
  `Unity.exe -batchmode -projectPath <copia> -runTests -testPlatform EditMode -testResults <arquivo.xml>`
