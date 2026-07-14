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
acompanha o progresso (docker → banco → backend → frontend) com uma animacao do
onibus; ao final, os logs sao seguidos automaticamente.

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
