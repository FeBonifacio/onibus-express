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
docker compose up --build
# ou
make up
```

### Sem Docker (desenvolvimento)

```bash
make install     # instala dependencias (backend + frontend)
make dev         # sobe backend + frontend juntos
make back        # somente a API
make front       # somente o frontend
```

> Rode `make help` para ver todos os comandos.

### Git hooks (padrao de qualidade)

Ao clonar o repositorio, ative os hooks de qualidade uma vez:

```bash
make setup-hooks
```

## Testes

```bash
make test         # todos os testes (backend + frontend)
make test-back    # somente backend
make test-front   # somente frontend
```

## Estrutura do projeto

```
onibus-express/
  backend/      # API .NET (Clean Architecture)
  frontend/     # App React (Vite)
  docs/         # Documentacao e decisoes
  docker-compose.yml
  Makefile
```

## Decisoes de arquitetura

Ver [docs/DECISOES.md](docs/DECISOES.md).
