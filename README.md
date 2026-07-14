# 🚌 OniBus Express

Sistema de venda de passagens rodoviarias (MVP) — desafio tecnico Full Stack.

> **Status:** em desenvolvimento. Este README sera preenchido ao longo das fases.

## Sumario

- [Sobre](#sobre)
- [Stack](#stack)
- [Como rodar](#como-rodar)
- [Testes](#testes)
- [Estrutura do projeto](#estrutura-do-projeto)
- [Decisoes de arquitetura](#decisoes-de-arquitetura)

## Sobre

A **OniBus Express** e um MVP de venda de passagens de onibus online: busca de
viagens, selecao de assento, compra com dados do passageiro e consulta/cancelamento
de reserva.

## Stack

| Camada    | Tecnologias                                              |
| --------- | -------------------------------------------------------- |
| Backend   | .NET 8, ASP.NET Core Web API, EF Core, PostgreSQL, xUnit |
| Frontend  | React 18, TypeScript, Vite, Zustand, Vitest + RTL        |
| Infra     | Docker + Docker Compose, Nginx (frontend)                |

## Como rodar

### Com Docker (recomendado — 1 comando)

```bash
make up          # sobe API + banco + frontend com feedback animado no terminal
```

O `make up` detecta automaticamente o Docker Compose disponivel — tanto o plugin
v2 (`docker compose`) quanto o standalone (`docker-compose`). Durante o boot voce
acompanha o progresso (docker → banco → backend → …) com uma animacao do onibus;
ao final, os logs sao seguidos automaticamente.

Sobe **PostgreSQL + API + Frontend**. A API **aplica as migrations e faz o seed
automaticamente no startup**. Depois de subir:

- **App (Frontend)**: <http://localhost:8081>
- API: <http://localhost:8080>
- **Swagger / OpenAPI**: <http://localhost:8080/swagger>
- Health: <http://localhost:8080/health> · Versao: <http://localhost:8080/version>

### API (backend)

Endpoints (paths em portugues, como o desafio pede; corpo/JSON em ingles):

| Metodo | Rota                | Descricao                                          |
| ------ | ------------------- | -------------------------------------------------- |
| GET    | `/rotas`            | Lista todas as rotas                               |
| GET    | `/viagens`          | Busca por `origem`, `destino`, `data`              |
| GET    | `/viagens/{id}`     | Detalhe da viagem (assentos livres/ocupados)       |
| POST   | `/reservas`         | Cria reserva (nome, documento, e-mail, viagem, assento) |
| GET    | `/reservas/{codigo}`| Consulta reserva pelo codigo (ex.: `ABC-12345`)    |
| DELETE | `/reservas/{codigo}`| Cancela reserva (ate 2h antes da partida)          |

Erros seguem **ProblemDetails (RFC 7807)** com um `code` estavel
(ex.: `SEAT_TAKEN` → 409, `DOCUMENT_INVALID` → 400, `NOT_FOUND` → 404).

Rodar so a API localmente (precisa de um PostgreSQL; use `docker compose up db`
ou aponte `ConnectionStrings__Default` para o seu banco):

```bash
make back        # dotnet watch run na API
```

Migrations (EF Core) — a API aplica sozinha no startup; para gerenciar manualmente:

```bash
make migration name=NomeDaMigration   # cria uma migration
make db-update                         # aplica as migrations
```

### Frontend (React)

SPA em **React + TypeScript (Vite)**, servida por **Nginx** que faz proxy de `/api`
para a API (mesma origem, sem CORS em producao). Quatro telas:

1. **Busca** — origem, destino, data; loading, vazio e erro.
2. **Selecao de assento** — mapa visual (livre / ocupado / selecionado; ocupado bloqueado).
3. **Dados do passageiro + confirmacao** — validacao no front (CPF com digito verificador,
   e-mail), resumo da compra e tela de sucesso com o codigo da reserva.
4. **Consulta de reserva** (bonus) — busca pelo codigo e cancelamento.

Rodar so o front em dev (proxy para a API em `http://localhost:8080`, ou defina
`VITE_API_PROXY_TARGET`):

```bash
make front       # vite dev em http://localhost:5173
```

### Sem Docker (desenvolvimento)

```bash
make install     # instala dependencias (backend + frontend)
make dev         # sobe backend + frontend juntos (1 comando, com feedback animado)
make back        # somente a API   (rodar separado)
make front       # somente o front (rodar separado)
```

### Preview da animacao

```bash
make demo        # mostra a animacao de boot sem subir nada (util para conferir o efeito)
```

> Rode `make help` para ver todos os comandos.
> Terminais sem cor/TTY (CI, logs redirecionados) ou `--plain` recebem saida em
> linhas simples automaticamente.

### Git hooks (padrao de qualidade)

Ao clonar o repositorio, ative os hooks de qualidade uma vez:

```bash
make setup-hooks
```

Isso ativa: `pre-commit` (format/lint), `commit-msg` (Conventional Commits) e
`post-commit` (atualiza a versao automatica — veja abaixo).

### Versao automatica

A versao e derivada do **git** e muda sozinha a cada commit — sem passo manual.
Combina uma base semantica estavel (arquivo `VERSION`) com a contagem de commits
e o hash curto:

```bash
make version          # ex.: v0.1.0 (#12 g1a2b3c4)
scripts/version.sh    # ex.: 0.1.0+build.12.g1a2b3c4   (--short, --commit, --count, --json)
```

- A cada `git commit`, o hook `post-commit` regenera o cache `.version` (usado em
  builds sem `.git`, ex.: dentro do container) e imprime o novo numero.
- Arvore com alteracoes nao commitadas aparece como `...-dirty`.
- O boot animado (`make up`/`make dev`) exibe a versao no titulo; a API e o
  frontend a exibirao (Fases 3-4) alimentados pela mesma fonte via build-arg.

## Testes

```bash
make test         # todos os testes (backend + frontend)
make test-back    # somente backend
make test-front   # somente frontend
```

- **Backend** — 115 testes (xUnit): unitarios (CPF, assento ocupado, cancelamento 2h,
  codigo unico) + integracao HTTP (`WebApplicationFactory` + SQLite in-memory).
- **Frontend** — testes de componente (Vitest + React Testing Library): busca
  (preenche + busca), mapa de assentos (selecao + bloqueio de ocupado) e validacao do
  formulario de passageiro; mais um teste de pagina com a API mockada.

## Estrutura do projeto

```
onibus-express/
  backend/
    src/OnibusExpress.Domain/          # entidades, VOs, regras (zero deps)
    src/OnibusExpress.Application/      # use cases, interfaces, DTOs
    src/OnibusExpress.Infrastructure/   # EF Core + PostgreSQL, repositorios, migrations
    src/OnibusExpress.Api/              # ASP.NET Core Minimal APIs, Swagger
    tests/OnibusExpress.Tests/          # xUnit (unit + integracao SQLite)
    Dockerfile
  frontend/
    src/components/                    # SeatMap, SearchForm, PassengerForm, ...
    src/pages/                         # as 4 telas
    src/services/                      # client da API (fetch)
    src/store/                         # estado (Zustand)
    src/lib/                           # validacao (CPF/e-mail) e formatacao
    Dockerfile  nginx.conf             # build + Nginx (serve o SPA + proxy /api)
  docs/                                # Documentacao e decisoes
  docker-compose.yml
  Makefile
```

Camadas seguem **Clean Architecture** (dependencias apontam para dentro):
`Api → Infrastructure → Application → Domain`.

## Decisoes de arquitetura

Ver [docs/DECISOES.md](docs/DECISOES.md).
