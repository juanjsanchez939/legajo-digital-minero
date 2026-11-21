import {
  ArrowRight,
  CheckCircle2,
  Database,
  HardHat,
  QrCode,
  Server,
  ShieldCheck,
  Smartphone,
  WifiOff,
} from 'lucide-react'
import { Link } from 'react-router-dom'

const stack = [
  {
    icon: Server,
    title: 'Backend .NET 10',
    detail: 'ASP.NET Core Web API + EF Core 10',
  },
  {
    icon: Database,
    title: 'PostgreSQL 16',
    detail: '7 entidades relacionales',
  },
  {
    icon: Smartphone,
    title: 'PWA instalable',
    detail: 'React 19 + Vite + TypeScript',
  },
  {
    icon: WifiOff,
    title: 'Modo offline',
    detail: 'Service Worker + IndexedDB',
  },
]

const dayOne = [
  'Repositorio limpio y estructura del monorepo',
  'API con /health, OpenAPI y Scalar',
  'Frontend con Vite, Tailwind y PWA base',
  'CI para backend y frontend',
]

export default function App() {
  return (
    <div className="flex min-h-screen flex-col bg-mining-50">
      <header className="bg-mining-900 text-white">
        <div className="mx-auto flex w-full max-w-5xl items-center justify-between gap-4 px-5 py-4">
          <div className="flex items-center gap-3">
            <span className="flex size-11 items-center justify-center rounded-2xl bg-white/10 ring-1 ring-white/20">
              <HardHat className="size-6 text-amber-300" aria-hidden />
            </span>
            <div>
              <p className="text-sm font-medium tracking-wide text-white/70">
                Legajo en el bolsillo
              </p>
              <h1 className="text-lg font-semibold leading-tight">
                Legajo Digital Minero
              </h1>
            </div>
          </div>
          <span className="hidden rounded-full bg-white/10 px-3 py-1 text-xs font-medium text-white/80 ring-1 ring-white/15 sm:inline">
            Día 1 de 15
          </span>
        </div>
      </header>

      <main className="mx-auto w-full max-w-5xl flex-1 px-5 py-8">
        <section className="rounded-3xl bg-white p-6 shadow-sm ring-1 ring-slate-900/5 sm:p-8">
          <span className="inline-flex items-center gap-2 rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700 ring-1 ring-emerald-200">
            <CheckCircle2 className="size-4" aria-hidden />
            Esqueleto funcionando
          </span>

          <h2 className="mt-4 max-w-2xl text-3xl font-bold tracking-tight text-mining-950 sm:text-4xl">
            Billetera digital de acreditación minera
          </h2>
          <p className="mt-3 max-w-2xl text-base leading-relaxed text-slate-600">
            Centralizá tus certificados, controlá vencimientos y mostrá tu
            aptitud con un QR dinámico, incluso sin señal en alta montaña.
          </p>

          <div className="mt-6 flex flex-wrap items-center gap-3">
            <Link
              to="/verificacion/demo"
              className="inline-flex items-center gap-2 rounded-xl bg-mining-900 px-4 py-2.5 text-sm font-semibold text-white transition hover:bg-mining-800"
            >
              Ver credencial de garita
              <ArrowRight className="size-4" aria-hidden />
            </Link>
            <a
              href="http://localhost:5235/scalar/v1"
              target="_blank"
              rel="noreferrer"
              className="inline-flex items-center gap-2 rounded-xl bg-white px-4 py-2.5 text-sm font-semibold text-mining-900 ring-1 ring-slate-200 transition hover:bg-mining-50"
            >
              <Server className="size-4" aria-hidden />
              Abrir API (Scalar)
            </a>
          </div>
        </section>

        <section className="mt-6 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          {stack.map((item) => (
            <article
              key={item.title}
              className="rounded-2xl bg-white p-4 shadow-sm ring-1 ring-slate-900/5"
            >
              <item.icon className="size-5 text-mining-700" aria-hidden />
              <h3 className="mt-3 text-sm font-semibold text-mining-950">
                {item.title}
              </h3>
              <p className="mt-1 text-xs leading-relaxed text-slate-500">
                {item.detail}
              </p>
            </article>
          ))}
        </section>

        <section className="mt-6 grid gap-3 lg:grid-cols-[1.4fr_1fr]">
          <article className="rounded-2xl bg-white p-5 shadow-sm ring-1 ring-slate-900/5">
            <div className="flex items-center gap-2">
              <ShieldCheck className="size-5 text-emerald-600" aria-hidden />
              <h3 className="text-sm font-semibold text-mining-950">
                Estado del Día 1
              </h3>
            </div>
            <ul className="mt-4 space-y-2.5">
              {dayOne.map((item) => (
                <li
                  key={item}
                  className="flex items-start gap-2 text-sm text-slate-600"
                >
                  <CheckCircle2
                    className="mt-0.5 size-4 shrink-0 text-emerald-600"
                    aria-hidden
                  />
                  {item}
                </li>
              ))}
            </ul>
          </article>

          <article className="rounded-2xl bg-mining-900 p-5 text-white shadow-sm">
            <QrCode className="size-6 text-amber-300" aria-hidden />
            <h3 className="mt-3 text-sm font-semibold">Próximo hito</h3>
            <p className="mt-1 text-sm leading-relaxed text-white/70">
              Día 2: las 7 entidades del DER, cifrado AES-256 de columnas
              sensibles, migración inicial y seeder.
            </p>
          </article>
        </section>
      </main>

      <footer className="border-t border-slate-200 bg-white">
        <div className="mx-auto flex w-full max-w-5xl flex-col gap-1 px-5 py-4 text-xs text-slate-500 sm:flex-row sm:items-center sm:justify-between">
          <span>Legajo en el bolsillo · San Juan, Argentina</span>
          <span>v0.1.0 · Día 1 de 15</span>
        </div>
      </footer>
    </div>
  )
}
