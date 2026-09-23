import React, { useState } from 'react';
import './ClinicalReviewView.css';
import { TriageDetailModal } from './TriageDetailModal';

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
  const [assessments] = useState(MOCK_ASSESSMENTS);
  const [referrals, setReferrals] = useState(MOCK_REFERRALS);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterUrgency, setFilterUrgency] = useState('ALL');
  const [filterSpecialty, setFilterSpecialty] = useState('ALL');
  const [selectedAssessment, setSelectedAssessment] = useState(null);

  const totalCount = assessments.length;
  const emergencyCount = assessments.filter(a => a.urgencyLevel?.toLowerCase() === 'emergency').length;
  const highCount = assessments.filter(a => a.urgencyLevel?.toLowerCase() === 'high').length;
  const avgScore = totalCount > 0 ? Math.round(assessments.reduce((acc, curr) => acc + curr.urgencyScore, 0) / totalCount) : 0;

  const handleUpdateReferralStatus = (referralId, newStatus) => {
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
      case 'emergency': return 'urgency-badge emergency';
      case 'high': return 'urgency-badge high';
      case 'medium': return 'urgency-badge medium';
      default: return 'urgency-badge low';
    }
  };

  const getReferralStatus = (triageId) => {
    const ref = referrals.find(r => r.triageId === triageId);
    return ref ? ref.status : 'Generated';
  };

  return (
    <div className="clinical-container">
      {/* Header */}
      <div className="clinical-header">
        <div className="clinical-title-section">
          <h1>Medical Triage & Specialist Matching</h1>
          <p>Clinical Review Board & AI Symptom Triage Assessment Monitor</p>
        </div>
        <div className="live-badge">
          <div className="live-pulse"></div>
          AI Triage Agent Active
        </div>
      </div>

      {/* Metrics Row */}
      <div className="metrics-grid">
        <div className="metric-card">
          <div className="metric-label">Total Intakes</div>
          <div className="metric-value">{totalCount}</div>
        </div>
        <div className="metric-card">
          <div className="metric-label">Emergency Alerts</div>
          <div className="metric-value emergency">{emergencyCount}</div>
        </div>
        <div className="metric-card">
          <div className="metric-label">High Priority</div>
          <div className="metric-value high">{highCount}</div>
        </div>
        <div className="metric-card">
          <div className="metric-label">Avg Severity Index</div>
          <div className="metric-value accent">{avgScore}/100</div>
        </div>
      </div>

      {/* Controls Bar */}
      <div className="controls-bar">
        <div className="search-box">
          <input
            type="text"
            className="search-input"
            placeholder="Search symptoms, appointment ID, or specialty..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
        <div className="filter-group">
          <select className="filter-select" value={filterUrgency} onChange={(e) => setFilterUrgency(e.target.value)}>
            <option value="ALL">All Urgency Levels</option>
            <option value="EMERGENCY">Emergency</option>
            <option value="HIGH">High</option>
            <option value="MEDIUM">Medium</option>
            <option value="LOW">Low</option>
          </select>
          <select className="filter-select" value={filterSpecialty} onChange={(e) => setFilterSpecialty(e.target.value)}>
            <option value="ALL">All Specialties</option>
            <option value="CARDIOLOGY">Cardiology</option>
            <option value="NEUROLOGY">Neurology</option>
            <option value="DERMATOLOGY">Dermatology</option>
            <option value="ORTHOPEDICS">Orthopedics</option>
          </select>
        </div>
      </div>

      {/* Data Table */}
      <div className="table-container">
        <table className="triage-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Appointment</th>
              <th>Raw Symptoms</th>
              <th>Urgency Level</th>
              <th>Score</th>
              <th>Matched Specialty</th>
              <th>Referral Status</th>
              <th>Action</th>
            </tr>
          </thead>
          <tbody>
            {filteredAssessments.length === 0 ? (
              <tr>
                <td colSpan="8" style={{ textAlign: 'center', padding: '2rem', color: 'var(--text-secondary)' }}>
                  No triage records matched the selected filters.
                </td>
              </tr>
            ) : (
              filteredAssessments.map(row => {
                const status = getReferralStatus(row.id);
                return (
                  <tr key={row.id} onClick={() => setSelectedAssessment(row)}>
                    <td>#{row.id}</td>
                    <td>Appt #{row.appointmentId}</td>
                    <td style={{ maxWidth: '300px', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>
                      {row.rawSymptoms}
                    </td>
                    <td>
                      <span className={getUrgencyBadgeClass(row.urgencyLevel)}>
                        {row.urgencyLevel}
                      </span>
                    </td>
                    <td style={{ fontWeight: 700 }}>{row.urgencyScore}/100</td>
                    <td style={{ color: '#38bdf8', fontWeight: 600 }}>
                      {row.recommendedSpecialty || 'General Medicine'}
                    </td>
                    <td>
                      <span className={`status-badge ${status.toLowerCase()}`}>
                        {status}
                      </span>
                    </td>
                    <td>
                      <button className="btn-action" onClick={(e) => { e.stopPropagation(); setSelectedAssessment(row); }}>
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
