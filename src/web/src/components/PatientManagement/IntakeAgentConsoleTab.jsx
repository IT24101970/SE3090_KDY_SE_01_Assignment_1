import { useState, useEffect, useCallback } from 'react';
import { intakeAgentApi, patientApi } from '../../api/channelCenterApi';
import IntakeResultCard from './IntakeResultCard';
import SlotBookingModal from './SlotBookingModal';
import './PatientManagement.css';

const SAMPLE_PROMPTS = [
  {
    label: '🚨 Chest Pain & Shortness of Breath (Cardiology)',
    text: 'Patient reports severe sharp chest pain radiating to left shoulder with acute shortness of breath for the past 2 days. Prefers a morning appointment with a cardiologist.',
  },
  {
    label: '🧠 Severe Migraine & Dizziness (Neurology)',
    text: 'Suffering from an intense pulsating migraine headache with visual aura and nausea for 3 days. Dizziness when standing up. Prefers Dr. Michael Chen or any neurologist.',
  },
  {
    label: '🧴 Spreading Skin Rash (Dermatology)',
    text: 'Developed an itchy, red spreading cutaneous rash on forearms for 5 days after working outdoors. Mild discomfort, requesting dermatology checkup this week.',
  },
  {
    label: '🦴 Chronic Knee & Joint Pain (Orthopedics)',
    text: 'Severe right knee swelling and persistent joint stiffness for over 2 weeks making stairs difficult. Looking for orthopedic consultation afternoon session.',
  },
];

export default function IntakeAgentConsoleTab({ initialPatient }) {
  const [patients, setPatients] = useState([]);
  const [selectedPatientId, setSelectedPatientId] = useState(initialPatient ? String(initialPatient.id) : '');

  const selectedPatient = patients.find((p) => String(p.id) === String(selectedPatientId))
    || (initialPatient && String(initialPatient.id) === String(selectedPatientId) ? initialPatient : null);

  const [rawText, setRawText] = useState('');
  const [processing, setProcessing] = useState(false);
  const [processingStep, setProcessingStep] = useState('');
  const [error, setError] = useState('');

  const [structuredResult, setStructuredResult] = useState(null);
  const [auditLogs, setAuditLogs] = useState([]);
  const [loadingLogs, setLoadingLogs] = useState(false);
  const [expandedLogId, setExpandedLogId] = useState(null);

  // Booking Modal
  const [bookingModalOpen, setBookingModalOpen] = useState(false);
  const [bookingPrefilledPatient, setBookingPrefilledPatient] = useState(null);

  const loadPatientLogs = useCallback(async (patientId) => {
    setLoadingLogs(true);
    try {
      const logs = await intakeAgentApi.getLogsByPatient(patientId);
      setAuditLogs(logs || []);
    } catch {
      setAuditLogs([]);
    } finally {
      setLoadingLogs(false);
    }
  }, []);

  const loadPatients = useCallback(async () => {
    try {
      const res = await patientApi.list({ pageSize: 50 });
      const items = res.items || [];
      setPatients(items);
      if (!selectedPatientId && items.length > 0) {
        setSelectedPatientId(String(items[0].id));
      }
    } catch {
      // Fallback
    }
  }, [selectedPatientId]);

  useEffect(() => {
    loadPatients();
  }, [loadPatients]);

  useEffect(() => {
    if (selectedPatientId) {
      loadPatientLogs(Number(selectedPatientId));
    }
  }, [selectedPatientId, loadPatientLogs]);

  const handleProcess = async (e) => {
    e.preventDefault();
    setError('');
    setStructuredResult(null);

    if (!selectedPatientId) {
      setError('Please select a patient for the intake process.');
      return;
    }
    if (!rawText.trim() || rawText.trim().length < 5) {
      setError('Please provide detailed symptom or clinical intake text (minimum 5 characters).');
      return;
    }

    setProcessing(true);
    setProcessingStep('1/3: Calling ValidatePatientEligibility tool...');

    try {
      setTimeout(() => {
        setProcessingStep('2/3: Calling GetPatientHistory tool...');
      }, 350);

      setTimeout(() => {
        setProcessingStep('3/3: Calling FormatIntakeSummary NLP tool...');
      }, 700);

      const res = await intakeAgentApi.process({
        patientId: Number(selectedPatientId),
        rawText: rawText.trim(),
      });

      setStructuredResult(res);
      // Reload audit logs after agent execution
      loadPatientLogs(Number(selectedPatientId));
    } catch (err) {
      setError(err.message || 'Intake agent encountered an execution failure.');
    } finally {
      setProcessing(false);
      setProcessingStep('');
    }
  };

  const handleProceedToBooking = () => {
    setBookingPrefilledPatient(selectedPatient);
    setBookingModalOpen(true);
  };

  return (
    <div className="pm-container">
      {/* Header */}
      <div className="pm-header">
        <div className="pm-header-titles">
          <p className="eyebrow">Dedicated AI Agent · Component 01</p>
          <h1>Intake & Intent Structuring Agent Console</h1>
          <p>
            Transforms unstructured patient symptoms into a verified clinical execution package with allow-listed tool execution.
          </p>
        </div>
      </div>

      <div className="pm-intake-shell">
        {/* Left Column: Natural Language Input & Controls */}
        <div className="pm-intake-pane">
          <div>
            <span className="eyebrow" style={{ color: 'var(--blue)' }}>Patient Selection</span>
            <h3 style={{ font: "700 17px 'Space Grotesk', sans-serif", margin: '4px 0 10px', color: 'var(--ink)' }}>
              1. Choose Patient Profile
            </h3>

            <select
              className="pm-select-field"
              style={{ width: '100%' }}
              value={selectedPatientId}
              onChange={(e) => setSelectedPatientId(e.target.value)}
              disabled={processing}
            >
              <option value="">-- Choose patient --</option>
              {patients.map((p) => (
                <option key={p.id} value={p.id}>
                  #{p.id} — {p.name} (NIC: {p.nic}, Age: {p.age}, Blood: {p.bloodGroup || 'O+'})
                </option>
              ))}
            </select>

            {selectedPatient && (
              <div style={{
                background: '#f8fafc',
                border: '1px solid var(--line)',
                borderRadius: '8px',
                padding: '10px 12px',
                marginTop: '8px',
                fontSize: '12px',
                color: 'var(--muted)',
                display: 'flex',
                justifyContent: 'space-between',
              }}>
                <span>Emergency: {selectedPatient.emergencyContact}</span>
                <span>Allergies: <strong style={{ color: selectedPatient.allergies ? 'var(--red)' : 'var(--ink)' }}>{selectedPatient.allergies || 'None'}</strong></span>
              </div>
            )}
          </div>

          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '4px' }}>
              <h3 style={{ font: "700 17px 'Space Grotesk', sans-serif", margin: 0, color: 'var(--ink)' }}>
                2. Natural Language Intake Narrative
              </h3>
              <span style={{ fontSize: '11px', color: 'var(--muted)' }}>Unstructured freeform text</span>
            </div>

            {/* Quick Prompt Chips */}
            <div style={{ marginBottom: '8px' }}>
              <span style={{ fontSize: '11px', fontWeight: 600, color: 'var(--muted)' }}>Quick Clinical Templates:</span>
              <div className="pm-template-chips">
                {SAMPLE_PROMPTS.map((sample, idx) => (
                  <button
                    key={idx}
                    type="button"
                    className="pm-chip"
                    onClick={() => setRawText(sample.text)}
                    disabled={processing}
                  >
                    {sample.label}
                  </button>
                ))}
              </div>
            </div>

            <textarea
              className={`pm-textarea ${error ? 'error' : ''}`}
              rows={5}
              style={{ width: '100%', fontSize: '13px', lineHeight: '1.5' }}
              placeholder="Enter patient narrative, verbal complaint, onset duration, and preferences (e.g. 'Patient has experienced high fever and vomiting for 2 days, would prefer a pediatrician on Friday morning...')"
              value={rawText}
              onChange={(e) => setRawText(e.target.value)}
              disabled={processing}
            />
          </div>

          {error && (
            <div className="pm-alert error" role="alert">
              <span>⚠️</span> <span>{error}</span>
            </div>
          )}

          {processing && (
            <div style={{
              background: '#edf4ff',
              border: '1px solid #cce0ff',
              borderRadius: '8px',
              padding: '12px',
              fontSize: '12px',
              color: 'var(--blue)',
              display: 'flex',
              alignItems: 'center',
              gap: '10px',
            }}>
              <div className="pm-spinner" style={{ width: '18px', height: '18px' }} />
              <strong>{processingStep || 'Processing structured intent...'}</strong>
            </div>
          )}

          <div>
            <button
              type="button"
              className="primary-button"
              style={{ width: '100%', padding: '12px 18px', fontSize: '13px' }}
              onClick={handleProcess}
              disabled={processing || !selectedPatientId || !rawText.trim()}
            >
              {processing ? 'Running Intake Agent Pipeline...' : '⚡ Process Intake with AI Agent'}
            </button>
          </div>

          {/* Audit Logs Drawer / Accordion */}
          <div style={{ borderTop: '1px solid var(--line)', paddingTop: '16px', marginTop: '4px' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
              <span className="pm-label" style={{ fontSize: '12px' }}>
                📋 Tool Execution & Audit Logs ({auditLogs.length})
              </span>
              <button
                className="pm-btn-sm outline"
                onClick={() => selectedPatientId && loadPatientLogs(Number(selectedPatientId))}
                disabled={loadingLogs}
              >
                {loadingLogs ? 'Loading...' : '↻ Refresh Logs'}
              </button>
            </div>

            {loadingLogs && (
              <div style={{ fontSize: '11px', color: 'var(--muted)', padding: '8px 0' }}>
                Fetching persisted tool audit traces...
              </div>
            )}

            {!loadingLogs && auditLogs.length === 0 && (
              <p style={{ fontSize: '11px', color: 'var(--muted)', margin: 0 }}>
                No tool audit traces persisted for this patient yet. Execute the agent above.
              </p>
            )}

            {!loadingLogs && auditLogs.length > 0 && (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '6px', maxHeight: '180px', overflowY: 'auto' }}>
                {auditLogs.map((log) => {
                  const isExpanded = expandedLogId === log.id;
                  return (
                    <div
                      key={log.id}
                      style={{
                        background: '#f8fafc',
                        border: '1px solid var(--line)',
                        borderRadius: '6px',
                        padding: '8px 10px',
                        fontSize: '11px',
                      }}
                    >
                      <div
                        style={{
                          display: 'flex',
                          justifyContent: 'space-between',
                          alignItems: 'center',
                          cursor: 'pointer',
                        }}
                        onClick={() => setExpandedLogId(isExpanded ? null : log.id)}
                      >
                        <div>
                          <strong style={{ color: 'var(--ink)' }}>Tool: {log.toolCalled}</strong>
                          <span style={{ color: 'var(--muted)', marginLeft: '8px' }}>
                            {new Date(log.createdAt).toLocaleTimeString()}
                          </span>
                        </div>
                        <span style={{ color: 'var(--blue)', fontWeight: 600 }}>
                          {isExpanded ? 'Hide Payload ▲' : 'View Payload ▼'}
                        </span>
                      </div>
                      {log.sessionNotes && (
                        <div style={{ color: 'var(--muted)', marginTop: '3px' }}>
                          {log.sessionNotes}
                        </div>
                      )}
                      {isExpanded && (
                        <div style={{ marginTop: '6px', borderTop: '1px dashed var(--line)', paddingTop: '6px' }}>
                          <div style={{ fontWeight: 600, color: 'var(--ink)', marginBottom: '2px' }}>OutputPayload:</div>
                          <pre style={{
                            background: '#fff',
                            border: '1px solid var(--line)',
                            borderRadius: '4px',
                            padding: '6px',
                            fontSize: '10px',
                            maxHeight: '120px',
                            overflow: 'auto',
                            margin: 0,
                          }}>
                            {typeof log.outputPayload === 'string' ? log.outputPayload : JSON.stringify(log.outputPayload, null, 2)}
                          </pre>
                        </div>
                      )}
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        {/* Right Column: Structured Output or Idle Placeholder */}
        <div>
          {structuredResult ? (
            <IntakeResultCard
              result={structuredResult}
              onProceedToBooking={handleProceedToBooking}
            />
          ) : (
            <div className="pm-intake-pane" style={{ justifyContent: 'center', alignItems: 'center', textAlign: 'center', minHeight: '400px' }}>
              <div style={{ fontSize: '42px', marginBottom: '12px' }}>⚡</div>
              <h3 style={{ font: "700 19px 'Space Grotesk', sans-serif", margin: '0 0 6px', color: 'var(--ink)' }}>
                Intake Agent Ready
              </h3>
              <p style={{ fontSize: '13px', color: 'var(--muted)', maxWidth: '360px', margin: 0 }}>
                Select a patient on the left and enter or choose a clinical complaint. The agent will execute allow-listed tools, validate medical history, and structure symptom entities.
              </p>
            </div>
          )}
        </div>
      </div>

      {/* Booking Modal with Pre-filled Patient */}
      <SlotBookingModal
        isOpen={bookingModalOpen}
        preselectedPatient={bookingPrefilledPatient}
        onClose={() => setBookingModalOpen(false)}
        onBooked={() => {
          // Success
        }}
      />
    </div>
  );
}
