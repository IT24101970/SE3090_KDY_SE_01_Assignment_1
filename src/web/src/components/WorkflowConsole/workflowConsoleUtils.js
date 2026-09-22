export const statusLabels = { PausedForApproval: 'Paused for review', Running: 'Running', Completed: 'Completed', Terminated: 'Safe-failed' };
export const statusClass = (status) => ({ PausedForApproval: 'paused', Running: 'running', Completed: 'completed', Terminated: 'terminated' }[status] || 'running');
export const formatDate = (date) => new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(date));
