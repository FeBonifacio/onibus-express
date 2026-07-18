# 🤖 Guia de uso de IA no OniBus Express

Como colaborar com assistentes de IA (Claude Code, Copilot, Cursor, etc.) **neste
repositório** mantendo o padrao de qualidade — cobrindo **backend (.NET)** e
**frontend (React)**. Este projeto foi construido em conjunto com IA; as regras abaixo
sao o que mantem o codigo consistente.

> TL;DR: peca sempre **codigo em ingles + feedback em portugues**, com **testes** e
> **build/lint/format verdes**, respeitando a **Clean Architecture** no back e as
> convencoes do front. Commits em **Conventional Commits** (ingles).

---

## 1. Regras de ouro (contexto que a IA SEMPRE precisa)

Cole isto (ou aponte para este arquivo) ao iniciar uma sessao com a IA:

1. **Idioma:** todo o **codigo** (tipos, membros, variaveis, arquivos, comentarios,
   nomes de teste) em **ingles**. Apenas **feedback ao usuario** em **portugues**
   (mensagens do terminal e mensagens de erro que chegam ao usuario via API/UI).
2. **Documento do passageiro = `Document`** (nunca `Cpf`). Ha um `DocumentType`
   (hoje so `Cpf`, validado por digito verificador) para escalar a outros tipos.
3. **Backend = Clean Architecture:** dependencias apontam para dentro
   (`Api → Infrastructure → Application → Domain`). **Regras de negocio vivem no
   Domain** (ex.: `Trip.Reserve`, `Reservation.Cancel`), nunca nos controllers/use cases.
4. **Frontend:** React + TypeScript **estrito**; a SPA fala com **uma unica origem**
   via `/api` (proxy do Vite em dev, Nginx em prod). Validacao do front **espelha** o
   backend (nao substitui).
5. **Qualidade e obrigatoria:** toda tarefa termina com **build + testes + lint/format
   verdes**. Testes acompanham o codigo.
6. **Commits:** Conventional Commits em ingles (o hook `commit-msg` bloqueia fora do
   padrao). **Nunca** commitar segredos.

Referencias vivas: [`DECISOES.md`](DECISOES.md) (o porque de cada decisao) e o
[`README.md`](../README.md) (como rodar).

---

## 2. Como pedir uma tarefa (prompt eficaz)

Um bom pedido para a IA neste repo tem:

- **Onde** (camada/tela) e **o que** (objetivo claro).
- **"Com testes"** e **"deixe build/test/format verdes"**.
- **"Codigo em ingles, feedback em portugues"** (se a IA nao tiver o contexto).

Exemplos:

> **Backend:** "Adicione um endpoint `GET /viagens/{id}/assentos` que retorna so os
> assentos livres. Coloque a logica no dominio/use case (nao no endpoint), com teste
> unitario e de integracao. Rode `make test-back` e `dotnet format` antes de terminar.
> Codigo em ingles, mensagens em portugues."

> **Frontend:** "Na tela de busca, adicione um filtro de faixa de preco. Reaproveite os
> componentes existentes, mantenha estados loading/vazio/erro, e adicione um teste RTL.
> Rode `npm run build`, `npm run lint` e `npm test`."

Evite: pedir "so o codigo, sem teste" ou aceitar mudanca sem rodar build/test.

---

## 3. Fluxo que a IA deve seguir

### Backend (.NET)
1. Implementar na **camada certa** (VO/entidade/regra no `Domain`; orquestracao no
   `Application`; EF/repositorio no `Infrastructure`; endpoint fino no `Api`).
2. **Testes:** unitarios (dominio) + integracao (`WebApplicationFactory` + SQLite).
3. `make test-back` → verde.
4. `dotnet format` (o `pre-commit` roda `--verify-no-changes`).
5. Mudou o schema? Gere migration: `make migration name=Xyz`.

### Frontend (React)
1. Implementar reaproveitando `components/`, `services/api`, `store/`, `lib/`.
2. **Teste RTL** por comportamento (nao detalhe de implementacao); mock da API com
   `vi.mock` (ver `pages/*.test.tsx`).
3. `npm run build` (tsc estrito) → `npm run lint` (oxlint) → `npm test` → verde.

### Verificacao real (quando fizer sentido)
Nao confie so no compilador: **rode o fluxo**. Ex.: subir Postgres + API e exercitar os
endpoints; ou o *seam* SPA → `/api` → API. Foi assim que 3 bugs reais foram pegos (ver
`DECISOES.md`, Fase 3).

### Commit
So com autorizacao do humano; mensagem em **ingles** (Conventional Commits). O
`post-commit` atualiza a versao automatica.

---

## 4. Comandos uteis (para voce e para a IA)

```bash
# Testes (nao precisam de Docker/Postgres — backend usa SQLite, front usa jsdom)
make test          # backend + frontend
make test-back     # so backend      make test-front   # so frontend

# Rodar o app
make up            # tudo via Docker (use DB_PORT=5433 make up se ja tiver Postgres local)
make dev           # backend + frontend locais           make back / make front

# Banco / EF Core
make migration name=NomeDaMigration      make db-update

# Qualidade / versao
make lint          make format          make version
```

---

## 5. Pontos de atencao especificos deste sistema

### Backend
- **As 5 regras de negocio** vivem no dominio: assento ocupado e viagem realizada em
  `Trip.Reserve`; janela de 2h em `Reservation.Cancel` (fronteira **estrita**); CPF por
  digito verificador no VO `Document`; codigo `ABC-12345` unico (retry + indice unico).
- **Chaves `Guid` sao `ValueGeneratedNever`** — o dominio gera o `Guid` nas factories;
  sem isso o EF tenta UPDATE em vez de INSERT.
- **Erros:** excecoes com `ErrorCode` (via `IHasErrorCode`) → `ErrorHttpMap` →
  **ProblemDetails (RFC 7807)**. Codes em ingles, **mensagens em portugues**.
- **Portabilidade Postgres/SQLite:** cuidado com o que so funciona em um provider
  (ex.: `ORDER BY DateTimeOffset` nao roda no SQLite — ordene no cliente).
- **Concorrencia:** indices unicos no banco + traducao da violacao no `EfUnitOfWork`
  (Postgres 23505 **e** SQLite) para 409.
- **Value Objects** sao `sealed class` com igualdade por valor (sem `default` que fure
  invariante). Nao acople o `Domain` a pacotes externos.

### Frontend
- **Estados loading / vazio / erro** em toda tela que chama a API.
- **Acessibilidade:** inputs com `label`; assentos sao `button` com `aria-label`
  ("Assento 5, livre/ocupado/selecionado") e ocupados ficam `disabled`.
- **Datas em `dd/mm/aaaa`** (campo com mascara — o `input type=date` nativo segue a
  locale do navegador); converta para ISO na borda da API.
- **CPF** com mascara + validacao por digito verificador; envie **so digitos** para a API.
- **Reservas lembradas** em `localStorage` (`lib/myReservations.ts`) e auto-carregadas
  na tela "Minhas reservas".
- Nunca chame a API por origem cheia hardcoded — use a base `/api`.

---

## 6. Como a IA foi usada na construcao (padrao recomendado)

Para tarefas grandes, o fluxo que deu certo aqui:

1. **Design panel** — varios designs independentes → sintese → critico endurece o spec.
2. **Fan-out de implementacao/testes** — agentes em paralelo a partir de um contrato exato.
3. **Review adversarial por dimensao** — cada achado e **refutado por um cetico** antes
   de virar fix (reduz falso-positivo).
4. **Verificacao end-to-end** contra a coisa real (Postgres, seam full-stack).

Voce nao precisa desse aparato para mudancas pequenas — mas para features/refactors
grandes ele elevou a qualidade e pegou bugs que testes sozinhos nao pegariam.

---

## 7. Checklist antes de commitar (com ou sem IA)

- [ ] `make test` verde (backend + frontend)
- [ ] Backend: `dotnet format` sem mudancas pendentes  ·  Frontend: `npm run lint` limpo
- [ ] Feature exercitada de verdade quando aplicavel (nao so compilou)
- [ ] Codigo em ingles, feedback em portugues; `Document` (nao `cpf`)
- [ ] Mensagem de commit em ingles, Conventional Commits
- [ ] Sem segredos/credenciais no diff

> Os git hooks (`make setup-hooks`) ja aplicam parte disso automaticamente:
> `pre-commit` (format/lint) e `commit-msg` (Conventional Commits).
