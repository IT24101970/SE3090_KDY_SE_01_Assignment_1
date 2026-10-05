import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import AuditTrailTab from '../components/WorkflowConsole/AuditTrailTab';
import { workflowApi } from '../api/channelCenterApi';

vi.mock('../api/channelCenterApi', () => ({
    workflowApi: {
        audit: vi.fn()
    }
}));

describe('AuditTrailTab', () => {
    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders audit logs with collapsible structured payload dropdowns', async () => {
        const mockLogs = [
            {
                id: 1,
                workflowId: 10,
                agentName: 'IntakeAgent',
                toolCalled: 'ParseSymptoms',
                toolOutput: JSON.stringify({ symptom: 'Fever', severity: 'High' }),
                createdAt: new Date().toISOString()
            }
        ];
        workflowApi.audit.mockResolvedValue(mockLogs);

        render(<AuditTrailTab />);

        expect(screen.getByText(/Loading governance logs…/i)).toBeInTheDocument();

        await waitFor(() => {
            expect(screen.getByText('IntakeAgent')).toBeInTheDocument();
        });

        expect(screen.getByText('⚡ ParseSymptoms')).toBeInTheDocument();
        const dropdownBtn = screen.getByText(/Tool Output \(ParseSymptoms\)/i);
        expect(dropdownBtn).toBeInTheDocument();

        // Click dropdown button to reveal unordered detailed list
        fireEvent.click(dropdownBtn);

        expect(screen.getByText('Symptom:')).toBeInTheDocument();
        expect(screen.getByText('Fever')).toBeInTheDocument();
    });

    it('filters logs by search query', async () => {
        const mockLogs = [
            { id: 1, agentName: 'IntakeAgent', toolCalled: 'ParseSymptoms', toolOutput: '{}', createdAt: new Date().toISOString() },
            { id: 2, agentName: 'TriageAgent', toolCalled: 'AssessUrgency', toolOutput: '{}', createdAt: new Date().toISOString() }
        ];
        workflowApi.audit.mockResolvedValue(mockLogs);

        render(<AuditTrailTab />);

        await waitFor(() => {
            expect(screen.getByText('IntakeAgent')).toBeInTheDocument();
            expect(screen.getByText('TriageAgent')).toBeInTheDocument();
        });

        const searchInput = screen.getByPlaceholderText(/Search by agent/i);
        fireEvent.change(searchInput, { target: { value: 'Triage' } });

        expect(screen.queryByText('IntakeAgent')).not.toBeInTheDocument();
        expect(screen.getByText('TriageAgent')).toBeInTheDocument();
    });
});
