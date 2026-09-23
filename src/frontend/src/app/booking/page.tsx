"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { api, buildQuery, type PagedResult } from "@/lib/api";
import { authApi, getStoredAuth } from "@/lib/auth";

type ServiceItem = {
  id: string;
  name: string;
  durationMinutes: number;
  price: number;
};

type StaffItem = {
  id: string;
  fullName: string;
  email: string;
};

type AvailableSlot = {
  startTime: string;
  endTime: string;
};

type AvailableSlotsResponse = {
  slots: AvailableSlot[];
};

type BookingResponse = {
  id: string;
  bookingCode: string;
  status: string;
};

function tomorrowText() {
  const date = new Date();
  date.setDate(date.getDate() + 1);
  return date.toISOString().slice(0, 10);
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
    timeZone: "Asia/Ho_Chi_Minh",
  }).format(new Date(value));
}

export default function BookingPage() {
  const router = useRouter();
  const [services, setServices] = useState<ServiceItem[]>([]);
  const [staffs, setStaffs] = useState<StaffItem[]>([]);
  const [serviceId, setServiceId] = useState("");
  const [staffId, setStaffId] = useState("");
  const [date, setDate] = useState(tomorrowText());
  const [customerNote, setCustomerNote] = useState("");
  const [slots, setSlots] = useState<AvailableSlot[]>([]);
  const [selectedStartTime, setSelectedStartTime] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    const auth = getStoredAuth();
    if (!auth || auth.role !== "Customer") {
      setError("Bạn cần đăng nhập bằng tài khoản Customer để tạo booking.");
      return;
    }

    const params = new URLSearchParams(window.location.search);

    Promise.all([
      api<PagedResult<ServiceItem>>("/services?pageSize=100"),
      authApi<StaffItem[]>("/staffs"),
    ])
      .then(([serviceResult, staffResult]) => {
        setServices(serviceResult.items);
        setStaffs(staffResult);
        setServiceId(params.get("serviceId") ?? serviceResult.items[0]?.id ?? "");
        setStaffId(staffResult[0]?.id ?? "");
      })
      .catch((caughtError) => setError(caughtError instanceof Error ? caughtError.message : "Không thể tải dữ liệu."));
  }, []);

  const loadSlots = async (event?: FormEvent) => {
    event?.preventDefault();
    setError("");
    setMessage("");
    setSelectedStartTime("");
    setSlots([]);

    if (!serviceId || !staffId || !date) {
      setError("Hãy chọn đầy đủ dịch vụ, nhân viên và ngày.");
      return;
    }

    setIsLoading(true);
    try {
      const query = buildQuery({ serviceId, staffId, date });
      const result = await authApi<AvailableSlotsResponse>(`/bookings/available-slots${query}`);
      setSlots(result.slots);
      if (result.slots.length === 0) {
        setMessage("Không có khung giờ trống cho lựa chọn này.");
      }
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tải khung giờ.");
    } finally {
      setIsLoading(false);
    }
  };

  const createBooking = async () => {
    setError("");
    setMessage("");

    if (!selectedStartTime) {
      setError("Hãy chọn một khung giờ trống.");
      return;
    }

    setIsLoading(true);
    try {
      const booking = await authApi<BookingResponse>("/bookings", {
        method: "POST",
        body: JSON.stringify({ serviceId, staffId, startTime: selectedStartTime, customerNote }),
      });

      setMessage(`Tạo booking ${booking.bookingCode} thành công. Trạng thái: ${booking.status}.`);
      setSelectedStartTime("");
      await loadSlots();
      router.refresh();
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tạo booking.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <section className="mx-auto max-w-5xl px-6 py-12">
      <p className="text-sm font-medium text-slate-500">Customer</p>
      <h1 className="mt-1 text-3xl font-bold text-slate-950">Đặt lịch</h1>

      <form className="mt-8 grid gap-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm md:grid-cols-4" onSubmit={loadSlots}>
        <label className="block md:col-span-2">
          <span className="text-sm font-medium text-slate-700">Dịch vụ</span>
          <select className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5" value={serviceId} onChange={(event) => setServiceId(event.target.value)}>
            {services.map((service) => (
              <option key={service.id} value={service.id}>
                {service.name} - {service.durationMinutes} phút
              </option>
            ))}
          </select>
        </label>

        <label className="block md:col-span-2">
          <span className="text-sm font-medium text-slate-700">Nhân viên</span>
          <select className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5" value={staffId} onChange={(event) => setStaffId(event.target.value)}>
            {staffs.map((staff) => (
              <option key={staff.id} value={staff.id}>
                {staff.fullName}
              </option>
            ))}
          </select>
        </label>

        <label className="block">
          <span className="text-sm font-medium text-slate-700">Ngày</span>
          <input className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5" type="date" value={date} onChange={(event) => setDate(event.target.value)} />
        </label>

        <label className="block md:col-span-3">
          <span className="text-sm font-medium text-slate-700">Ghi chú</span>
          <input className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5" value={customerNote} onChange={(event) => setCustomerNote(event.target.value)} placeholder="Ví dụ: cần tư vấn thêm" />
        </label>

        <button className="rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 disabled:opacity-50 md:col-span-4" disabled={isLoading} type="submit">
          {isLoading ? "Đang xử lý..." : "Tìm khung giờ trống"}
        </button>
      </form>

      {error && <div className="mt-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}
      {message && <div className="mt-5 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{message}</div>}

      <div className="mt-8 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
        <h2 className="text-lg font-semibold text-slate-950">Khung giờ trống</h2>
        <div className="mt-4 grid gap-3 sm:grid-cols-2 md:grid-cols-4">
          {slots.map((slot) => (
            <button
              key={slot.startTime}
              type="button"
              onClick={() => setSelectedStartTime(slot.startTime)}
              className={`rounded-lg border px-4 py-3 text-sm font-semibold ${
                selectedStartTime === slot.startTime
                  ? "border-slate-900 bg-slate-900 text-white"
                  : "border-slate-200 bg-white text-slate-700 hover:border-slate-400"
              }`}
            >
              {formatTime(slot.startTime)} - {formatTime(slot.endTime)}
            </button>
          ))}
        </div>

        {slots.length > 0 && (
          <button className="mt-5 rounded-lg bg-emerald-600 px-5 py-2.5 text-sm font-semibold text-white hover:bg-emerald-700 disabled:opacity-50" disabled={isLoading} type="button" onClick={createBooking}>
            Tạo booking
          </button>
        )}
      </div>
    </section>
  );
}
