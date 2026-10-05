/**
 * AppointmentDirectoryTab.test.jsx
 *
 * Unit tests for Student 1 — Appointment Management
 * Tests: loading state, success render, status filtering, modal triggers,
 * status update, and cancellation flow.
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, cleanup } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import AppointmentDirectoryTab from '../components/PatientManagement/AppointmentDirectoryTab';

// ── Mock API ─────────────────────────────────────────────────────────────────
vi.mock('../api/channelCenterApi', () => ({
  appointmentApi: {
    list: vi.fn(),
    updateStatus: vi.fn(),
    cancel: vi.fn(),
  },
}));

vi.mock('../components/PatientManagement/SlotBookingModal', () => ({
  default: ({ isOpen, onClose }) =>
    isOpen ? <div data-testid="slot-booking-modal"><button onClick={onClose}>Close</button></div> : null,
}));
vi.mock('../components/PatientManagement/AppointmentCancelModal', () => ({
  default: ({ isOpen, onClose }) =>
    isOpen ? <div data-testid="cancel-modal"><button onClick={onClose}>Close</button></div> : null,
}));
vi.mock('../components/PatientManagement/PatientManagement.css', () => ({}));

// ── Sample data ───────────────────────────────────────────────────────────────
const MOCK_APPOINTMENTS = [
  {
    id: 101,
    patientId: 1,
    patientName: 'Alice Perera',
    doctorId: 10,
    doctorName: 'Dr. Nimal',
    appointmentDate: '2026-10-01T09:00:00',
    status: 'Pending',
    reason: 'Routine check',
    notes: '',
  },
  {
    id: 102,
    patientId: 2,
    patientName: 'Bob G.',
    doctorId: 11,
    doctorName: 'Dr. Saman',
    appointmentDate: '2026-10-02T11:00:00',
    status: 'Confirmed',
    reason: 'Follow-up',
    notes: 'Bring previous reports',
  },
];

import { appointmentApi } from '../api/channelCenterApi';

// ─────────────────────────────────────────────────────────────────────────────

describe('AppointmentDirectoryTab', () => {
  beforeEach(() => {
    appointmentApi.list.mockResolvedValue({ items: MOCK_APPOINTMENTS, totalCount: 2 });
    appointmentApi.updateStatus.mockResolvedValue({});
    appointmentApi.cancel.mockResolvedValue({});
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  // ── 1. Render states ──────────────────────────────────────────────────────

  it('shows page heading', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() =>
      expect(screen.getByText(/appointment management/i)).toBeInTheDocument()
    );
  });

  it('shows loading spinner initially', () => {
    appointmentApi.list.mockReturnValue(new Promise(() => {}));
    render(<AppointmentDirectoryTab />);
    expect(screen.getByText(/retrieving appointment records/i)).toBeInTheDocument();
  });

  it('renders appointment rows after load', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() => expect(screen.getByText('Alice Perera')).toBeInTheDocument());
    expect(screen.getByText('Bob G.')).toBeInTheDocument();
  });

  it('shows empty state when API returns no appointments', async () => {
    appointmentApi.list.mockResolvedValue({ items: [], totalCount: 0 });
    render(<AppointmentDirectoryTab />);
    await waitFor(() => expect(screen.getByText(/no appointments found/i)).toBeInTheDocument());
  });

  it('shows error message on API failure', async () => {
    appointmentApi.list.mockRejectedValue(new Error('Server down'));
    render(<AppointmentDirectoryTab />);
    await waitFor(() => expect(screen.getByText('Server down')).toBeInTheDocument());
  });

  // ── 2. Status filtering (pill buttons) ───────────────────────────────────

  it('re-fetches when status filter changes', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    // The component uses pill buttons (not a select) for status filtering
    const confirmedPill = screen.getByRole('button', { name: 'Confirmed' });
    await userEvent.click(confirmedPill);

    await waitFor(() =>
      expect(appointmentApi.list).toHaveBeenCalledWith(
        expect.objectContaining({ status: 'Confirmed' })
      )
    );
  });

  // ── 3. "Book New Appointment" modal ─────────────────────────────────────

  it('opens SlotBookingModal when New Booking is clicked', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    // The header button is labelled "+ Book New Appointment"
    await userEvent.click(screen.getByRole('button', { name: /book new appointment/i }));
    expect(screen.getByTestId('slot-booking-modal')).toBeInTheDocument();
  });

  // ── 4. Cancel modal ───────────────────────────────────────────────────────

  it('opens Cancel modal when Cancel button is clicked on a pending appointment', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    // Locate the Cancel button using its title attribute (avoids ambiguity with pill buttons)
    const cancelBtn = screen.getAllByTitle('Cancel booking with required reason')[0];
    await userEvent.click(cancelBtn);

    expect(screen.getByTestId('cancel-modal')).toBeInTheDocument();
  });

  // ── 5. Pagination ─────────────────────────────────────────────────────────

  it('Previous page button is disabled on first page', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const prevBtn = screen.getByRole('button', { name: /previous/i });
    expect(prevBtn).toBeDisabled();
  });

  it('Next page button is disabled when all records fit on one page', async () => {
    render(<AppointmentDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const nextBtn = screen.getByRole('button', { name: /next/i });
    expect(nextBtn).toBeDisabled();
  });
});
