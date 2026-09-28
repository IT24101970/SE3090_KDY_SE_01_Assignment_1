export const statusLabels = {
  Running: 'Running',
  PausedForApproval: 'Paused for review',
  Completed: 'Completed',
  Terminated: 'Terminated',
  SafeFailed: 'Safe-failed',
  0: 'Running',
  1: 'Paused for review',
  2: 'Completed',
  3: 'Terminated',
  4: 'Safe-failed'
};

export const statusClass = (status) =>
  ({
    Running: 'running',
    PausedForApproval: 'paused',
    Completed: 'completed',
    Terminated: 'terminated',
    SafeFailed: 'terminated',
    0: 'running',
    1: 'paused',
    2: 'completed',
    3: 'terminated',
    4: 'terminated'
  }[status] || 'running');

export const formatDate = (date) =>
  new Intl.DateTimeFormat('en', { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' }).format(new Date(date));

