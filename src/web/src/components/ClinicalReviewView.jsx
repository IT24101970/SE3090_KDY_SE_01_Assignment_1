import React, { useState, useEffect } from 'react';
import './ClinicalReviewView.css';
import './PatientManagement/PatientManagement.css';
import { TriageDetailModal } from './TriageDetailModal';
import { referralApi } from '../api/channelCenterApi';

const MOCK_ASSESSMENTS = [
  {
    id: 1,
    appointmentId: 42,
    rawSymptoms: "Acute severe chest pain radiating to left shoulder with shortness of breath",
    urgencyScore: 95,
    urgencyLevel: "Emergency",
    reasoningTrace: "[Symptom Triage Agent] Parsed 2 symptom entries for raw symptoms: 'Acute severe chest pain radiating to left shoulder'. Highest severity score: 9/10. Emergency red flags: Detected. Matched specialty: Cardiology. Assessed urgency level: Emergency (Score: 95/100).",
    recommendedSpecialty: "Cardiology",
    symptomLogs: [
      { symptomKeyword: "chest pain", severityRating: 9, durationInDays: 1 },
      { symptomKeyword: "shortness of breath", severityRating: 8, durationInDays: 1 }
    ],
    createdAt: new Date().toISOString()
  },
  {
    id: 2,
    appointmentId: 45,
    rawSymptoms: "Severe migraine headache with sudden dizziness and light sensitivity",
    urgencyScore: 82,
    urgencyLevel: "Emergency",
    reasoningTrace: "[Symptom Triage Agent] Parsed 2 symptom entries. Highest severity rating: 8/10. Matched specialty: Neurology. Emergency red flags: Detected. Assessed urgency level: Emergency.",
    recommendedSpecialty: "Neurology",
    symptomLogs: [
      { symptomKeyword: "migraine", severityRating: 8, durationInDays: 2 },
      { symptomKeyword: "dizziness", severityRating: 7, durationInDays: 1 }
    ],
    createdAt: new Date(Date.now() - 3600000).toISOString()
  },
  {
    id: 3,
    appointmentId: 48,
    rawSymptoms: "Red itching skin rash on forearm after contacting new detergent",
    urgencyScore: 40,
    urgencyLevel: "Medium",
    reasoningTrace: "[Symptom Triage Agent] Parsed 1 symptom entry. Highest severity rating: 4/10. Emergency red flags: None. Matched specialty: Dermatology. Assessed urgency level: Medium.",
    recommendedSpecialty: "Dermatology",
    symptomLogs: [
      { symptomKeyword: "skin rash", severityRating: 4, durationInDays: 3 }
    ],
    createdAt: new Date(Date.now() - 7200000).toISOString()
  },
  {
    id: 4,
    appointmentId: 52,
    rawSymptoms: "Right knee pain and swelling following basketball game",
    urgencyScore: 65,
    urgencyLevel: "High",
    reasoningTrace: "[Symptom Triage Agent] Parsed 1 symptom entry. Highest severity rating: 6/10. Matched specialty: Orthopedics. Assessed urgency level: High.",
    recommendedSpecialty: "Orthopedics",
    symptomLogs: [
      { symptomKeyword: "knee pain", severityRating: 6, durationInDays: 2 }
    ],
    createdAt: new Date(Date.now() - 14400000).toISOString()
  }
];

const MOCK_REFERRALS = [
  { id: 1, triageId: 1, targetSpecialty: "Cardiology", status: "Generated" },
  { id: 2, triageId: 2, targetSpecialty: "Neurology", status: "Reviewed" },
  { id: 3, triageId: 3, targetSpecialty: "Dermatology", status: "Assigned" },
  { id: 4, triageId: 4, targetSpecialty: "Orthopedics", status: "Generated" }
];

export function ClinicalReviewView() {
  const [assessments, setAssessments] = useState(MOCK_ASSESSMENTS);
  const [referrals, setReferrals] = useState(MOCK_REFERRALS);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterUrgency, setFilterUrgency] = useState('ALL');
  const [filterSpecialty, setFilterSpecialty] = useState('ALL');
  const [selectedAssessment, setSelectedAssessment] = useState(null);

  useEffect(() => {
    let isMounted = true;
    async function loadData() {
      try {
        const data = await referralApi.getAll();
        if (isMounted && Array.isArray(data) && data.length > 0) {
          const loadedReferrals = data.map(r => ({
            id: r.id,
            triageId: r.triageId,
            targetSpecialty: r.targetSpecialty,
            status: r.status,
          }));
          const loadedAssessments = data
            .filter(r => r.triageSummary)
            .map(r => r.triageSummary);

          setReferrals(loadedReferrals);
          if (loadedAssessments.length > 0) {
            setAssessments(loadedAssessments);
          }
        }
      } catch (err) {
        console.warn('Using mock clinical review data:', err);
      }
    }
    loadData();
    return () => { isMounted = false; };
  }, []);

  const totalCount = assessments.length;
  const emergencyCount = assessments.filter(a => a.urgencyLevel?.toLowerCase() === 'emergency').length;
  const highCount = assessments.filter(a => a.urgencyLevel?.toLowerCase() === 'high').length;
  const avgScore = totalCount > 0 ? Math.round(assessments.reduce((acc, curr) => acc + curr.urgencyScore, 0) / totalCount) : 0;

  const handleUpdateReferralStatus = async (referralId, newStatus) => {
    try {
      await referralApi.updateStatus(referralId, newStatus);
    } catch (err) {
      console.warn('Failed to update referral status on backend:', err);
    }
    setReferrals(prev => prev.map(r => r.id === referralId ? { ...r, status: newStatus } : r));
    if (selectedAssessment) {
      setSelectedAssessment(null);
    }
  };

  const filteredAssessments = assessments.filter(item => {
    const matchesSearch = item.rawSymptoms.toLowerCase().includes(searchTerm.toLowerCase()) ||
                          item.appointmentId.toString().includes(searchTerm) ||
                          item.recommendedSpecialty.toLowerCase().includes(searchTerm.toLowerCase());
    
    const matchesUrgency = filterUrgency === 'ALL' || item.urgencyLevel?.toUpperCase() === filterUrgency;
    const matchesSpecialty = filterSpecialty === 'ALL' || item.recommendedSpecialty.toUpperCase() === filterSpecialty;

    return matchesSearch && matchesUrgency && matchesSpecialty;
  });

  const getUrgencyBadgeClass = (level) => {
    switch (level?.toLowerCase()) {
      case 'emergency': return 'urgency-pill emergency';
      case 'high': return 'urgency-pill high';
      case 'medium': return 'urgency-pill medium';
      default: return 'urgency-pill low';
    }
  };

  const getReferralBadgeClass = (status) => {
    switch (status?.toLowerCase()) {
      case 'assigned': return 'referral-pill assigned';
      case 'reviewed': return 'referral-pill reviewed';
      default: return 'referral-pill generated';
    }
  };

  const getReferralStatus = (triageId) => {
    const ref = referrals.find(r => r.triageId === triageId);
    return ref ? ref.status : 'Generated';
  };

  return (
    <div className="clinical-container pm-container">
      {/* Header */}
      <div className="clinical-header pm-header">
        <div className="clinical-title-section pm-header-titles">
          <span className="eyebrow">COMPONENT 3 — TRIAGE & SPECIALIST MATCHING</span>
          <h1>Medical Triage & Specialist Matching</h1>
          <p>Clinical Review Board & AI Symptom Triage Assessment Monitor</p>
        </div>
        <div className="pm-header-actions">
          <div className="live-badge">
            <div className="live-pulse"></div>
            AI Triage Agent Active
          </div>
        </div>
      </div>

      {/* Metrics Row */}
      <div className="pm-stats-grid">
        <div className="pm-stat-card">
          <div className="pm-stat-label">Total Intakes</div>
          <div className="pm-stat-value">{totalCount}</div>
          <div className="pm-stat-hint">Active clinical cases</div>
        </div>
        <div className="pm-stat-card">
          <div className="pm-stat-label">Emergency Alerts</div>
          <div className="pm-stat-value" style={{ color: 'var(--red)' }}>{emergencyCount}</div>
          <div className="pm-stat-hint">Immediate action required</div>
        </div>
        <div className="pm-stat-card">
          <div className="pm-stat-label">High Priority</div>
          <div className="pm-stat-value" style={{ color: 'var(--amber)' }}>{highCount}</div>
          <div className="pm-stat-hint">Urgent specialist matching</div>
        </div>
        <div className="pm-stat-card">
          <div className="pm-stat-label">Avg Severity Index</div>
          <div className="pm-stat-value" style={{ color: 'var(--blue)' }}>{avgScore}/100</div>
          <div className="pm-stat-hint">Composite risk index</div>
        </div>
      </div>

      {/* Controls Bar */}
      <div className="pm-toolbar">
        <div className="pm-search-box" style={{ flex: 1, minWidth: '280px' }}>
          <span className="pm-search-icon">🔍</span>
          <input
            type="text"
            className="pm-search-input"
            placeholder="Search symptoms, appointment ID, or specialty..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <div style={{ display: 'flex', gap: '10px' }}>
          <select className="pm-select" value={filterUrgency} onChange={(e) => setFilterUrgency(e.target.value)}>
            <option value="ALL">All Urgency Levels</option>
            <option value="EMERGENCY">Emergency</option>
            <option value="HIGH">High</option>
            <option value="MEDIUM">Medium</option>
            <option value="LOW">Low</option>
          </select>
          <select className="pm-select" value={filterSpecialty} onChange={(e) => setFilterSpecialty(e.target.value)}>
            <option value="ALL">All Specialties</option>
            <option value="CARDIOLOGY">Cardiology</option>
            <option value="NEUROLOGY">Neurology</option>
            <option value="DERMATOLOGY">Dermatology</option>
            <option value="ORTHOPEDICS">Orthopedics</option>
          </select>
        </div>
      </div>

      {/* Data Table Card */}
      <div className="pm-table-card">
        <table className="pm-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Appointment</th>
              <th>Raw Symptoms</th>
              <th>Urgency Level</th>
              <th>Score</th>
              <th>Matched Specialty</th>
              <th>Referral Status</th>
              <th style={{ textAlign: 'right' }}>Action</th>
            </tr>
          </thead>
          <tbody>
            {filteredAssessments.length === 0 ? (
              <tr>
                <td colSpan="8" style={{ textAlign: 'center', padding: '2rem', color: 'var(--muted)' }}>
                  No triage records matched the selected filters.
                </td>
              </tr>
            ) : (
              filteredAssessments.map(row => {
                const status = getReferralStatus(row.id);
                return (
                  <tr key={row.id} onClick={() => setSelectedAssessment(row)}>
                    <td style={{ fontWeight: 700 }}>#{row.id}</td>
                    <td>{row.patientName ? `${row.patientName} (#${row.appointmentId})` : `Appt #${row.appointmentId}`}</td>
                    <td style={{ maxWidth: '300px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {row.rawSymptoms}
                    </td>
                    <td>
                      <span className={getUrgencyBadgeClass(row.urgencyLevel)}>
                        {row.urgencyLevel}
                      </span>
                    </td>
                    <td style={{ fontWeight: 700 }}>{row.urgencyScore}/100</td>
                    <td style={{ color: 'var(--blue)', fontWeight: 600 }}>
                      {row.recommendedSpecialty || 'General Medicine'}
                    </td>
                    <td>
                      <span className={getReferralBadgeClass(status)}>
                        {status}
                      </span>
                    </td>
                    <td style={{ textAlign: 'right' }}>
                      <button className="outline-button" onClick={(e) => { e.stopPropagation(); setSelectedAssessment(row); }}>
                        Review
                      </button>
                    </td>
                  </tr>
                );
              })
            )}
          </tbody>
        </table>
      </div>

      {/* Modal */}
      {selectedAssessment && (
        <TriageDetailModal
          assessment={selectedAssessment}
          referral={referrals.find(r => r.triageId === selectedAssessment.id)}
          onClose={() => setSelectedAssessment(null)}
          onUpdateReferralStatus={handleUpdateReferralStatus}
        />
      )}
    </div>
  );
}
