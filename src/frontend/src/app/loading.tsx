export default function Loading() {
  return (
    <div className="mx-auto max-w-6xl px-6 py-12">
      <div className="h-7 w-48 animate-pulse rounded bg-slate-200" />
      <div className="mt-6 grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {[1, 2, 3].map((item) => (
          <div key={item} className="h-40 animate-pulse rounded-xl border border-slate-200 bg-white" />
        ))}
      </div>
    </div>
  );
}
