/**
 * ClinicalReviewView.test.jsx
 *
 * Component 3 — Medical Triage & Specialist Matching
 * Unit tests for Clinical Referral Review Queue Dashboard.
 */
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { render, screen, waitFor, cleanup } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { ClinicalReviewView } from '../components/ClinicalReviewView';

vi.mock('../api/channelCenterApi', () => ({
  referralApi: {
    getAll: vi.fn(),
    updateStatus: vi.fn(),
  },
}));

vi.mock('../App.css', () => ({}));

const MOCK_REFERRALS_API = [
  {
    id: 1,
    triageId: 1,
    targetSpecialty: 'Cardiology',
    status: 'Generated',
    triageSummary: {
      id: 1,
      appointmentId: 101,
      rawSymptoms: 'Severe chest pain radiating to left arm',
      urgencyScore: 95,
      urgencyLevel: 'Emergency',
      reasoningTrace: '[Symptom Triage Agent] Chest pain emergency matched Cardiology.',
      recommendedSpecialty: 'Cardiology',
      symptomLogs: [{ symptomKeyword: 'chest pain', severityRating: 9, durationInDays: 1 }],
      createdAt: '2026-09-28T10:00:00Z',
    },
  },
  {
    id: 2,
    triageId: 2,
    targetSpecialty: 'Dermatology',
    status: 'Reviewed',
    triageSummary: {
      id: 2,
      appointmentId: 102,
      rawSymptoms: 'Red skin rash on right forearm',
      urgencyScore: 40,
      urgencyLevel: 'Medium',
      reasoningTrace: '[Symptom Triage Agent] Skin rash matched Dermatology.',
      recommendedSpecialty: 'Dermatology',
      symptomLogs: [{ symptomKeyword: 'skin rash', severityRating: 4, durationInDays: 3 }],
      createdAt: '2026-09-28T09:00:00Z',
    },
  },
];

import { referralApi } from '../api/channelCenterApi';

describe('ClinicalReviewView', () => {
  beforeEach(() => {
    referralApi.getAll.mockResolvedValue(MOCK_REFERRALS_API);
    referralApi.updateStatus.mockResolvedValue({});
  });

  afterEach(() => {
    cleanup();
    vi.clearAllMocks();
  });

  it('renders header title and subtitle', async () => {
    render(<ClinicalReviewView />);
    expect(screen.getByText('Medical Triage & Specialist Matching')).toBeInTheDocument();
    expect(screen.getByText('Clinical Review Board & AI Symptom Triage Assessment Monitor')).toBeInTheDocument();
  });

  it('loads and displays referrals from referralApi', async () => {
    render(<ClinicalReviewView />);
    await waitFor(() => {
      expect(screen.getByText(/Severe chest pain radiating to left arm/i)).toBeInTheDocument();
      expect(screen.getByText(/Red skin rash on right forearm/i)).toBeInTheDocument();
    });
  });

  it('filters table rows when search query is entered', async () => {
    render(<ClinicalReviewView />);
    await waitFor(() => expect(screen.getByText(/Severe chest pain/i)).toBeInTheDocument());

    const searchInput = screen.getByPlaceholderText(/search symptoms/i);
    await userEvent.type(searchInput, 'Dermatology');

    expect(screen.queryByText(/Severe chest pain/i)).not.toBeInTheDocument();
    expect(screen.getByText(/Red skin rash on right forearm/i)).toBeInTheDocument();
  });

  it('opens TriageDetailModal when Review button is clicked', async () => {
    render(<ClinicalReviewView />);
    await waitFor(() => expect(screen.getByText(/Severe chest pain/i)).toBeInTheDocument());

    const reviewButtons = screen.getAllByRole('button', { name: /review/i });
    await userEvent.click(reviewButtons[0]);

    expect(screen.getByText('Triage Assessment #1')).toBeInTheDocument();
    expect(screen.getByText('[Symptom Triage Agent] Chest pain emergency matched Cardiology.')).toBeInTheDocument();
  });

  it('calls referralApi.updateStatus when updating referral status from modal', async () => {
    render(<ClinicalReviewView />);
    await waitFor(() => expect(screen.getByText(/Severe chest pain/i)).toBeInTheDocument());

    const reviewButtons = screen.getAllByRole('button', { name: /review/i });
    await userEvent.click(reviewButtons[0]);

    const assignStatusBtn = screen.getByRole('button', { name: /assign specialist referral/i });
    await userEvent.click(assignStatusBtn);

    expect(referralApi.updateStatus).toHaveBeenCalledWith(1, 'Assigned');
  });
});
