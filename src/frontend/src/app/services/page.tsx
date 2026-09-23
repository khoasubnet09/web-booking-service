"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { api, type PagedResult } from "@/lib/api";

type ServiceItem = {
  id: string;
  name: string;
  description: string | null;
  durationMinutes: number;
  price: number;
  isActive: boolean;
};

export default function ServicesPage() {
  const [services, setServices] = useState<ServiceItem[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    api<PagedResult<ServiceItem>>("/services?pageSize=50")
      .then((result) => setServices(result.items))
      .catch(() => setServices([]))
      .finally(() => setIsLoading(false));
  }, []);

  return (
    <section className="mx-auto max-w-6xl px-6 py-12">
      <div className="flex items-end justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-slate-500">Danh mục</p>
          <h1 className="mt-1 text-3xl font-bold text-slate-950">Dịch vụ</h1>
        </div>
      </div>

      {isLoading ? (
        <div className="mt-8 rounded-xl border border-slate-200 bg-white p-10 text-center text-sm text-slate-500">
          Đang tải dịch vụ...
        </div>
      ) : services.length === 0 ? (
        <div className="mt-8 rounded-xl border border-dashed border-slate-300 bg-white p-10 text-center text-sm text-slate-500">
          Chưa có dịch vụ active hoặc backend chưa chạy.
        </div>
      ) : (
        <div className="mt-8 grid gap-4 md:grid-cols-2 lg:grid-cols-3">
          {services.map((service) => (
            <article key={service.id} className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
              <h2 className="text-lg font-semibold text-slate-950">{service.name}</h2>
              <p className="mt-2 min-h-10 text-sm text-slate-500">{service.description ?? "Không có mô tả"}</p>
              <div className="mt-5 flex items-center justify-between text-sm">
                <span className="font-medium text-slate-600">{service.durationMinutes} phút</span>
                <span className="font-semibold text-slate-950">
                  {new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(service.price)}
                </span>
              </div>
              <Link
                href={`/booking?serviceId=${service.id}`}
                className="mt-5 inline-flex w-full justify-center rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700"
              >
                Đặt lịch
              </Link>
            </article>
          ))}
        </div>
      )}
    </section>
  );
}
