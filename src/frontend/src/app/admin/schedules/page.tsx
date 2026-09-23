"use client";

import { FormEvent, useEffect, useMemo, useState } from "react";
import { authApi, getStoredAuth } from "@/lib/auth";

type StaffItem = {
  id: string;
  fullName: string;
  email: string;
  isActive: boolean;
};

type StaffForm = {
  fullName: string;
  email: string;
  isActive: boolean;
};

type WorkSchedule = {
  id: string;
  staffId: string;
  workDate: string;
  startTime: string;
  endTime: string;
};

type CalendarDay = {
  dateKey: string;
  dayName: string;
  displayDate: string;
};

const emptyStaff: StaffForm = {
  fullName: "",
  email: "",
  isActive: true,
};

const STAFF_COLOR_CLASSES = [
  "border-emerald-100 bg-emerald-50 text-emerald-800",
  "border-sky-100 bg-sky-50 text-sky-800",
  "border-violet-100 bg-violet-50 text-violet-800",
  "border-amber-100 bg-amber-50 text-amber-800",
  "border-rose-100 bg-rose-50 text-rose-800",
  "border-cyan-100 bg-cyan-50 text-cyan-800",
];

function normalizeStaffForm(form: StaffForm): StaffForm {
  return {
    fullName: form.fullName.trim(),
    email: form.email.trim().toLowerCase(),
    isActive: form.isActive,
  };
}

function toDateKey(date: Date) {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");

  return `${year}-${month}-${day}`;
}

function addDays(date: Date, days: number) {
  const nextDate = new Date(date);
  nextDate.setDate(nextDate.getDate() + days);

  return nextDate;
}

function getStartOfWeek(date: Date) {
  const startOfWeek = new Date(date);
  const dayOfWeek = startOfWeek.getDay();
  const daysFromMonday = dayOfWeek === 0 ? 6 : dayOfWeek - 1;

  startOfWeek.setDate(startOfWeek.getDate() - daysFromMonday);
  startOfWeek.setHours(0, 0, 0, 0);

  return startOfWeek;
}

function getScheduleDateKey(workDate: string) {
  return workDate.slice(0, 10);
}

function formatTime(time: string) {
  return time.slice(0, 5);
}

function buildCalendarDays(startDate: Date, totalDays = 7): CalendarDay[] {
  const dayFormatter = new Intl.DateTimeFormat("vi-VN", { weekday: "long" });
  const dateFormatter = new Intl.DateTimeFormat("vi-VN", { day: "2-digit", month: "2-digit" });

  return Array.from({ length: totalDays }, (_, index) => {
    const date = addDays(startDate, index);

    return {
      dateKey: toDateKey(date),
      dayName: dayFormatter.format(date),
      displayDate: dateFormatter.format(date),
    };
  });
}

function formatWeekRange(startDate: Date) {
  const endDate = addDays(startDate, 6);
  const formatter = new Intl.DateTimeFormat("vi-VN", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  });

  return `${formatter.format(startDate)} - ${formatter.format(endDate)}`;
}

export default function AdminSchedulesPage() {
  const [staffs, setStaffs] = useState<StaffItem[]>([]);
  const [schedules, setSchedules] = useState<WorkSchedule[]>([]);
  const [allSchedules, setAllSchedules] = useState<WorkSchedule[]>([]);
  const [selectedStaffId, setSelectedStaffId] = useState("");
  const [calendarStaffId, setCalendarStaffId] = useState("all");
  const [calendarWeekStart, setCalendarWeekStart] = useState(() => getStartOfWeek(new Date()));
  const [editingStaffId, setEditingStaffId] = useState("");
  const [staffForm, setStaffForm] = useState<StaffForm>(emptyStaff);
  const [originalStaffForm, setOriginalStaffForm] = useState<StaffForm>(emptyStaff);
  const [scheduleForm, setScheduleForm] = useState({ workDate: "", startTime: "09:00", endTime: "17:00" });
  const [message, setMessage] = useState("");
  const [messageTone, setMessageTone] = useState<"success" | "info">("success");
  const [error, setError] = useState("");

  const normalizedStaffForm = normalizeStaffForm(staffForm);
  const normalizedOriginalStaffForm = normalizeStaffForm(originalStaffForm);
  const hasStaffChanges =
    !editingStaffId ||
    normalizedStaffForm.fullName !== normalizedOriginalStaffForm.fullName ||
    normalizedStaffForm.email !== normalizedOriginalStaffForm.email ||
    normalizedStaffForm.isActive !== normalizedOriginalStaffForm.isActive;

  const staffById = useMemo(() => new Map(staffs.map((staff) => [staff.id, staff])), [staffs]);
  const staffColorById = useMemo(() => {
    return new Map(staffs.map((staff, index) => [staff.id, STAFF_COLOR_CLASSES[index % STAFF_COLOR_CLASSES.length]]));
  }, [staffs]);
  const calendarDays = useMemo(() => buildCalendarDays(calendarWeekStart, 7), [calendarWeekStart]);
  const calendarWeekRange = useMemo(() => formatWeekRange(calendarWeekStart), [calendarWeekStart]);
  const todayKey = useMemo(() => toDateKey(new Date()), []);

  const calendarSchedules = useMemo(() => {
    return allSchedules
      .filter((schedule) => calendarStaffId === "all" || schedule.staffId === calendarStaffId)
      .sort((first, second) => {
        const dateCompare = getScheduleDateKey(first.workDate).localeCompare(getScheduleDateKey(second.workDate));

        if (dateCompare !== 0) {
          return dateCompare;
        }

        return first.startTime.localeCompare(second.startTime);
      });
  }, [allSchedules, calendarStaffId]);

  const loadSchedules = async (staffId: string) => {
    if (!staffId) {
      setSchedules([]);
      return;
    }

    const result = await authApi<WorkSchedule[]>(`/staffs/${staffId}/schedules`);
    setSchedules(result);
  };

  const loadAllSchedules = async (staffList: StaffItem[]) => {
    if (staffList.length === 0) {
      setAllSchedules([]);
      return;
    }

    const scheduleGroups = await Promise.all(
      staffList.map(async (staff) => {
        const result = await authApi<WorkSchedule[]>(`/staffs/${staff.id}/schedules`);
        return result.map((schedule) => ({ ...schedule, staffId: schedule.staffId || staff.id }));
      }),
    );

    setAllSchedules(scheduleGroups.flat());
  };

  const loadStaffs = async () => {
    const result = await authApi<StaffItem[]>("/staffs/admin");
    setStaffs(result);

    if (!selectedStaffId && result[0]) {
      setSelectedStaffId(result[0].id);
    }

    await loadAllSchedules(result);
  };

  useEffect(() => {
    const auth = getStoredAuth();

    if (!auth || auth.role !== "Admin") {
      setError("Bạn cần đăng nhập bằng tài khoản Admin để quản lý nhân viên và lịch làm việc.");
      return;
    }

    loadStaffs().catch((caughtError) =>
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tải nhân viên."),
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    loadSchedules(selectedStaffId).catch((caughtError) =>
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tải lịch làm việc."),
    );
  }, [selectedStaffId]);

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

  const saveStaff = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    setMessage("");

    if (editingStaffId && !hasStaffChanges) {
      setMessageTone("info");
      setMessage("Chưa có thay đổi nào để cập nhật.");
      return;
    }

    try {
      if (editingStaffId) {
        await authApi<StaffItem>(`/staffs/${editingStaffId}`, {
          method: "PUT",
          body: JSON.stringify(normalizedStaffForm),
        });
        setMessageTone("success");
        setMessage("Cập nhật nhân viên thành công.");
      } else {
        await authApi<StaffItem>("/staffs", {
          method: "POST",
          body: JSON.stringify(normalizedStaffForm),
        });
        setMessageTone("success");
        setMessage("Tạo nhân viên thành công.");
      }

      setEditingStaffId("");
      setStaffForm(emptyStaff);
      setOriginalStaffForm(emptyStaff);
      await loadStaffs();
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Không thể lưu nhân viên.");
    }
  };

  const editStaff = (staff: StaffItem) => {
    const nextStaffForm = {
      fullName: staff.fullName,
      email: staff.email,
      isActive: staff.isActive,
    };

    setEditingStaffId(staff.id);
    setStaffForm(nextStaffForm);
    setOriginalStaffForm(nextStaffForm);
    setMessage("");
    setError("");
  };

  const createSchedule = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError("");
    setMessage("");

    try {
      await authApi<WorkSchedule>(`/staffs/${selectedStaffId}/schedules`, {
        method: "POST",
        body: JSON.stringify(scheduleForm),
      });
      setMessageTone("success");
      setMessage("Tạo lịch làm việc thành công.");
      await loadSchedules(selectedStaffId);
      await loadAllSchedules(staffs);
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : "Không thể tạo lịch làm việc.");
    }
  };

  const moveCalendarWeek = (weekOffset: number) => {
    setCalendarWeekStart((currentWeekStart) => addDays(currentWeekStart, weekOffset * 7));
  };

  const resetCalendarToCurrentWeek = () => {
    setCalendarWeekStart(getStartOfWeek(new Date()));
  };

  const selectCalendarDay = (dateKey: string) => {
    setScheduleForm((currentForm) => ({ ...currentForm, workDate: dateKey }));
    setMessageTone("info");
    setMessage(`Đã chọn ngày ${dateKey} cho form tạo ca làm việc.`);
  };

  return (
    <section className="mx-auto max-w-7xl px-6 py-12">
      <p className="text-sm font-medium text-slate-500">Admin</p>
      <h1 className="mt-1 text-3xl font-bold text-slate-950">Nhân viên & lịch làm việc</h1>

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

      <div className="mt-8 grid gap-6 lg:grid-cols-[0.95fr_1.35fr]">
        <div>
          <form className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm" onSubmit={saveStaff}>
            <h2 className="text-lg font-semibold text-slate-950">{editingStaffId ? "Sửa nhân viên" : "Tạo nhân viên"}</h2>
            <label className="mt-4 block">
              <span className="text-sm font-medium text-slate-700">Họ tên</span>
              <input
                required
                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
                value={staffForm.fullName}
                onChange={(event) => setStaffForm({ ...staffForm, fullName: event.target.value })}
              />
            </label>
            <label className="mt-4 block">
              <span className="text-sm font-medium text-slate-700">Email</span>
              <input
                required
                type="email"
                className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
                value={staffForm.email}
                onChange={(event) => setStaffForm({ ...staffForm, email: event.target.value })}
              />
            </label>
            <label className="mt-4 flex items-center gap-2 text-sm font-medium text-slate-700">
              <input
                type="checkbox"
                checked={staffForm.isActive}
                onChange={(event) => setStaffForm({ ...staffForm, isActive: event.target.checked })}
              />
              Active
            </label>
            <button className="mt-5 w-full rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700" type="submit">
              {editingStaffId ? "Cập nhật nhân viên" : "Tạo nhân viên"}
            </button>
          </form>

          <div className="mt-5 space-y-3">
            {staffs.map((staff) => (
              <div
                key={staff.id}
                className={`rounded-xl border p-4 shadow-sm ${
                  selectedStaffId === staff.id ? "border-slate-900 bg-slate-50" : "border-slate-200 bg-white"
                }`}
              >
                <button className="w-full text-left" type="button" onClick={() => setSelectedStaffId(staff.id)}>
                  <div className="flex justify-between gap-3">
                    <div>
                      <p className="font-semibold text-slate-950">{staff.fullName}</p>
                      <p className="text-sm text-slate-500">{staff.email}</p>
                    </div>
                    <span
                      className={`h-fit rounded-full px-2.5 py-1 text-xs font-semibold ${
                        staff.isActive ? "bg-emerald-50 text-emerald-700" : "bg-slate-100 text-slate-500"
                      }`}
                    >
                      {staff.isActive ? "Active" : "Inactive"}
                    </span>
                  </div>
                </button>
                <button className="mt-3 text-sm font-semibold text-slate-700 hover:text-slate-950" type="button" onClick={() => editStaff(staff)}>
                  Sửa
                </button>
              </div>
            ))}
          </div>
        </div>

        <div className="space-y-6">
          <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <h2 className="text-lg font-semibold text-slate-950">Lịch làm việc</h2>
            <p className="mt-1 text-sm text-slate-500">
              Chọn nhân viên ở danh sách bên trái để tạo ca làm việc và xem lịch chi tiết của nhân viên đó.
            </p>

            <form className="mt-4 grid gap-4 md:grid-cols-3" onSubmit={createSchedule}>
              <label className="block">
                <span className="text-sm font-medium text-slate-700">Ngày</span>
                <input
                  required
                  type="date"
                  className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
                  value={scheduleForm.workDate}
                  onChange={(event) => setScheduleForm({ ...scheduleForm, workDate: event.target.value })}
                />
              </label>
              <label className="block">
                <span className="text-sm font-medium text-slate-700">Bắt đầu</span>
                <input
                  required
                  type="time"
                  className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
                  value={scheduleForm.startTime}
                  onChange={(event) => setScheduleForm({ ...scheduleForm, startTime: event.target.value })}
                />
              </label>
              <label className="block">
                <span className="text-sm font-medium text-slate-700">Kết thúc</span>
                <input
                  required
                  type="time"
                  className="mt-2 w-full rounded-lg border border-slate-300 px-3 py-2.5"
                  value={scheduleForm.endTime}
                  onChange={(event) => setScheduleForm({ ...scheduleForm, endTime: event.target.value })}
                />
              </label>
              <button className="rounded-lg bg-slate-900 px-4 py-2.5 text-sm font-semibold text-white hover:bg-slate-700 md:col-span-3" type="submit">
                Tạo ca làm việc
              </button>
            </form>

            <div className="mt-6 divide-y divide-slate-100">
              {schedules.map((schedule) => (
                <div key={schedule.id} className="flex items-center justify-between gap-4 py-3 text-sm">
                  <span className="font-medium text-slate-950">{getScheduleDateKey(schedule.workDate)}</span>
                  <span className="text-slate-600">
                    {formatTime(schedule.startTime)} - {formatTime(schedule.endTime)}
                  </span>
                </div>
              ))}
              {schedules.length === 0 && <div className="py-8 text-center text-sm text-slate-500">Chưa có lịch cho nhân viên này.</div>}
            </div>
          </div>

          <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
            <div className="flex flex-col gap-4 xl:flex-row xl:items-start xl:justify-between">
              <div>
                <h2 className="text-lg font-semibold text-slate-950">Lịch làm việc trực quan</h2>
                <p className="mt-1 text-sm text-slate-500">
                  Xem nhanh ca làm việc theo tuần. Click vào một ngày để điền nhanh ngày đó vào form tạo ca.
                </p>
              </div>
              <div className="flex flex-col gap-3 lg:flex-row lg:items-center">
                <select
                  className="min-w-48 rounded-lg border border-slate-300 bg-white px-3 py-2.5 text-sm"
                  value={calendarStaffId}
                  onChange={(event) => setCalendarStaffId(event.target.value)}
                >
                  <option value="all">Tất cả nhân viên</option>
                  {staffs.map((staff) => (
                    <option key={staff.id} value={staff.id}>
                      {staff.fullName}
                    </option>
                  ))}
                </select>
                <div className="grid grid-cols-3 rounded-lg border border-slate-300 bg-white p-1">
                  <button
                    className="whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-semibold text-slate-600 hover:bg-slate-100"
                    type="button"
                    onClick={() => moveCalendarWeek(-1)}
                  >
                    Tuần trước
                  </button>
                  <button
                    className="whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-semibold text-slate-600 hover:bg-slate-100"
                    type="button"
                    onClick={resetCalendarToCurrentWeek}
                  >
                    Tuần này
                  </button>
                  <button
                    className="whitespace-nowrap rounded-md px-3 py-1.5 text-sm font-semibold text-slate-600 hover:bg-slate-100"
                    type="button"
                    onClick={() => moveCalendarWeek(1)}
                  >
                    Tuần sau
                  </button>
                </div>
              </div>
            </div>

            <div className="mt-4 flex flex-wrap items-center justify-between gap-3 rounded-xl bg-slate-50 px-4 py-3">
              <p className="text-sm font-semibold text-slate-700">Tuần: {calendarWeekRange}</p>
              <p className="text-xs text-slate-500">Màu sắc giúp phân biệt ca làm việc của từng nhân viên.</p>
            </div>

            <div className="mt-5 overflow-x-auto pb-2">
              <div className="grid min-w-[1050px] grid-cols-7 gap-3">
                {calendarDays.map((day) => {
                  const daySchedules = calendarSchedules.filter((schedule) => getScheduleDateKey(schedule.workDate) === day.dateKey);
                  const isToday = day.dateKey === todayKey;

                  return (
                    <button
                      key={day.dateKey}
                      className={`min-h-48 rounded-xl border p-4 text-left transition hover:-translate-y-0.5 hover:shadow-md ${
                        scheduleForm.workDate === day.dateKey
                          ? "border-slate-900 bg-slate-100"
                          : isToday
                            ? "border-sky-200 bg-sky-50"
                            : "border-slate-200 bg-slate-50"
                      }`}
                      type="button"
                      onClick={() => selectCalendarDay(day.dateKey)}
                    >
                      <div className="flex items-start justify-between gap-3">
                        <div>
                          <p className="whitespace-nowrap capitalize text-sm font-semibold text-slate-950">{day.dayName}</p>
                          <p className="text-xs text-slate-500">{day.displayDate}</p>
                        </div>
                        <span className="whitespace-nowrap rounded-full bg-white px-2.5 py-1 text-xs font-semibold text-slate-500">
                          {daySchedules.length} ca
                        </span>
                      </div>

                      <div className="mt-4 space-y-2">
                        {daySchedules.map((schedule) => {
                          const staff = staffById.get(schedule.staffId);

                          return (
                            <div
                              key={schedule.id}
                              className={`rounded-lg border px-3 py-2 text-sm ${
                                staff?.isActive === false
                                  ? "border-slate-200 bg-white text-slate-500"
                                  : staffColorById.get(schedule.staffId) ?? "border-emerald-100 bg-emerald-50 text-emerald-800"
                              }`}
                            >
                              <p className="truncate font-semibold">{staff?.fullName ?? "Nhân viên không xác định"}</p>
                              <p className="mt-0.5 whitespace-nowrap text-xs">
                                {formatTime(schedule.startTime)} - {formatTime(schedule.endTime)}
                              </p>
                            </div>
                          );
                        })}

                        {daySchedules.length === 0 && (
                          <div className="rounded-lg border border-dashed border-slate-200 bg-white px-3 py-6 text-center text-sm text-slate-400">
                            Chưa có lịch
                          </div>
                        )}
                      </div>
                    </button>
                  );
                })}
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
