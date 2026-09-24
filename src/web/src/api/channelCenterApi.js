const API_BASE = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5066';

const workflowStatuses = ['Running', 'PausedForApproval', 'Completed', 'Terminated'];
const approvalDecisions = ['Approved', 'Rejected', 'Revised'];

export function enumName(value, values) {
  if (typeof value === 'number') return values[value] || String(value);
  if (typeof value === 'string' && values.includes(value)) return value;
  return value;
}

export function normalizeWorkflow(workflow) {
  return {
    ...workflow,
    status: enumName(workflow.status, workflowStatuses),
    auditLogs: (workflow.auditLogs || workflow.AuditLogs || []).map((log) => ({ ...log })),
  };
}

export async function apiRequest(path, options = {}) {
  const token = localStorage.getItem('channel-center-token');
  const response = await fetch(`${API_BASE}${path}`, {
    ...options,
    headers: {
      'Content-Type': 'application/json',
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...options.headers,
    },
  });

  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    let message = body.message;
    if (!message && body.errors) {
      message = Object.values(body.errors).flat().join(' ');
    }
    if (!message) {
      if (response.status === 401) message = 'Session expired or unauthorized. Please sign in.';
      else if (response.status === 403) message = 'You do not have permission to perform this action.';
      else if (response.status === 404) message = 'The requested resource was not found.';
      else if (response.status === 409) message = 'A conflicting record or booking already exists.';
      else message = `Request failed with status ${response.status}`;
    }
    const error = new Error(message);
    error.status = response.status;
    error.data = body;
    throw error;
  }
  return response.status === 204 ? null : response.json();
}

export const workflowApi = {
  list: async (status) => (await apiRequest(`/api/admin/workflows${status ? `?status=${status}` : ''}`)).map(normalizeWorkflow),
  detail: async (id) => normalizeWorkflow(await apiRequest(`/api/admin/workflows/${id}`)),
  decide: async (id, decision) => {
    const response = await apiRequest(`/api/admin/workflows/${id}/approve`, { method: 'POST', body: JSON.stringify({ decision: approvalDecisions.indexOf(decision) }) });
    return { ...response, updatedWorkflowStatus: enumName(response.updatedWorkflowStatus, workflowStatuses) };
  },
  overview: () => apiRequest('/api/admin/analytics/overview'),
  aiMetrics: () => apiRequest('/api/admin/analytics/ai-metrics'),
  audit: () => apiRequest('/api/admin/audit-logs'),
};

export const authApi = {
  devLogin: (adminUserId) => apiRequest('/api/auth/dev-login', {
    method: 'POST',
    body: JSON.stringify(adminUserId),
  }),
  login: (dto) => apiRequest('/api/auth/login', {
    method: 'POST',
    body: JSON.stringify(dto),
  }),
  register: (dto) => apiRequest('/api/auth/register', {
    method: 'POST',
    body: JSON.stringify(dto),
  }),
  me: () => apiRequest('/api/auth/me'),
};

export const patientApi = {
  list: (filter = {}) => {
    const params = new URLSearchParams();
    if (filter.searchTerm) params.append('searchTerm', filter.searchTerm);
    if (filter.gender) params.append('gender', filter.gender);
    if (filter.bloodGroup) params.append('bloodGroup', filter.bloodGroup);
    if (filter.page) params.append('page', String(filter.page));
    if (filter.pageSize) params.append('pageSize', String(filter.pageSize));
    const qs = params.toString();
    return apiRequest(`/api/patients${qs ? `?${qs}` : ''}`);
  },
  getById: (id) => apiRequest(`/api/patients/${id}`),
  getByUserId: (userId) => apiRequest(`/api/patients/user/${userId}`),
  create: (dto) => apiRequest('/api/patients', {
    method: 'POST',
    body: JSON.stringify(dto),
  }),
  update: (id, dto) => apiRequest(`/api/patients/${id}`, {
    method: 'PUT',
    body: JSON.stringify(dto),
  }),
  delete: (id) => apiRequest(`/api/patients/${id}`, {
    method: 'DELETE',
  }),
};

export const appointmentApi = {
  list: (filter = {}) => {
    const params = new URLSearchParams();
    if (filter.patientId) params.append('patientId', String(filter.patientId));
    if (filter.doctorId) params.append('doctorId', String(filter.doctorId));
    if (filter.scheduleId) params.append('scheduleId', String(filter.scheduleId));
    if (filter.status) params.append('status', filter.status);
    if (filter.startDate) params.append('startDate', filter.startDate);
    if (filter.endDate) params.append('endDate', filter.endDate);
    if (filter.upcomingOnly !== undefined) params.append('upcomingOnly', String(filter.upcomingOnly));
    if (filter.page) params.append('page', String(filter.page));
    if (filter.pageSize) params.append('pageSize', String(filter.pageSize));
    const qs = params.toString();
    return apiRequest(`/api/appointments${qs ? `?${qs}` : ''}`);
  },
  getById: (id) => apiRequest(`/api/appointments/${id}`),
  getSlots: (filter = {}) => {
    const params = new URLSearchParams();
    if (filter.doctorId) params.append('doctorId', String(filter.doctorId));
    if (filter.specialtyId) params.append('specialtyId', String(filter.specialtyId));
    if (filter.date) params.append('date', filter.date);
    const qs = params.toString();
    return apiRequest(`/api/appointments/slots${qs ? `?${qs}` : ''}`);
  },
  getPatientHistory: (patientId, filter = {}) => {
    const params = new URLSearchParams();
    if (filter.page) params.append('page', String(filter.page));
    if (filter.pageSize) params.append('pageSize', String(filter.pageSize));
    const qs = params.toString();
    return apiRequest(`/api/appointments/patient/${patientId}/history${qs ? `?${qs}` : ''}`);
  },
  create: (dto) => apiRequest('/api/appointments', {
    method: 'POST',
    body: JSON.stringify(dto),
  }),
  updateStatus: (id, dto) => apiRequest(`/api/appointments/${id}/status`, {
    method: 'PATCH',
    body: JSON.stringify(dto),
  }),
  cancel: (id, cancelReason) => apiRequest(`/api/appointments/${id}/cancel`, {
    method: 'POST',
    body: JSON.stringify({ cancelReason }),
  }),
};

export const intakeAgentApi = {
  process: (dto) => apiRequest('/api/intake-agent/process', {
    method: 'POST',
    body: JSON.stringify(dto),
  }),
  getLogsByPatient: (patientId) => apiRequest(`/api/intake-agent/logs/${patientId}`),
  getAllLogs: (page = 1, pageSize = 20) => apiRequest(`/api/intake-agent/logs?page=${page}&pageSize=${pageSize}`),
  validateEligibility: (patientId) => apiRequest('/api/intake-agent/tools/validate-eligibility', {
    method: 'POST',
    body: JSON.stringify({ patientId }),
  }),
  formatSummary: (rawText) => apiRequest('/api/intake-agent/tools/format-summary', {
    method: 'POST',
    body: JSON.stringify({ rawText }),
  }),
};

export const doctorApi = {
  list: () => apiRequest('/api/doctor-scheduling/doctors'),
};

export const specialtiesApi = {
  list: () => apiRequest('/api/triage/specialties'),
};

