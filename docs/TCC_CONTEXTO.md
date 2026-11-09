# Contexto do TCC — Geração Procedimental de Mapas Táticos em Grid

Este projeto em Unity é a implementação prática de um Trabalho de Conclusão de Curso (TCC) sobre
**Geração Procedimental de Conteúdo (PCG)** para mapas táticos em grid, voltado a jogos de
estratégia por turnos em cenário isométrico, no estilo de *Rebel Cops*.

## Objetivo acadêmico

Desenvolver um protótipo capaz de:

1. Gerar mapas em grid (visualização isométrica, coordenadas lógicas X,Y) com paredes, coberturas
   e pontos de spawn criados proceduralmente por algoritmos de partição espacial.
2. Validar o mapa por **métricas topológicas** (conectividade, distribuição e acesso a coberturas)
   e por **simulação de partidas** entre duas IAs idênticas, uma de cada lado.
3. **Comparar** mapas com coberturas posicionadas de forma aleatória contra mapas com coberturas
   guiadas pelas métricas, verificando se a proposta melhora o equilíbrio tático.

## Resumo técnico (embasa as decisões de design)

- **Problema central:** abordagens tradicionais de PCG para mapas de estratégia garantem apenas
  conectividade básica ou simetria estrutural entre os lados (ex.: PSMAGE, de Uriarte e Ontañón,
  2013, que usa diagramas de Voronoi e métricas de equilíbrio em mapas de StarCraft). Nenhuma
  valida o **equilíbrio tático local**, isto é, se a distribuição de coberturas oferece
  oportunidades justas aos dois lados.
- **Referencial metodológico:** Togelius et al. (2011) definem a geração procedimental baseada em
  busca (*search-based PCG*) a partir de três eixos: tipo de conteúdo, representação e método de
  avaliação (função de aptidão explícita, simulação ou avaliação interativa). Este projeto usa
  **dois tipos de avaliação**: métricas topológicas diretas e avaliação baseada em simulação
  (partidas entre IAs).
- **Proposta:** um sistema que combina (1) partição espacial para estruturar o mapa,
  (2) colocação de coberturas e (3) validação por métricas topológicas e por simulação.
- **Métricas-alvo (a refinar durante o desenvolvimento):**
  - *Topológicas:* conectividade entre regiões e spawns; distribuição de coberturas por área;
    equidade de acesso a cobertura entre os lados (ex.: distância de caminhada até a cobertura
    mais próxima); ausência de zonas expostas (áreas sem nenhuma cobertura acessível).
  - *Simulação:* taxa de vitória de cada lado com IAs idênticas (perto de 50% indica mapa
    equilibrado), duração média das partidas e taxa de empate por limite de turnos.

## Regras do jogo simulado

**Unidades e ações**
- Cada unidade tem 2 pontos de ação (PA); cada PA serve para andar ou para atacar.
- Alcance de movimento por PA, alcance máximo de ataque e quantidade de unidades por lado:
  configuráveis. Um tiro que acerta elimina a unidade.
- Unidades dos dois lados têm atributos idênticos (isola o efeito do mapa no resultado).
- Movimento em 8 direções (com diagonais).

**Terreno**
- Tipos de célula: piso, parede, meia cobertura, cobertura total e spawn.
- Parede, meia cobertura e cobertura total bloqueiam o movimento.

**Visão, tiro e cobertura**
- A visão é definida apenas pelo alcance: paredes e coberturas **não** bloqueiam o tiro (regra
  assumida, inspirada em *Rebel Cops*; ver DEVLOG).
- A chance base de acerto decai com a distância (função configurável).
- A cobertura reduz a chance de acerto somente quando está adjacente ao alvo, na direção do
  atirador (8 direções). Cobertura total reduz mais que meia cobertura (valores configuráveis).
- Atirador em flanco (sem cobertura adjacente ao alvo na direção do tiro): sem redução.

**Partida**
- Turnos alternados unidade por unidade, um lado e depois o outro.
- Fim: um lado elimina todas as unidades do outro, ou limite de turnos (empate).
- IA heurística simples (aproximar-se, buscar cobertura, atacar), idêntica nos dois lados.
- Todo sorteio (mapa e tiros) usa seed configurável, para experimentos reproduzíveis.

## Stack e requisitos técnicos

- Unity 6.3 LTS (6000.3.x), C#, Tilemap isométrica com tiles placeholder gerados por código.
- Toda a lógica (mapa, unidades, regras, geração, métricas, IAs, simulação) fica no **Núcleo**
  (C# puro, sem Unity), para rodar em testes e em lote.
- Parâmetros (tamanho do mapa, PA, alcances, reduções de cobertura, limite de turnos, seeds,
  quantidade de unidades) configuráveis, sem valores fixos no código.
- Código, nomes e comentários em português. Testes automatizados (Unity Test Framework).

## Roteiro de etapas

1. ✅ Estrutura do projeto + grid isométrico configurável.
2. Representação lógica do mapa (`MapaTatico`, tipos de célula, vizinhança de 8, mapa de teste).
3. Unidades e pontos de ação (alcance de movimento por BFS, alcance de ataque).
4. Chance de acerto e cobertura (decaimento por distância, redução por cobertura, sorteio com seed).
5. Unidades na Unity (exibição, destaque de alcances, chance de acerto por alvo).
6. Gerador procedural (partição espacial, paredes, spawns, conectividade).
7. Colocação de coberturas: versão aleatória e versão guiada pelas métricas.
8. Turnos e IAs (fluxo da partida, IAs heurísticas idênticas, partidas em lote sem Unity).
9. Métricas e experimentos (topológicas, de simulação, comparação aleatória × guiada, CSV).

O histórico de decisões técnicas fica em [`DEVLOG.md`](../DEVLOG.md).
