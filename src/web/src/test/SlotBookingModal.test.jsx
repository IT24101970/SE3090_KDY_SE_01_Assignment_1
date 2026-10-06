import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, cleanup } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import SlotBookingModal from '../components/PatientManagement/SlotBookingModal';

vi.mock('../api/channelCenterApi', () => ({
  patientApi: {
    list: vi.fn(),
  },
  specialtiesApi: {
    list: vi.fn(),
  },
  doctorApi: {
    list: vi.fn(),
  },
  appointmentApi: {
    getSlots: vi.fn(),
    create: vi.fn(),
  },
}));

vi.mock('../components/PatientManagement/PatientManagement.css', () => ({}));

import { appointmentApi, patientApi, specialtiesApi, doctorApi } from '../api/channelCenterApi';

const MOCK_PATIENT = {
  id: 3,
  name: 'nethmi herath',
  nic: '7895452102',
  phoneNumber: '07894561233',
};

const MOCK_SLOTS = [
  {
    scheduleId: 9,
    doctorId: 2,
    doctorName: 'Dr. Michael Chen',
    specialtyId: 2,
    specialtyName: 'Neurology',
    roomName: 'Room 505',
    roomFloor: '5th Floor-E wing',
    startTime: '2026-09-29T13:45:00Z',
    endTime: '2026-09-29T15:25:00Z',
    maxPatients: 15,
    bookedSlots: 0,
    availableSlots: 15,
    isAvailable: true,
  },
];

describe('SlotBookingModal', () => {
  beforeEach(() => {
    vi.clearAllMocks();
    patientApi.list.mockResolvedValue({ items: [MOCK_PATIENT] });
    specialtiesApi.list.mockResolvedValue([{ id: 2, name: 'Neurology' }]);
    doctorApi.list.mockResolvedValue([{ id: 2, fullName: 'Dr. Michael Chen' }]);
    appointmentApi.getSlots.mockResolvedValue(MOCK_SLOTS);
  });

  afterEach(() => {
    cleanup();
  });

  it('renders correctly and auto-selects slot when only 1 available slot exists', async () => {
    render(
      <SlotBookingModal
        isOpen={true}
        preselectedPatient={MOCK_PATIENT}
        onClose={vi.fn()}
        onBooked={vi.fn()}
      />
    );

    expect(screen.getByText('Book Patient Appointment')).toBeInTheDocument();
    await waitFor(() => {
      expect(screen.getAllByText('Dr. Michael Chen').length).toBeGreaterThan(0);
    });

    // Verify auto-selected slot indicator and summary banner
    await waitFor(() => {
      expect(screen.getByText('✓ Selected Slot')).toBeInTheDocument();
      expect(screen.getByText(/Selected Session:/i)).toBeInTheDocument();
    });
  });

  it('shows validation error when reason for visit is too short', async () => {
    const user = userEvent.setup();
    render(
      <SlotBookingModal
        isOpen={true}
        preselectedPatient={MOCK_PATIENT}
        onClose={vi.fn()}
        onBooked={vi.fn()}
      />
    );

    await waitFor(() => {
      expect(screen.getAllByText('Dr. Michael Chen').length).toBeGreaterThan(0);
    });

    const confirmBtn = screen.getByRole('button', { name: /Confirm Appointment/i });
    await user.click(confirmBtn);

    await waitFor(() => {
      expect(screen.getByText(/Reason for visit must be between 3 and 500 characters/i)).toBeInTheDocument();
    });
    expect(appointmentApi.create).not.toHaveBeenCalled();
  });

  it('successfully books appointment and calls onBooked when form is valid', async () => {
    const user = userEvent.setup();
    const handleBooked = vi.fn();
    const handleClose = vi.fn();

    appointmentApi.create.mockResolvedValueOnce({
      id: 10,
      patientId: 3,
      normalizedRawSymptoms: 'headache; dizziness',
    });

    render(
      <SlotBookingModal
        isOpen={true}
        preselectedPatient={MOCK_PATIENT}
        onClose={handleClose}
        onBooked={handleBooked}
      />
    );

    await waitFor(() => {
      expect(screen.getAllByText('Dr. Michael Chen').length).toBeGreaterThan(0);
    });

    const reasonInput = screen.getByPlaceholderText(/Describe your reason for visiting/i);
    await user.type(reasonInput, 'Severe migraine and nausea for two days');

    const confirmBtn = screen.getByRole('button', { name: /Confirm Appointment/i });
    await user.click(confirmBtn);

    await waitFor(() => {
      expect(appointmentApi.create).toHaveBeenCalledWith({
        patientId: 3,
        doctorId: 2,
        scheduleId: 9,
        appointmentDate: '2026-09-29T13:45:00Z',
        reasonForVisit: 'Severe migraine and nausea for two days',
        skipAiWorkflows: true,
      });
      expect(handleBooked).toHaveBeenCalledTimes(1);
      expect(handleClose).toHaveBeenCalledTimes(1);
    });
  });
});
