import Link from "next/link";

export default function HomePage() {
  return (
    <section className="mx-auto grid min-h-[calc(100vh-73px)] max-w-6xl place-items-center px-6 py-16">
      <div className="max-w-3xl text-center">
        <span className="inline-flex rounded-full bg-slate-900 px-3 py-1 text-xs font-semibold uppercase tracking-wider text-white">
          DEMO SERVICE BOOKING MANAGEMENT SYSTEM
        </span>
        <h2 className="mt-6 text-4xl font-bold tracking-tight text-slate-950 sm:text-6xl">
          Đặt lịch dịch vụ đơn giản, rõ ràng và dễ quản lý
        </h2>
        <p className="mx-auto mt-6 max-w-2xl text-lg leading-8 text-slate-600">
          Khách hàng xem dịch vụ, chọn nhân viên và khung giờ phù hợp. Admin quản lý dịch vụ, nhân viên,
          lịch làm việc và toàn bộ booking.
        </p>
        <div className="mt-8 flex flex-wrap justify-center gap-3">
          <Link
            href="/services"
            className="rounded-lg bg-slate-900 px-5 py-3 text-sm font-semibold text-white hover:bg-slate-700"
          >
            Xem dịch vụ
          </Link>
          <Link
            href="/login"
            className="rounded-lg border border-slate-300 bg-white px-5 py-3 text-sm font-semibold text-slate-900 hover:bg-slate-50"
          >
            Đăng nhập
          </Link>
        </div>
      </div>
    </section>
  );
}
