/**
 * PatientDirectoryTab.test.jsx
 *
 * Unit tests for Student 1 — Patient Management
 * Tests cover: rendering states, search/filter, pagination, modal triggers,
 * delete confirmation, and the "AI Intake" hand-off.
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, cleanup } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import PatientDirectoryTab from '../components/PatientManagement/PatientDirectoryTab';

// ── Mock API ────────────────────────────────────────────────────────────────
vi.mock('../api/channelCenterApi', () => ({
  patientApi: {
    list: vi.fn(),
    delete: vi.fn(),
  },
}));

// ── Mock child modals so they don't need their own API calls ─────────────────
vi.mock('../components/PatientManagement/PatientFormModal', () => ({
  default: ({ isOpen, onClose }) =>
    isOpen ? <div data-testid="patient-form-modal"><button onClick={onClose}>Close</button></div> : null,
}));
vi.mock('../components/PatientManagement/PatientDetailModal', () => ({
  default: ({ isOpen, onClose }) =>
    isOpen ? <div data-testid="patient-detail-modal"><button onClick={onClose}>Close</button></div> : null,
}));
vi.mock('../components/PatientManagement/SlotBookingModal', () => ({
  default: ({ isOpen, onClose }) =>
    isOpen ? <div data-testid="slot-booking-modal"><button onClick={onClose}>Close</button></div> : null,
}));

// ── Silence CSS import inside the component ──────────────────────────────────
vi.mock('../components/PatientManagement/PatientManagement.css', () => ({}));

// ── Sample data ──────────────────────────────────────────────────────────────
const MOCK_PATIENTS = [
  {
    id: 1,
    name: 'Alice Perera',
    age: 32,
    nic: '982345678V',
    phoneNumber: '0771234567',
    email: 'alice@test.com',
    gender: 'Female',
    bloodGroup: 'O+',
    emergencyContact: '0771234000',
    totalAppointments: 3,
  },
  {
    id: 2,
    name: 'Bob Gunawardena',
    age: 45,
    nic: '791234567V',
    phoneNumber: '0779876543',
    email: 'bob@test.com',
    gender: 'Male',
    bloodGroup: 'A-',
    emergencyContact: '0779870000',
    totalAppointments: 0,
  },
];

import { patientApi } from '../api/channelCenterApi';

// ─────────────────────────────────────────────────────────────────────────────

describe('PatientDirectoryTab', () => {
  beforeEach(() => {
    patientApi.list.mockResolvedValue({ items: MOCK_PATIENTS, totalCount: 2 });
    patientApi.delete.mockResolvedValue({});
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  // ── 1. Initial render ─────────────────────────────────────────────────────

  it('renders the page heading', async () => {
    render(<PatientDirectoryTab />);
    expect(screen.getByText('Patient Directory & Onboarding')).toBeInTheDocument();
  });

  it('shows loading spinner on first mount', () => {
    // api.list is pending — spinner must appear
    patientApi.list.mockReturnValue(new Promise(() => {}));
    render(<PatientDirectoryTab />);
    expect(screen.getByText(/loading patient records/i)).toBeInTheDocument();
  });

  it('renders patient rows after successful API load', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => expect(screen.getByText('Alice Perera')).toBeInTheDocument());
    expect(screen.getByText('Bob Gunawardena')).toBeInTheDocument();
  });

  it('shows KPI total count from API response', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));
    // The "Total Registered" stat card should show totalCount = 2
    const statLabel = screen.getByText('Total Registered');
    const statCard = statLabel.closest('.pm-stat-card');
    expect(statCard).toHaveTextContent('2');
  });

  // ── 2. Empty & error states ───────────────────────────────────────────────

  it('shows empty state when no patients returned', async () => {
    patientApi.list.mockResolvedValue({ items: [], totalCount: 0 });
    render(<PatientDirectoryTab />);
    await waitFor(() => expect(screen.getByText('No Patients Found')).toBeInTheDocument());
  });

  it('shows error message on API failure', async () => {
    patientApi.list.mockRejectedValue(new Error('Network error'));
    render(<PatientDirectoryTab />);
    await waitFor(() => expect(screen.getByText('Network error')).toBeInTheDocument());
  });

  it('shows Try Again button on error', async () => {
    patientApi.list.mockRejectedValue(new Error('fail'));
    render(<PatientDirectoryTab />);
    await waitFor(() => expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument());
  });

  // ── 3. Filter / Search controls ──────────────────────────────────────────

  it('Re-fetches when search term changes', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const searchInput = screen.getByPlaceholderText(/search by name/i);
    await userEvent.type(searchInput, 'Bob');

    await waitFor(() => expect(patientApi.list).toHaveBeenCalledWith(
      expect.objectContaining({ searchTerm: 'Bob' })
    ));
  });

  it('shows Reset Filters button when a filter is active', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const searchInput = screen.getByPlaceholderText(/search by name/i);
    await userEvent.type(searchInput, 'Alice');

    expect(screen.getByRole('button', { name: /reset filters/i })).toBeInTheDocument();
  });

  it('clears filters when Reset Filters is clicked', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    await userEvent.type(screen.getByPlaceholderText(/search by name/i), 'Alice');
    await userEvent.click(screen.getByRole('button', { name: /reset filters/i }));

    expect(screen.queryByRole('button', { name: /reset filters/i })).not.toBeInTheDocument();
  });

  // ── 4. Modal interactions ─────────────────────────────────────────────────

  it('opens PatientFormModal when Register New Patient is clicked', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    await userEvent.click(screen.getByRole('button', { name: /register new patient/i }));
    expect(screen.getByTestId('patient-form-modal')).toBeInTheDocument();
  });

  it('opens PatientDetailModal when View is clicked on a row', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const viewButtons = screen.getAllByRole('button', { name: /view/i });
    await userEvent.click(viewButtons[0]);
    expect(screen.getByTestId('patient-detail-modal')).toBeInTheDocument();
  });

  it('opens PatientFormModal for editing when Edit is clicked', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const editButtons = screen.getAllByRole('button', { name: /edit/i });
    await userEvent.click(editButtons[0]);
    expect(screen.getByTestId('patient-form-modal')).toBeInTheDocument();
  });

  it('opens SlotBookingModal when Book is clicked', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const bookButtons = screen.getAllByRole('button', { name: /book/i });
    await userEvent.click(bookButtons[0]);
    expect(screen.getByTestId('slot-booking-modal')).toBeInTheDocument();
  });

  // ── 5. AI Intake hand-off ────────────────────────────────────────────────

  it('renders Intake button when onLaunchIntake prop is provided', async () => {
    const onLaunchIntake = vi.fn();
    render(<PatientDirectoryTab onLaunchIntake={onLaunchIntake} />);
    await waitFor(() => screen.getByText('Alice Perera'));

    expect(screen.getAllByRole('button', { name: /intake/i })).toHaveLength(2);
  });

  it('calls onLaunchIntake with the correct patient when Intake is clicked', async () => {
    const onLaunchIntake = vi.fn();
    render(<PatientDirectoryTab onLaunchIntake={onLaunchIntake} />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const intakeButtons = screen.getAllByRole('button', { name: /intake/i });
    await userEvent.click(intakeButtons[0]);

    expect(onLaunchIntake).toHaveBeenCalledWith(MOCK_PATIENTS[0]);
  });

  it('does not render Intake button when onLaunchIntake prop is absent', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));
    expect(screen.queryByRole('button', { name: /intake/i })).not.toBeInTheDocument();
  });

  // ── 6. Delete flow ────────────────────────────────────────────────────────

  it('calls delete API and re-fetches after confirmation', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const deleteButtons = screen.getAllByRole('button', { name: '✕' });
    await userEvent.click(deleteButtons[0]);

    await waitFor(() => expect(patientApi.delete).toHaveBeenCalledWith(1));
    expect(patientApi.list).toHaveBeenCalledTimes(2); // initial + after delete
  });

  it('does not call delete API when user cancels confirmation', async () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const deleteButtons = screen.getAllByRole('button', { name: '✕' });
    await userEvent.click(deleteButtons[0]);

    expect(patientApi.delete).not.toHaveBeenCalled();
  });

  // ── 7. Pagination ────────────────────────────────────────────────────────

  it('Next button is disabled when on the last page', async () => {
    // totalCount == 2, pageSize == 10 → only 1 page
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const nextBtn = screen.getByRole('button', { name: /next/i });
    expect(nextBtn).toBeDisabled();
  });

  it('Previous button is disabled on the first page', async () => {
    render(<PatientDirectoryTab />);
    await waitFor(() => screen.getByText('Alice Perera'));

    const prevBtn = screen.getByRole('button', { name: /previous/i });
    expect(prevBtn).toBeDisabled();
  });
});
