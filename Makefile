# OniBus Express - Makefile
# Orquestracao de desenvolvimento e Docker.
# Uso: `make help` para listar os alvos disponiveis.

.DEFAULT_GOAL := help
BACKEND_DIR   := backend
FRONTEND_DIR  := frontend
API_PROJECT   := backend/src/OnibusExpress.Api
# Auto-detecta o Docker Compose: plugin v2 (`docker compose`) ou standalone (`docker-compose`).
COMPOSE       := $(shell docker compose version >/dev/null 2>&1 && echo "docker compose" || echo "docker-compose")

.PHONY: help install setup-hooks dev back front demo test test-back test-front lint format up down logs clean

help: ## Lista os comandos disponiveis
	@grep -E '^[a-zA-Z_-]+:.*?## .*$$' $(MAKEFILE_LIST) | awk 'BEGIN {FS = ":.*?## "}; {printf "  \033[36m%-14s\033[0m %s\n", $$1, $$2}'

# ---- Setup ----
install: ## Instala dependencias (backend restore + frontend npm install)
	@if [ -d "$(BACKEND_DIR)" ]; then dotnet restore $(BACKEND_DIR); fi
	@if [ -f "$(FRONTEND_DIR)/package.json" ]; then cd $(FRONTEND_DIR) && npm install; fi

setup-hooks: ## Ativa os git hooks versionados (.githooks)
	git config core.hooksPath .githooks
	chmod +x .githooks/*
	@echo "Git hooks ativados (core.hooksPath=.githooks)"

# ---- Desenvolvimento (sem Docker) ----
dev: ## Roda backend + frontend juntos (1 comando) com feedback animado
	@bash scripts/dev.sh

back: ## Roda somente a API (.NET) em modo watch
	@echo "🚌 backend → http://localhost:5000 (Ctrl-C para sair)"
	cd $(API_PROJECT) && dotnet watch run

front: ## Roda somente o frontend (Vite) em modo dev
	@echo "🚌 frontend → http://localhost:5173 (Ctrl-C para sair)"
	cd $(FRONTEND_DIR) && npm run dev

demo: ## Mostra a animacao do boot (preview, sem subir nada)
	@bash scripts/up.sh --demo

# ---- Testes / Qualidade ----
test: test-back test-front ## Roda todos os testes

test-back: ## Testes do backend (.NET)
	dotnet test $(BACKEND_DIR)

test-front: ## Testes do frontend (Vitest)
	cd $(FRONTEND_DIR) && npm test

lint: ## Verifica formatacao/lint (nao altera arquivos)
	@if [ -d "$(BACKEND_DIR)" ]; then dotnet format $(BACKEND_DIR) --verify-no-changes; fi
	@if [ -f "$(FRONTEND_DIR)/package.json" ]; then cd $(FRONTEND_DIR) && npm run lint; fi

format: ## Corrige formatacao/lint automaticamente
	@if [ -d "$(BACKEND_DIR)" ]; then dotnet format $(BACKEND_DIR); fi
	@if [ -f "$(FRONTEND_DIR)/package.json" ]; then cd $(FRONTEND_DIR) && npm run lint -- --fix; fi

# ---- Docker (um comando sobe tudo) ----
up: ## Sobe todo o ambiente (API + banco + frontend) com feedback animado
	@bash scripts/up.sh

down: ## Derruba os containers
	$(COMPOSE) down

logs: ## Acompanha os logs dos containers
	$(COMPOSE) logs -f

clean: ## Remove artefatos de build (.NET bin/obj)
	find . -type d -name bin -prune -exec rm -rf {} + 2>/dev/null || true
	find . -type d -name obj -prune -exec rm -rf {} + 2>/dev/null || true
