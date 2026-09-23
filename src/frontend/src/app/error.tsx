"use client";

export default function ErrorPage({ reset }: { reset: () => void }) {
  return (
    <div className="mx-auto max-w-2xl px-6 py-16 text-center">
      <h2 className="text-2xl font-bold text-slate-950">Đã có lỗi xảy ra</h2>
      <p className="mt-2 text-sm text-slate-500">Không thể tải dữ liệu ở thời điểm hiện tại.</p>
      <button
        type="button"
        onClick={reset}
        className="mt-6 rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700"
      >
        Thử lại
      </button>
    </div>
  );
}
