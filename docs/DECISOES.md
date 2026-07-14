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

### Fase 2 — Backend: Domain + Application

**Contexto:** modelar o nucleo de negocio (entidades, regras) em Clean Architecture,
com Domain sem dependencias externas e Application so sobre Domain + abstracoes.
As decisoes abaixo saibram de um *design panel* (3 designs independentes) sintetizado
e endurecido por um revisor adversarial.

> **Idioma do codigo:** todo o codigo (tipos, membros, arquivos, comentarios, nomes de teste)
> e em **ingles**; apenas os feedbacks ao usuario (mensagens do terminal e mensagens de erro
> que chegam ao usuario via API/frontend) ficam em portugues. Por isso as entidades sao
> `Route`/`Trip`/`Passenger`/`Reservation`, etc.

- **`Trip` como Aggregate Root** — possui as `Reservation`s e concentra as invariantes 1
  (assento ocupado) e 2 (viagem realizada) em `Trip.Reserve(...)`. Regras nao vazam para os
  use cases (sem `if` de negocio espalhado).
- **`Document` em vez de `Cpf`** — o documento do passageiro e modelado como VO `Document`
  com `DocumentType` (hoje so `Cpf`, validado por digito verificador modulo 11). A nomenclatura
  ja nasce generica para escalar a outros tipos de documento sem tocar nos call sites.
  Parsing *lenient* mas **so digitos ASCII** (`'0'..'9'`), evitando digitos Unicode que
  corromperiam o calculo do DV.
- **Assento sem entidade propria** — VO `SeatNumber` (>= 1); ocupacao **derivada** das
  reservas ativas (fonte unica de verdade, elimina "ocupado mas cancelado"). O teto e checado
  pela `Trip`, que conhece o total.
- **`ReservationCode` (VO) gera forma + aleatoriedade; unicidade e da Application** — o VO
  recebe a fonte de aleatoriedade como `Func<int,int>` (Domain **nao** conhece RNG nem
  persistencia). Unicidade garantida por retry (5x) contra `IReservationRepository.CodeExistsAsync`.
- **VOs como sealed class** (`SeatNumber`, `ReservationCode`, `Document`, `Email`) — em vez de
  `record struct`, para nao existir um `default` que burle as invariantes (achado do review).
- **`IClock` (UTC) definido no Domain** — `SystemClock` (Application) e `FakeClock` (testes).
  Fronteira das 2h **estrita** (`>`); viagem realizada no **instante exato** da partida (`<=`).
- **Erros de regra por excecoes de dominio** — cada uma herda `DomainException` com um
  `ErrorCode` estavel em ingles (`SEAT_TAKEN`, `DOCUMENT_INVALID`…); `ErrorHttpMap` traduz para
  HTTP (contrato do handler global da Fase 3). As **mensagens** das excecoes ficam em portugues.
- **Application: 1 use case por operacao** (`ExecuteAsync`), sem MediatR; DTOs `record`
  imutaveis (nenhum tipo de Domain vaza para fora); mapeamento manual; repositorios por DIP.
- **Snapshots na `Reservation`** (`DepartureUtc`, `Price`) — permitem consultar/cancelar a
  reserva sem carregar a `Trip`, e justificam um `IReservationRepository` de leitura/cancelamento.

**Riscos e trade-offs assumidos:**
- **TOCTOU de assento e de codigo** — o check em memoria nao serializa dois POSTs concorrentes.
  Mitigacao na Fase 3: indices unicos no PostgreSQL (`(TripId, Seat) WHERE Status='Active'` e
  `Reservation.Code`) + traducao da violacao em 409, com retry do codigo (`Reservation.RegenerateCode`).
- **Cancelamento persiste via `IReservationRepository`, nao pela raiz** — o review apontou a
  "fronteira de agregado" (a `Reservation`, filha de `Trip`, e escrita por dois repositorios).
  **Decisao: manter**, pois `DELETE /reservas/{codigo}` so tem o codigo; graças ao snapshot
  `DepartureUtc`, cancelar sem carregar a `Trip` inteira e mais simples e direto. Na Fase 3 (EF)
  ambos os repositorios mapeiam o mesmo `DbSet<Reservation>` — e a mesma linha, nao escrita dupla.
- **`decimal Price` sem VO `Money`** — aceitavel no MVP; VO de dinheiro fica como melhoria.

**Qualidade:** um *design panel* (3 designs → sintese → critico) definiu a modelagem, e um
*review adversarial* (5 dimensoes, cada achado refutado por um cetico) pegou 9 defeitos reais
aplicados aqui (VOs `default`, `char.IsDigit` no CPF, `Email.GetHashCode`, testes vacuos).

**Testes:** cobrem os 4 exigidos (CPF/documento, assento ocupado, cancelamento 2h, codigo unico)
e edge-cases (fronteiras de tempo, ordem de checagem, colisao **real** de codigo + retry, reuso
de assento apos cancelamento, parsing lenient), com `FakeClock`, repositorios in-memory
compartilhados e builders.

### Fase 3 — Backend: Infrastructure + API

**Contexto:** tornar o backend executavel — persistencia relacional (EF Core + PostgreSQL),
os 6 endpoints, Swagger, tratamento de erros e Docker com um comando.

- **Minimal APIs (em vez de Controllers) — *por que*:** para um MVP com 6 endpoints finos que
  apenas orquestram use cases, Minimal APIs entregam o mesmo resultado com **muito menos
  boilerplate** (sem classes de controller, atributos, nem herança), o roteamento fica
  declarativo e agrupado (`MapGroup`), e a injecao de dependencia por parametro deixa cada
  endpoint com uma unica responsabilidade. Os endpoints ficam organizados em extensoes por
  recurso (`RouteEndpoints`/`TripEndpoints`/`ReservationEndpoints`), mantendo o `Program.cs` enxuto.
  Como toda a regra vive no dominio/use cases, os controllers "gordos" nao agregam nada aqui.
- **Tabelas relacionais com FKs** — `Routes`, `Trips` (FK → `Routes`), `Passengers`, `Reservations`
  (FK → `Trips` e → `Passengers`). VOs mapeados por *value converters* (`SeatNumber`, `Email`,
  `ReservationCode`) e `Document` como *owned type* (colunas `Document`/`DocumentType`).
- **Paths dos endpoints em portugues** (`/rotas`, `/viagens`, `/reservas`) — sao contrato externo
  exigido pelo enunciado; o codigo permanece em ingles. Corpo/JSON em ingles (campo `document`).
- **Concorrencia protegida pelo banco** — indice unico parcial `(TripId, Seat) WHERE Status='Active'`
  e unico em `Reservation.Code`. O `EfUnitOfWork` traduz a violacao (SQLSTATE 23505) pela
  `ConstraintName` em `SeatTakenException` (409) / `ReservationCodeDuplicateException` (409),
  fechando os dois TOCTOU deixados em aberto na Fase 2.
- **Chaves `Guid` `ValueGeneratedNever`** — o dominio gera o `Guid` nas factories; sem isso o EF
  trata as entidades novas do agregado como "existentes" e tenta UPDATE (0 linhas) em vez de INSERT.
- **Erros → ProblemDetails (RFC 7807)** — um `IExceptionHandler` global usa `IHasErrorCode` +
  `ErrorHttpMap` para traduzir `ErrorCode` em status; corpo `{ code, message, traceId }`. Mensagens
  em portugues (feedback), `code` em ingles. 5xx nao vaza stack/mensagem interna.
- **Connection string** — `ConnectionStrings:Default`, sobrescrita no Docker pelo override padrao
  `ConnectionStrings__Default` (evita o appsettings vencer o env dentro do container).
- **Migrations + seed automaticos no startup** — `MigrateAsync` (Npgsql) ou `EnsureCreated`
  (SQLite/testes); `DbSeeder` idempotente com datas relativas ao `IClock` (viagens sempre futuras).
- **Docker** — `Dockerfile` multi-stage e `docker-compose.yml` (api + postgres, healthcheck,
  `depends_on: service_healthy`), com a versao automatica no build-arg (exposta em `GET /version`).
- **Portabilidade Postgres/SQLite** — busca ordena por `DepartureUtc` no cliente (SQLite nao faz
  `ORDER BY` de `DateTimeOffset`), permitindo testes de integracao com **SQLite in-memory**.

**Qualidade:** a verificacao end-to-end contra um **PostgreSQL real** (13/13 no fluxo completo)
pegou 3 bugs que testes unitarios nao pegariam: precedencia da connection string, geracao de chave
EF (UPDATE vs INSERT) e o `ORDER BY` de `DateTimeOffset` no SQLite. Alem de **12 testes de
integracao** (`WebApplicationFactory` + SQLite) e um *review adversarial* por dimensao.

---

## O que ficou de fora (e por que) — a preencher

_A ser detalhado nas fases finais._

## Melhorias futuras (com mais tempo) — a preencher

_A ser detalhado nas fases finais._
