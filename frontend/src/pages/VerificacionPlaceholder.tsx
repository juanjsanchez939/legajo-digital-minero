import { ArrowLeft, HardHat, ShieldAlert } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'

export default function VerificacionPlaceholder() {
  const { token } = useParams<{ token: string }>()

  return (
    <div className="flex min-h-screen flex-col bg-mining-900 text-white">
      <header className="border-b border-white/10">
        <div className="mx-auto flex w-full max-w-3xl items-center justify-between px-5 py-4">
          <div className="flex items-center gap-3">
            <span className="flex size-10 items-center justify-center rounded-xl bg-white/10 ring-1 ring-white/20">
              <HardHat className="size-5 text-amber-300" aria-hidden />
            </span>
            <div>
              <p className="text-xs font-medium tracking-wide text-white/60">
                Verificación pública
              </p>
              <h1 className="text-base font-semibold">Garita minera</h1>
            </div>
          </div>
          <Link
            to="/"
            className="inline-flex items-center gap-1.5 rounded-lg bg-white/10 px-3 py-1.5 text-xs font-medium text-white/80 ring-1 ring-white/15 transition hover:bg-white/15"
          >
            <ArrowLeft className="size-3.5" aria-hidden />
            Volver
          </Link>
        </div>
      </header>

      <main className="mx-auto flex w-full max-w-3xl flex-1 items-center px-5 py-10">
        <div className="w-full rounded-3xl bg-white p-6 text-slate-900 shadow-xl sm:p-8">
          <span className="inline-flex items-center gap-2 rounded-full bg-amber-50 px-3 py-1 text-xs font-semibold text-amber-700 ring-1 ring-amber-200">
            <ShieldAlert className="size-4" aria-hidden />
            Pendiente para el Día 7
          </span>
          <h2 className="mt-4 text-2xl font-bold tracking-tight text-mining-950">
            Credencial de verificación
          </h2>
          <p className="mt-2 text-sm leading-relaxed text-slate-600">
            Esta ruta va a mostrar el semáforo de aptitud, los datos
            enmascarados del trabajador y el estado de cada documento, sin
            exponer los archivos originales.
          </p>
          <dl className="mt-6 rounded-xl bg-mining-50 p-4 text-sm">
            <dt className="text-xs font-medium uppercase tracking-wide text-slate-500">
              Token recibido
            </dt>
            <dd className="mt-1 break-all font-mono text-mining-900">
              {token ?? 'sin token'}
            </dd>
          </dl>
        </div>
      </main>
    </div>
  )
}
