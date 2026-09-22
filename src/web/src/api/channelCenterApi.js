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
    headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}), ...options.headers },
  });
  if (!response.ok) {
    const body = await response.json().catch(() => ({}));
    const error = new Error(body.message || `Request failed with status ${response.status}`);
    error.status = response.status;
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
};
