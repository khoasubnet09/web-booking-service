"use client";

import { FormEvent, useState } from "react";
import { useRouter } from "next/navigation";
import { ApiError } from "@/lib/api";
import { login } from "@/lib/auth";

export default function LoginPage() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const user = await login(email.trim(), password);
      router.push(user.role === "Admin" ? "/admin/bookings" : "/services");
      router.refresh();
    } catch (caughtError) {
      if (caughtError instanceof ApiError && caughtError.status === 401) {
        setError("Email hoặc mật khẩu không đúng.");
      } else if (caughtError instanceof Error) {
        setError(caughtError.message);
      } else {
        setError("Không thể đăng nhập. Hãy kiểm tra backend đang chạy.");
      }
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <section className="mx-auto flex min-h-[calc(100vh-73px)] max-w-6xl items-center justify-center px-6 py-12">
      <div className="w-full max-w-md rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
        <div>
          <p className="text-sm font-medium text-slate-500">Service Booking</p>
          <h1 className="mt-2 text-2xl font-bold text-slate-950">Đăng nhập</h1>
          <p className="mt-2 text-sm text-slate-500">
            Dùng tài khoản seed: Admin hoặc Customer để vào đúng màn hình phân quyền.
          </p>
        </div>

        <form className="mt-8 space-y-5" onSubmit={submit}>
          <label className="block">
            <span className="text-sm font-medium text-slate-700">Email</span>
            <input
              type="email"
              name="email"
              autoComplete="off"
              required
              value={email}
              onChange={(event) => setEmail(event.target.value)}
              className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5 outline-none focus:border-slate-900"
              placeholder="Nhập email"
            />
          </label>

          <label className="block">
            <span className="text-sm font-medium text-slate-700">Mật khẩu</span>
            <input
              type="password"
              name="password"
              autoComplete="off"
              required
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5 outline-none focus:border-slate-900"
              placeholder="********"
            />
          </label>

          {error && (
            <div className="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
              {error}
            </div>
          )}

          <button
            type="submit"
            disabled={isSubmitting}
            className="w-full rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 disabled:cursor-not-allowed disabled:opacity-50"
          >
            {isSubmitting ? "Đang đăng nhập..." : "Đăng nhập"}
          </button>
        </form>

        <div className="mt-5 rounded-lg bg-slate-50 p-3 text-xs leading-5 text-slate-600">
          <p>Admin: admin@servicebooking.com / Admin@123</p>
          <p>Customer 1: customer1@servicebooking.com / Customer@123</p>
          <p>Customer 2: customer2@servicebooking.com / Customer@123</p>
        </div>
      </div>
    </section>
  );
}
