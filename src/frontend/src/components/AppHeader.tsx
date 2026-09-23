"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { clearStoredAuth, getStoredAuth, type AuthUser } from "@/lib/auth";

export default function AppHeader() {
  const [auth, setAuth] = useState<AuthUser | null>(null);

  useEffect(() => {
    const refresh = () => setAuth(getStoredAuth());

    refresh();
    window.addEventListener("auth-changed", refresh);
    window.addEventListener("storage", refresh);

    return () => {
      window.removeEventListener("auth-changed", refresh);
      window.removeEventListener("storage", refresh);
    };
  }, []);

  const logout = () => {
    clearStoredAuth();
    window.location.href = "/login";
  };

  return (
    <header className="border-b border-slate-200 bg-white">
      <div className="mx-auto flex max-w-6xl flex-wrap items-center justify-between gap-4 px-6 py-4">
        <Link href="/" className="text-lg font-semibold text-slate-900">
          Service Booking
        </Link>

        <nav className="flex flex-wrap items-center gap-5 text-sm font-medium text-slate-600">
          <Link className="hover:text-slate-950" href="/services">
            Dịch vụ
          </Link>

          {auth?.role === "Customer" && (
            <>
              <Link className="hover:text-slate-950" href="/booking">
                Đặt lịch
              </Link>
              <Link className="hover:text-slate-950" href="/my-bookings">
                Booking của tôi
              </Link>
            </>
          )}

          {auth?.role === "Admin" && (
            <>
              <Link className="hover:text-slate-950" href="/admin/services">
                Quản lý dịch vụ
              </Link>
              <Link className="hover:text-slate-950" href="/admin/schedules">
                Nhân viên & lịch
              </Link>
              <Link className="hover:text-slate-950" href="/admin/bookings">
                Booking
              </Link>
            </>
          )}

          {auth ? (
            <button className="text-left hover:text-slate-950" type="button" onClick={logout}>
              Đăng xuất
            </button>
          ) : (
            <Link className="hover:text-slate-950" href="/login">
              Đăng nhập
            </Link>
          )}
        </nav>
      </div>
    </header>
  );
}
