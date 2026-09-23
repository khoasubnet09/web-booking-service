"use client";

import { FormEvent, useEffect, useState } from "react";
import { type PagedResult } from "@/lib/api";
import { authApi, getStoredAuth } from "@/lib/auth";

type ServiceItem = {
  id: string;
  name: string;
  description: string | null;
  durationMinutes: number;
  price: number;
  isActive: boolean;
};

type ServiceForm = {
  name: string;
  description: string;
  durationMinutes: string;
  price: string;
  isActive: boolean;
};

type NormalizedServiceForm = {
  name: string;
  description: string;
  durationMinutes: number;
  price: number;
  isActive: boolean;
};

const emptyForm: ServiceForm = {
  name: "",
  description: "",
  durationMinutes: "30",
  price: "100000",
  isActive: true,
};

function normalizeServiceForm(form: ServiceForm): NormalizedServiceForm {
  return {
    name: form.name.trim(),
    description: form.description.trim(),
    durationMinutes: Number(form.durationMinutes),
    price: Number(form.price),
    isActive: form.isActive,
  };
}

export default function AdminServicesPage() {
  const [services, setServices] = useState<ServiceItem[]>([]);
  const [editingId, setEditingId] = useState("");
  const [form, setForm] = useState<ServiceForm>(emptyForm);
  const [originalForm, setOriginalForm] = useState<ServiceForm>(emptyForm);
  const [message, setMessage] = useState("");
  const [messageTone, setMessageTone] = useState<"success" | "info">("success");
  const [error, setError] = useState("");

  const normalizedForm = normalizeServiceForm(form);
  const normalizedOriginalForm = normalizeServiceForm(originalForm);
  const hasServiceChanges =
    !editingId ||
    normalizedForm.name !== normalizedOriginalForm.name ||
    normalizedForm.description !== normalizedOriginalForm.description ||
    normalizedForm.durationMinutes !== normalizedOriginalForm.durationMinutes ||
    normalizedForm.price !== normalizedOriginalForm.price ||
    normalizedForm.isActive !== normalizedOriginalForm.isActive;

  const loadServices = async () => {
    const result = await authApi<PagedResult<ServiceItem>>("/services/admin?pageSize=100");
    setServices(result.items);
  };

  useEffect(() => {
    const auth = getStoredAuth();
    if (!auth || auth.role !== "Admin") {
      setError("Bạn cần đăng nhập bằng tài khoản Admin để quản lý dịch vụ.");
      return;
    }

    loadServices().catch((caughtError) =>
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tải dịch vụ."),
    );
  }, []);

  useEffect(() => {
    if (!message && !error) {
      return;
    }

    const timeoutId = window.setTimeout(() => {
      setMessage("");
      setError("");
    }, 3500);

    return () => window.clearTimeout(timeoutId);
  }, [message, error]);

  const submit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    setMessage("");

    if (!Number.isFinite(normalizedForm.durationMinutes) || normalizedForm.durationMinutes <= 0) {
      setError("Thời lượng phải lớn hơn 0.");
      return;
    }

    if (!Number.isFinite(normalizedForm.price) || normalizedForm.price < 0) {
      setError("Giá phải lớn hơn hoặc bằng 0.");
      return;
    }

    if (editingId && !hasServiceChanges) {
      setMessageTone("info");
      setMessage("Chưa có thay đổi nào để cập nhật.");
      return;
    }

    const payload = {
      ...normalizedForm,
      description: normalizedForm.description || null,
    };

    try {
      if (editingId) {
        await authApi<ServiceItem>(`/services/${editingId}`, {
          method: "PUT",
          body: JSON.stringify(payload),
        });
        setMessageTone("success");
        setMessage("Cập nhật dịch vụ thành công.");
      } else {
        await authApi<ServiceItem>("/services", {
          method: "POST",
          body: JSON.stringify(payload),
        });
        setMessageTone("success");
        setMessage("Tạo dịch vụ thành công.");
      }

      setEditingId("");
      setForm(emptyForm);
      setOriginalForm(emptyForm);
      await loadServices();
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Không thể lưu dịch vụ.");
    }
  };

  const edit = (service: ServiceItem) => {
    const nextForm = {
      name: service.name,
      description: service.description ?? "",
      durationMinutes: String(service.durationMinutes),
      price: String(service.price),
      isActive: service.isActive,
    };

    setEditingId(service.id);
    setForm(nextForm);
    setOriginalForm(nextForm);
    setMessage("");
    setError("");
  };

  return (
    <section className="mx-auto max-w-6xl px-6 py-12">
      <p className="text-sm font-medium text-slate-500">Admin</p>
      <h1 className="mt-1 text-3xl font-bold text-slate-950">Quản lý dịch vụ</h1>

      <form className="mt-8 grid gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm md:grid-cols-4" onSubmit={submit}>
        <label className="block md:col-span-2">
          <span className="text-sm font-medium text-slate-700">Tên dịch vụ</span>
          <input
            required
            className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
            value={form.name}
            onChange={(event) => setForm({ ...form, name: event.target.value })}
          />
        </label>

        <label className="block">
          <span className="text-sm font-medium text-slate-700">Thời lượng</span>
          <input
            required
            min={1}
            type="number"
            inputMode="numeric"
            className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
            value={form.durationMinutes}
            onChange={(event) => setForm({ ...form, durationMinutes: event.target.value })}
          />
        </label>

        <label className="block">
          <span className="text-sm font-medium text-slate-700">Giá</span>
          <input
            required
            min={0}
            type="number"
            inputMode="numeric"
            className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
            value={form.price}
            onChange={(event) => setForm({ ...form, price: event.target.value })}
          />
        </label>

        <label className="block md:col-span-3">
          <span className="text-sm font-medium text-slate-700">Mô tả</span>
          <input
            className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
            value={form.description}
            onChange={(event) => setForm({ ...form, description: event.target.value })}
          />
        </label>

        <label className="flex items-center gap-2 pt-8 text-sm font-medium text-slate-700">
          <input type="checkbox" checked={form.isActive} onChange={(event) => setForm({ ...form, isActive: event.target.checked })} />
          Active
        </label>

        <button className="rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 md:col-span-4" type="submit">
          {editingId ? "Cập nhật dịch vụ" : "Tạo dịch vụ"}
        </button>
      </form>

      {error && <div className="mt-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}
      {message && (
        <div
          className={`mt-5 rounded-lg border px-4 py-3 text-sm ${
            messageTone === "success"
              ? "border-emerald-200 bg-emerald-50 text-emerald-700"
              : "border-sky-200 bg-sky-50 text-sky-700"
          }`}
        >
          {message}
        </div>
      )}

      <div className="mt-8 grid gap-4 md:grid-cols-2 lg:grid-cols-3">
        {services.map((service) => (
          <article key={service.id} className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="flex items-start justify-between gap-3">
              <h2 className="text-lg font-semibold text-slate-950">{service.name}</h2>
              <span className={`rounded-full px-2 py-1 text-xs font-semibold ${service.isActive ? "bg-emerald-50 text-emerald-700" : "bg-slate-100 text-slate-500"}`}>
                {service.isActive ? "Active" : "Inactive"}
              </span>
            </div>
            <p className="mt-2 text-sm text-slate-500">{service.description ?? "Không có mô tả"}</p>
            <p className="mt-4 text-sm text-slate-600">{service.durationMinutes} phút</p>
            <button className="mt-4 rounded-lg border border-slate-300 px-3 py-2 text-sm font-semibold hover:bg-slate-50" type="button" onClick={() => edit(service)}>
              Sửa
            </button>
          </article>
        ))}
      </div>
    </section>
  );
}
