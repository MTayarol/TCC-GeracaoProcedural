# Contexto do TCC — Geração Procedimental de Mapas Táticos em Grid

Este projeto em Unity é a implementação prática de um Trabalho de Conclusão de Curso (TCC) sobre
**Geração Procedimental de Conteúdo (PCG)** para mapas táticos em grid, voltado a jogos de
estratégia por turnos.

## Objetivo acadêmico

Desenvolver um protótipo capaz de:

1. Gerar mapas em grid (visualização isométrica, coordenadas lógicas X,Y) usando algoritmos de
   partição espacial.
2. Validar taticamente o mapa gerado por meio de análise topológica de cobertura (*cover*) e
   linhas de visão (*line of sight*).
3. Garantir conectividade entre pontos do mapa e equilíbrio na distribuição dos elementos de
   cobertura.

## Resumo técnico (embasa as decisões de design)

- **Problema central:** abordagens tradicionais de PCG para mapas de estratégia garantem apenas
  (a) conectividade básica entre pontos do mapa ou (b) simetria estrutural entre os lados
  (ex.: PSMAGE, de Uriarte e Ontañón, 2013, que usa diagramas de Voronoi para gerar regiões
  balanceadas em mapas de StarCraft). Nenhuma dessas abordagens valida o **equilíbrio tático
  local** — ou seja, se as posições geradas oferecem cobertura e linhas de visão justas entre os
  jogadores.
- **Referencial metodológico:** Togelius et al. (2011) definem a Geração Procedimental Baseada em
  Busca (*search-based PCG*) a partir de três eixos: tipo de conteúdo gerado, representação
  interna do conteúdo e método de avaliação (função de aptidão explícita, simulação ou avaliação
  interativa). Este projeto se posiciona no eixo de **avaliação por simulação/métricas
  topológicas**.
- **Proposta do TCC:** um sistema que combina (1) um algoritmo de partição espacial para
  estruturar o ambiente em grid e (2) uma etapa de análise topológica que avalia e, se
  necessário, ajusta a distribuição de coberturas e linhas de visão, de modo a garantir
  conectividade e equilíbrio tático.
- **Métricas-alvo de validação (a refinar durante o desenvolvimento):**
  - conectividade entre regiões/pontos de spawn;
  - distribuição de elementos de cobertura por área;
  - simetria ou equidade de oportunidades táticas entre os lados do mapa;
  - ausência de "zonas cegas" (áreas sem nenhuma cobertura acessível).

## Stack e requisitos técnicos

- Unity 6.3 LTS (6000.3.x), C#.
- Tilemap isométrica (Grid + Tilemap, Cell Layout = Isometric) com tiles placeholder gerados por
  código (losangos coloridos), sem depender de assets artísticos.
- Tamanho do grid configurável pelo Inspector.
- Separação entre a **representação lógica** do mapa (C# puro, sem dependência da Unity) e a
  **camada visual** (Tilemap). O algoritmo de PCG manipula apenas os dados.
- Código, nomes e comentários em português.
- Testes automatizados (Unity Test Framework) para o gerador e para as métricas táticas.

## Roteiro de etapas

1. Estrutura inicial do projeto + grid isométrico configurável.
2. Representação lógica dos dados do mapa.
3. Algoritmo de geração / partição espacial.
4. Métricas de validação tática (cobertura, linha de visão, conectividade, equilíbrio).

O histórico de decisões técnicas fica em [`DEVLOG.md`](../DEVLOG.md).
