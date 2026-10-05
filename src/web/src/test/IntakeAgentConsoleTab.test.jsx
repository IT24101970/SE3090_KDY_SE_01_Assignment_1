/**
 * IntakeAgentConsoleTab.test.jsx
 *
 * Unit tests for Student 1 — AI Intake & Intent Structuring Agent Console
 * Tests: initial render, patient pre-population from prop, validation guards,
 * successful processing flow, error handling, and log display.
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, cleanup } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import IntakeAgentConsoleTab from '../components/PatientManagement/IntakeAgentConsoleTab';

// ── Mock APIs ─────────────────────────────────────────────────────────────────
vi.mock('../api/channelCenterApi', () => ({
  patientApi: {
    list: vi.fn(),
  },
  intakeAgentApi: {
    process: vi.fn(),
    getLogsByPatient: vi.fn(),
  },
}));

// IntakeResultCard mock: renders the mock card AND a "Proceed to Booking" button
// that calls onProceedToBooking — matching the real component's prop signature.
vi.mock('../components/PatientManagement/IntakeResultCard', () => ({
  default: ({ result, onProceedToBooking }) => (
    <div data-testid="intake-result-card">
      <span>{result?.summary ?? 'result'}</span>
      {onProceedToBooking && (
        <button onClick={onProceedToBooking}>📅 Proceed to Booking</button>
      )}
    </div>
  ),
}));
vi.mock('../components/PatientManagement/SlotBookingModal', () => ({
  default: ({ isOpen, onClose }) =>
    isOpen ? <div data-testid="slot-booking-modal"><button onClick={onClose}>Close</button></div> : null,
}));
vi.mock('../components/PatientManagement/PatientManagement.css', () => ({}));

// ── Sample data ───────────────────────────────────────────────────────────────
const MOCK_PATIENTS = [
  { id: 1, name: 'Alice Perera', age: 32, bloodGroup: 'O+', nic: '9823456V' },
  { id: 2, name: 'Bob G.', age: 45, bloodGroup: 'O+', nic: '7912345V' },
];

const MOCK_RESULT = {
  summary: 'Possible cardiac event. Urgency: High.',
  specialtyRecommendation: 'Cardiology',
  urgencyLevel: 'High',
};

import { patientApi, intakeAgentApi } from '../api/channelCenterApi';

// ── Helper: wait for the component to settle ──────────────────────────────────
// Actual h1 text: "Intake & Intent Structuring Agent Console"
const waitForReady = () =>
  waitFor(() => expect(screen.getByRole('heading', { level: 1 })).toBeInTheDocument());

// Helper: wait for patient to be loaded and selected
const waitForPatientSelected = async (patientId = '1') => {
  await waitFor(() => {
    const select = screen.getByRole('combobox');
    expect(select.value).toBe(String(patientId));
  });
};

// ─────────────────────────────────────────────────────────────────────────────

describe('IntakeAgentConsoleTab', () => {
  beforeEach(() => {
    patientApi.list.mockResolvedValue({ items: MOCK_PATIENTS, totalCount: 2 });
    intakeAgentApi.getLogsByPatient.mockResolvedValue([]);
    intakeAgentApi.process.mockResolvedValue(MOCK_RESULT);
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  // ── 1. Render ─────────────────────────────────────────────────────────────

  it('renders the AI Intake console heading', async () => {
    render(<IntakeAgentConsoleTab />);
    await waitForReady();
    expect(screen.getByRole('heading', { level: 1 })).toHaveTextContent('Intake');
  });

  it('loads patient list on mount', async () => {
    render(<IntakeAgentConsoleTab />);
    await waitFor(() => expect(patientApi.list).toHaveBeenCalled());
  });

  // ── 2. initialPatient prop ────────────────────────────────────────────────

  it('pre-populates patient selector when initialPatient prop is given', async () => {
    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();

    // initialPatient sets selectedPatientId immediately
    const select = screen.getByRole('combobox');
    expect(select.value).toBe('1');
  });

  // ── 3. Validation guard: blank text keeps button disabled ────────────────────

  it('process button is disabled when textarea is empty', async () => {
    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();
    await waitForPatientSelected('1');

    // Patient is selected but textarea is empty — button must stay disabled
    const processBtn = screen.getByRole('button', { name: /process intake/i });
    expect(processBtn).toBeDisabled();
  });

  // ── 4. Validation guard: no patient shows idle placeholder ─────────────────

  it('shows Intake Agent Ready placeholder when no patient is selected', async () => {
    // Empty patients list — selector stays on blank option
    patientApi.list.mockResolvedValue({ items: [], totalCount: 0 });

    render(<IntakeAgentConsoleTab />);
    await waitForReady();
    await waitFor(() => patientApi.list.mock.calls.length > 0);

    // Right panel should show the idle "Intake Agent Ready" state
    expect(screen.getByText('Intake Agent Ready')).toBeInTheDocument();
  });

  // ── 5. Successful processing ──────────────────────────────────────────────

  it('calls intakeAgentApi.process with correct payload', async () => {
    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();
    await waitForPatientSelected('1');

    const CLINICAL_TEXT = 'Patient has severe chest pain and shortness of breath.';
    await userEvent.type(screen.getByRole('textbox'), CLINICAL_TEXT);

    await waitFor(() => {
      const btn = screen.getByRole('button', { name: /process intake/i });
      expect(btn).not.toBeDisabled();
    });
    await userEvent.click(screen.getByRole('button', { name: /process intake/i }));

    await waitFor(() =>
      expect(intakeAgentApi.process).toHaveBeenCalledWith({
        patientId: 1,
        rawText: CLINICAL_TEXT,
      })
    );
  });

  it('displays IntakeResultCard after successful processing', async () => {
    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();
    await waitForPatientSelected('1');

    await userEvent.type(screen.getByRole('textbox'), 'Shortness of breath and fatigue.');

    await waitFor(() =>
      expect(screen.getByRole('button', { name: /process intake/i })).not.toBeDisabled()
    );
    await userEvent.click(screen.getByRole('button', { name: /process intake/i }));

    await waitFor(() =>
      expect(screen.getByTestId('intake-result-card')).toBeInTheDocument()
    );
  });

  it('shows error when process API fails', async () => {
    intakeAgentApi.process.mockRejectedValue(new Error('AI agent offline'));

    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();
    await waitForPatientSelected('1');

    await userEvent.type(screen.getByRole('textbox'), 'Severe migraine and vomiting.');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: /process intake/i })).not.toBeDisabled()
    );
    await userEvent.click(screen.getByRole('button', { name: /process intake/i }));

    await waitFor(() =>
      expect(screen.getByText('AI agent offline')).toBeInTheDocument()
    );
  });

  // ── 6. Sample prompt buttons ──────────────────────────────────────────────

  it('fills textarea when a sample prompt button is clicked', async () => {
    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();

    // Chip button text: "🚨 Chest Pain & Shortness of Breath (Cardiology)"
    const sampleBtn = screen.getAllByRole('button', { name: /cardiology/i })[0];
    await userEvent.click(sampleBtn);

    const textarea = screen.getByRole('textbox');
    expect(textarea.value).toContain('cardiologist');
  });

  // ── 7. Proceed-to-booking modal ───────────────────────────────────────────

  it('opens SlotBookingModal when Proceed to Booking is clicked after result', async () => {
    render(<IntakeAgentConsoleTab initialPatient={MOCK_PATIENTS[0]} />);
    await waitForReady();
    await waitForPatientSelected('1');

    await userEvent.type(screen.getByRole('textbox'), 'Shortness of breath and fever.');
    await waitFor(() =>
      expect(screen.getByRole('button', { name: /process intake/i })).not.toBeDisabled()
    );
    await userEvent.click(screen.getByRole('button', { name: /process intake/i }));

    // Wait for the result card to appear (with the Proceed button)
    await waitFor(() => screen.getByTestId('intake-result-card'));

    // "📅 Proceed to Booking" button is rendered by our IntakeResultCard mock
    await userEvent.click(screen.getByRole('button', { name: /proceed to booking/i }));
    expect(screen.getByTestId('slot-booking-modal')).toBeInTheDocument();
  });
});
