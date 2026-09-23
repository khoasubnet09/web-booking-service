"use client";

import { useEffect, useState } from "react";
import { ApiError, type PagedResult } from "@/lib/api";
import { authApi, getStoredAuth } from "@/lib/auth";

type BookingItem = {
  id: string;
  bookingCode: string;
  serviceName: string;
  staffName: string;
  startTime: string;
  endTime: string;
  status: string;
  customerNote: string | null;
  cancellationReason: string | null;
};

const statusLabels: Record<string, string> = {
  Pending: "Chờ xác nhận",
  Confirmed: "Đã xác nhận",
  Completed: "Đã hoàn thành",
  Cancelled: "Đã hủy",
};

const statusClasses: Record<string, string> = {
  Pending: "bg-amber-50 text-amber-700 ring-amber-200",
  Confirmed: "bg-blue-50 text-blue-700 ring-blue-200",
  Completed: "bg-emerald-50 text-emerald-700 ring-emerald-200",
  Cancelled: "bg-slate-100 text-slate-600 ring-slate-200",
};

function formatDate(value: string) {
  return new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "2-digit",
    timeZone: "Asia/Ho_Chi_Minh",
  }).format(new Date(value));
}

function formatTime(value: string) {
  return new Intl.DateTimeFormat("vi-VN", {
    hour: "2-digit",
    minute: "2-digit",
    timeZone: "Asia/Ho_Chi_Minh",
  }).format(new Date(value));
}

function shortBookingCode(value: string) {
  if (value.length <= 24) {
    return value;
  }

  return `${value.slice(0, 16)}...${value.slice(-6)}`;
}

function getCancelError(error: unknown) {
  if (error instanceof ApiError) {
    if (error.code === "BOOKING_COMPLETED_CANNOT_BE_CANCELLED") {
      return "Không thể hủy booking đã hoàn thành.";
    }

    if (error.code === "BOOKING_ALREADY_STARTED") {
      return "Không thể hủy booking đã bắt đầu.";
    }

    if (error.code === "BOOKING_ALREADY_CANCELLED") {
      return "Booking này đã được hủy trước đó.";
    }

    if (error.code === "CANCELLATION_REASON_REQUIRED") {
      return "Bạn cần nhập lý do hủy booking.";
    }

    return error.message;
  }

  return error instanceof Error ? error.message : "Không thể hủy booking.";
}

export default function MyBookingsPage() {
  const [bookings, setBookings] = useState<BookingItem[]>([]);
  const [status, setStatus] = useState("");
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [cancellingId, setCancellingId] = useState("");

  const loadBookings = async (nextStatus = status) => {
    setIsLoading(true);
    setError("");

    try {
      const query = nextStatus ? `?status=${nextStatus}&pageSize=50` : "?pageSize=50";
      const result = await authApi<PagedResult<BookingItem>>(`/bookings/my-bookings${query}`);
      setBookings(result.items);
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tải booking.");
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    const auth = getStoredAuth();
    if (!auth || auth.role !== "Customer") {
      setError("Bạn cần đăng nhập bằng tài khoản Customer để xem booking của mình.");
      return;
    }

    loadBookings("");
    // eslint-disable-next-line react-hooks/exhaustive-deps
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

  const cancelBooking = async (bookingId: string) => {
    const cancellationReason = window.prompt("Nhập lý do hủy booking:")?.trim();
    if (!cancellationReason) {
      setError("Bạn cần nhập lý do hủy booking.");
      return;
    }

    setError("");
    setMessage("");
    setCancellingId(bookingId);

    try {
      await authApi<BookingItem>(`/bookings/${bookingId}/cancel`, {
        method: "POST",
        body: JSON.stringify({ cancellationReason }),
      });
      setMessage("Hủy booking thành công.");
      await loadBookings();
    } catch (caughtError) {
      setError(getCancelError(caughtError));
    } finally {
      setCancellingId("");
    }
  };

  return (
    <section className="mx-auto max-w-7xl px-6 py-12">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <p className="text-sm font-medium text-slate-500">Customer</p>
          <h1 className="mt-1 text-3xl font-bold text-slate-950">Booking của tôi</h1>
        </div>

        <select
          className="rounded-lg border border-slate-300 px-3 py-2.5 text-sm"
          value={status}
          onChange={(event) => {
            setStatus(event.target.value);
            loadBookings(event.target.value);
          }}
        >
          <option value="">Tất cả trạng thái</option>
          <option value="1">Pending</option>
          <option value="2">Confirmed</option>
          <option value="3">Completed</option>
          <option value="4">Cancelled</option>
        </select>
      </div>

      {error && <div className="mt-5 rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}
      {message && <div className="mt-5 rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm text-emerald-700">{message}</div>}

      <div className="mt-8 overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="min-w-[980px] w-full table-fixed text-left text-sm">
          <colgroup>
            <col className="w-[230px]" />
            <col className="w-[220px]" />
            <col className="w-[190px]" />
            <col className="w-[170px]" />
            <col className="w-[150px]" />
            <col className="w-[120px]" />
          </colgroup>
          <thead className="bg-slate-50 text-slate-600">
            <tr>
              <th className="px-4 py-3">Mã</th>
              <th className="px-4 py-3">Dịch vụ</th>
              <th className="px-4 py-3">Nhân viên</th>
              <th className="px-4 py-3">Thời gian</th>
              <th className="px-4 py-3">Trạng thái</th>
              <th className="px-4 py-3">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {bookings.map((booking) => (
              <tr key={booking.id} className="align-middle hover:bg-slate-50/60">
                <td className="px-4 py-3">
                  <span className="block truncate font-mono text-xs font-semibold text-slate-900" title={booking.bookingCode}>
                    {shortBookingCode(booking.bookingCode)}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className="block truncate" title={booking.serviceName}>
                    {booking.serviceName}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <span className="block truncate" title={booking.staffName}>
                    {booking.staffName}
                  </span>
                </td>
                <td className="px-4 py-3">
                  <div className="whitespace-nowrap">
                    <p className="font-medium text-slate-900">{formatDate(booking.startTime)}</p>
                    <p className="text-xs text-slate-500">
                      {formatTime(booking.startTime)} - {formatTime(booking.endTime)}
                    </p>
                  </div>
                </td>
                <td className="px-4 py-3">
                  <span
                    className={`inline-flex whitespace-nowrap rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ${
                      statusClasses[booking.status] ?? "bg-slate-100 text-slate-600 ring-slate-200"
                    }`}
                  >
                    {statusLabels[booking.status] ?? booking.status}
                  </span>
                </td>
                <td className="px-4 py-3">
                  {booking.status !== "Cancelled" && booking.status !== "Completed" && (
                    <button
                      className="h-8 w-20 rounded-md bg-red-50 text-xs font-semibold text-red-700 ring-1 ring-red-200 hover:bg-red-100 disabled:cursor-not-allowed disabled:opacity-50"
                      disabled={cancellingId === booking.id}
                      type="button"
                      onClick={() => cancelBooking(booking.id)}
                    >
                      Hủy
                    </button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>

        {!isLoading && bookings.length === 0 && (
          <div className="p-8 text-center text-sm text-slate-500">Chưa có booking phù hợp.</div>
        )}
        {isLoading && <div className="p-8 text-center text-sm text-slate-500">Đang tải...</div>}
      </div>
    </section>
  );
}
