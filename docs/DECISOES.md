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

---

## O que ficou de fora (e por que) — a preencher

_A ser detalhado nas fases finais._

## Melhorias futuras (com mais tempo) — a preencher

_A ser detalhado nas fases finais._
