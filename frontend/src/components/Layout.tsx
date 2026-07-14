import { Link, Outlet } from 'react-router-dom'

export function Layout() {
  return (
    <>
      <header className="app-header">
        <div className="app-header__inner">
          <Link to="/" className="brand">
            <span className="brand__badge" aria-hidden="true">
              🚌
            </span>{' '}
            OniBus Express
          </Link>
          <nav className="app-nav">
            <Link to="/consulta">Minha reserva</Link>
          </nav>
        </div>
      </header>
      <main className="app-main">
        <Outlet />
      </main>
      <footer className="app-footer">OniBus Express — MVP de venda de passagens rodoviarias.</footer>
    </>
  )
}
