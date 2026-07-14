# Decisoes de Arquitetura e Tecnologia — OniBus Express

> Documento **vivo**: atualizado a cada fase do desenvolvimento.
> No final do projeto, serve como o relatorio consolidado das decisoes tomadas
> e do porque de cada uma.

## Visao geral

| Item        | Decisao                                                        |
| ----------- | -------------------------------------------------------------- |
| Escopo      | Full Stack (Backend .NET + Frontend React integrados)         |
| Backend     | .NET 8 (ASP.NET Core Web API), Clean Architecture             |
| Banco       | PostgreSQL (EF Core + Npgsql)                                  |
| Frontend    | React 18 + TypeScript, build com Vite                         |
| Estado (FE) | Zustand                                                        |
| Testes BE   | xUnit + SQLite in-memory (integracao)                         |
| Testes FE   | Vitest + React Testing Library                                 |
| Orquestracao| Docker Compose (1 comando) + Makefile (comandos separados)    |
| Qualidade   | Git hooks nativos (pre-commit format/lint + commit-msg)       |

---

## Registro de decisoes (por fase)

### Fase 0 — Fundacao do repositorio

**Contexto:** montar a base do monorepo antes de escrever codigo de produto.

- **Monorepo (`backend/` + `frontend/`)** — mantem back e front no mesmo
  repositorio, facilita a entrega unica e o `docker-compose` orquestrando tudo.
- **PostgreSQL em vez de SQL Server** — imagem Docker mais leve, gratuito e
  provider Npgsql maduro no EF Core. Melhor custo/beneficio para um MVP.
- **Vite + Vitest + Zustand** — Vite oferece build/dev-server rapidos; Vitest
  integra nativamente com o Vite (mesma config); Zustand traz estado global
  simples, com pouco boilerplate, adequado ao tamanho do MVP.
- **Makefile para orquestracao local** — `make dev` sobe back+front em paralelo
  (`make -j2`), e `make back` / `make front` rodam cada um isolado. Escolhido por
  ser leve e agnostico de linguagem (funciona bem num monorepo .NET + Node).
- **Sem Husky — git hooks nativos** — em vez de depender do ecossistema Node so
  para tooling de commit, usamos hooks versionados em `.githooks/` ativados via
  `core.hooksPath`:
  - `pre-commit`: roda `dotnet format --verify-no-changes` (backend) e `eslint`
    (frontend). **Bloqueia o commit** se algo estiver fora do padrao.
  - `commit-msg`: valida Conventional Commits com regex em bash puro
    (zero dependencias).
- **`.editorconfig` + analyzers Roslyn** — padroniza estilo e habilita regras de
  qualidade que o `dotnet format` usa como fonte da verdade.
- **.NET 8 SDK instalado localmente** — melhor DX (build/test rapidos,
  IntelliSense) e permite o hook de `dotnet format` rodar nativo.

### Fase 1 — Experiencia de Run (DX)

**Contexto:** dar uma experiencia de subida agradavel — 1 comando para tudo, mas
tambem execucao separada — com feedback claro no terminal ("agora docker / agora
backend / agora frontend") e uma animacao do onibus.

- **Scripts dedicados em `scripts/` (SRP)** — `up.sh` (Docker), `dev.sh` (local,
  sem Docker) e `lib/ui.sh` (apresentacao). Cada arquivo tem uma responsabilidade;
  o Makefile so orquestra e chama os scripts. Lógica de shell complexa (animacao,
  polling de readiness) sai do Makefile, onde seria dificil de manter.
- **`lib/ui.sh` como fonte unica de UI (DRY)** — cores, deteccao do Compose,
  spinner, barra de progresso e o renderer da animacao (onibus fixo + estrada
  rolando) ficam num unico lugar, reusados pelos dois scripts.
- **Auto-deteccao do Docker Compose** — `docker compose` (v2) ou `docker-compose`
  (v1/standalone). O ambiente de desenvolvimento so tinha o standalone, entao
  fixar `docker compose` quebraria o `make up`. A deteccao esta no Makefile
  (`COMPOSE := $(shell ...)`) e na lib (`ui_detect_compose`).
- **Degradacao graciosa** — sem TTY, com `NO_COLOR` ou `--plain`, a saida vira
  linhas simples (seguro para CI e para logs redirecionados); serviços ainda
  inexistentes (backend/frontend) sao pulados, e ha um modo `--demo`/`make demo`
  que mostra a animacao mesmo antes de existir codigo de produto.
- **Limpeza no Ctrl-C (`trap`)** — durante o boot via Docker, interromper derruba
  os containers; no modo local, mata os processos-filho (API/Vite). Sem estado orfao.

---

## O que ficou de fora (e por que) — a preencher

_A ser detalhado nas fases finais._

## Melhorias futuras (com mais tempo) — a preencher

_A ser detalhado nas fases finais._
