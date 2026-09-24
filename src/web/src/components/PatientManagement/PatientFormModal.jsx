import { useState } from 'react';
import { patientApi } from '../../api/channelCenterApi';

const defaultFormData = {
  name: '',
  nic: '',
  dateOfBirth: '',
  gender: 'Male',
  phoneNumber: '',
  email: '',
  emergencyContact: '',
  bloodGroup: 'O+',
  allergies: '',
  medicalHistory: '',
};

function getInitialFormData(patient) {
  if (!patient) return defaultFormData;
  return {
    name: patient.name || '',
    nic: patient.nic || '',
    dateOfBirth: patient.dateOfBirth ? patient.dateOfBirth.split('T')[0] : '',
    gender: patient.gender || 'Male',
    phoneNumber: patient.phoneNumber || '',
    email: patient.email || '',
    emergencyContact: patient.emergencyContact || '',
    bloodGroup: patient.bloodGroup || 'O+',
    allergies: patient.allergies || '',
    medicalHistory: patient.medicalHistory || '',
  };
}

export default function PatientFormModal({ patient, isOpen, onClose, onSaved }) {
  const isEdit = Boolean(patient && patient.id);

  const [prevPatient, setPrevPatient] = useState(patient);
  const [formData, setFormData] = useState(() => getInitialFormData(patient));
  const [errors, setErrors] = useState({});
  const [submitting, setSubmitting] = useState(false);
  const [apiError, setApiError] = useState('');

  if (patient !== prevPatient) {
    setPrevPatient(patient);
    setFormData(getInitialFormData(patient));
    setErrors({});
    setApiError('');
  }

  if (!isOpen) return null;

  const validate = () => {
    const errs = {};
    if (!formData.name.trim() || formData.name.trim().length < 2) {
      errs.name = 'Patient name must be at least 2 characters.';
    }
    if (!isEdit && (!formData.nic.trim() || formData.nic.trim().length < 6)) {
      errs.nic = 'Valid National Identity Card (NIC) number is required.';
    }
    if (!formData.dateOfBirth) {
      errs.dateOfBirth = 'Date of birth is required.';
    } else {
      const dob = new Date(formData.dateOfBirth);
      if (dob > new Date()) {
        errs.dateOfBirth = 'Date of birth cannot be in the future.';
      }
    }
    if (!formData.phoneNumber.trim()) {
      errs.phoneNumber = 'Phone number is required.';
    } else if (!/^[0-9+()-\s]{7,20}$/.test(formData.phoneNumber.trim())) {
      errs.phoneNumber = 'Invalid phone number format.';
    }
    if (!formData.email.trim()) {
      errs.email = 'Email address is required.';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(formData.email.trim())) {
      errs.email = 'Invalid email address format.';
    }
    if (!formData.emergencyContact.trim()) {
      errs.emergencyContact = 'Emergency contact details are required.';
    }
    setErrors(errs);
    return Object.keys(errs).length === 0;
  };

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setApiError('');
    if (!validate()) return;

    setSubmitting(true);
    try {
      if (isEdit) {
        const updatePayload = {
          name: formData.name.trim(),
          emergencyContact: formData.emergencyContact.trim(),
          gender: formData.gender,
          phoneNumber: formData.phoneNumber.trim(),
          email: formData.email.trim(),
          bloodGroup: formData.bloodGroup,
          allergies: formData.allergies ? formData.allergies.trim() : null,
          medicalHistory: formData.medicalHistory ? formData.medicalHistory.trim() : null,
        };
        const updated = await patientApi.update(patient.id, updatePayload);
        onSaved(updated, 'Patient record successfully updated.');
      } else {
        const createPayload = {
          name: formData.name.trim(),
          nic: formData.nic.trim(),
          dateOfBirth: new Date(formData.dateOfBirth).toISOString(),
          gender: formData.gender,
          phoneNumber: formData.phoneNumber.trim(),
          email: formData.email.trim(),
          emergencyContact: formData.emergencyContact.trim(),
          bloodGroup: formData.bloodGroup,
          allergies: formData.allergies ? formData.allergies.trim() : null,
          medicalHistory: formData.medicalHistory ? formData.medicalHistory.trim() : null,
        };
        const created = await patientApi.create(createPayload);
        onSaved(created, 'New patient successfully registered.');
      }
      onClose();
    } catch (err) {
      setApiError(err.message || 'Failed to save patient profile.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="pm-modal-backdrop" onClick={onClose} role="dialog" aria-modal="true">
      <div className="pm-modal-card" onClick={(e) => e.stopPropagation()}>
        <div className="pm-modal-header">
          <div>
            <span className="eyebrow">{isEdit ? 'Patient Profile Update' : 'New Patient Onboarding'}</span>
            <h2>{isEdit ? `Edit Profile — ${patient.name}` : 'Register New Patient'}</h2>
          </div>
          <button className="pm-modal-close" onClick={onClose} aria-label="Close dialog">×</button>
        </div>

        <form onSubmit={handleSubmit}>
          <div className="pm-modal-body">
            {apiError && (
              <div className="pm-alert error" role="alert">
                <span>⚠️</span> <span>{apiError}</span>
              </div>
            )}

            <div className="pm-form-grid">
              <div className="pm-form-group full-width">
                <label className="pm-label" htmlFor="patient-name">
                  Full Legal Name <span className="required">*</span>
                </label>
                <input
                  id="patient-name"
                  name="name"
                  type="text"
                  className={`pm-input ${errors.name ? 'error' : ''}`}
                  placeholder="e.g. Eleanor Vance"
                  value={formData.name}
                  onChange={handleChange}
                  disabled={submitting}
                />
                {errors.name && <span className="pm-field-error">{errors.name}</span>}
              </div>

              <div className="pm-form-group">
                <label className="pm-label" htmlFor="patient-nic">
                  NIC / National ID {!isEdit && <span className="required">*</span>}
                </label>
                <input
                  id="patient-nic"
                  name="nic"
                  type="text"
                  className={`pm-input ${errors.nic ? 'error' : ''}`}
                  placeholder="e.g. 199012345678 or 901234567V"
                  value={formData.nic}
                  onChange={handleChange}
                  disabled={isEdit || submitting}
                  title={isEdit ? 'NIC cannot be modified after registration' : ''}
                />
                {errors.nic && <span className="pm-field-error">{errors.nic}</span>}
              </div>

              <div className="pm-form-group">
                <label className="pm-label" htmlFor="patient-dob">
                  Date of Birth <span className="required">*</span>
                </label>
                <input
                  id="patient-dob"
                  name="dateOfBirth"
                  type="date"
                  className={`pm-input ${errors.dateOfBirth ? 'error' : ''}`}
                  value={formData.dateOfBirth}
                  onChange={handleChange}
                  disabled={isEdit || submitting}
                  title={isEdit ? 'Date of birth is immutable' : ''}
                />
                {errors.dateOfBirth && <span className="pm-field-error">{errors.dateOfBirth}</span>}
              </div>

              <div className="pm-form-group">
                <label className="pm-label" htmlFor="patient-gender">
                  Gender <span className="required">*</span>
                </label>
                <select
                  id="patient-gender"
                  name="gender"
                  className="pm-select-field"
                  value={formData.gender}
                  onChange={handleChange}
                  disabled={submitting}
                >
                  <option value="Male">Male</option>
                  <option value="Female">Female</option>
                  <option value="Other">Other</option>
                </select>
              </div>

              <div className="pm-form-group">
                <label className="pm-label" htmlFor="patient-bloodgroup">Blood Group</label>
                <select
                  id="patient-bloodgroup"
                  name="bloodGroup"
                  className="pm-select-field"
                  value={formData.bloodGroup}
                  onChange={handleChange}
                  disabled={submitting}
                >
                  <option value="O+">O+</option>
                  <option value="O-">O-</option>
                  <option value="A+">A+</option>
                  <option value="A-">A-</option>
                  <option value="B+">B+</option>
                  <option value="B-">B-</option>
                  <option value="AB+">AB+</option>
                  <option value="AB-">AB-</option>
                  <option value="Unknown">Unknown</option>
                </select>
              </div>

              <div className="pm-form-group">
                <label className="pm-label" htmlFor="patient-phone">
                  Phone Number <span className="required">*</span>
                </label>
                <input
                  id="patient-phone"
                  name="phoneNumber"
                  type="tel"
                  className={`pm-input ${errors.phoneNumber ? 'error' : ''}`}
                  placeholder="0771234567"
                  value={formData.phoneNumber}
                  onChange={handleChange}
                  disabled={submitting}
                />
                {errors.phoneNumber && <span className="pm-field-error">{errors.phoneNumber}</span>}
              </div>

              <div className="pm-form-group">
                <label className="pm-label" htmlFor="patient-email">
                  Email Address <span className="required">*</span>
                </label>
                <input
                  id="patient-email"
                  name="email"
                  type="email"
                  className={`pm-input ${errors.email ? 'error' : ''}`}
                  placeholder="patient@example.com"
                  value={formData.email}
                  onChange={handleChange}
                  disabled={submitting}
                />
                {errors.email && <span className="pm-field-error">{errors.email}</span>}
              </div>

              <div className="pm-form-group full-width">
                <label className="pm-label" htmlFor="patient-emergency">
                  Emergency Contact <span className="required">*</span>
                </label>
                <input
                  id="patient-emergency"
                  name="emergencyContact"
                  type="text"
                  className={`pm-input ${errors.emergencyContact ? 'error' : ''}`}
                  placeholder="Name & Contact (e.g. Jane Doe - 0779998877)"
                  value={formData.emergencyContact}
                  onChange={handleChange}
                  disabled={submitting}
                />
                {errors.emergencyContact && <span className="pm-field-error">{errors.emergencyContact}</span>}
              </div>

              <div className="pm-form-group full-width">
                <label className="pm-label" htmlFor="patient-allergies">
                  Known Allergies
                </label>
                <textarea
                  id="patient-allergies"
                  name="allergies"
                  rows={2}
                  className="pm-textarea"
                  placeholder="e.g. Penicillin, Peanuts, Aspirin (Leave blank if none)"
                  value={formData.allergies}
                  onChange={handleChange}
                  disabled={submitting}
                />
              </div>

              <div className="pm-form-group full-width">
                <label className="pm-label" htmlFor="patient-medhistory">
                  Medical History & Pre-existing Conditions
                </label>
                <textarea
                  id="patient-medhistory"
                  name="medicalHistory"
                  rows={2}
                  className="pm-textarea"
                  placeholder="e.g. Type 2 Diabetes, Hypertension, Coronary Stent in 2021"
                  value={formData.medicalHistory}
                  onChange={handleChange}
                  disabled={submitting}
                />
              </div>
            </div>
          </div>

          <div className="pm-modal-footer">
            <button
              type="button"
              className="outline-button"
              onClick={onClose}
              disabled={submitting}
            >
              Cancel
            </button>
            <button
              type="submit"
              className="primary-button"
              disabled={submitting}
            >
              {submitting ? 'Saving...' : isEdit ? 'Update Profile' : 'Complete Registration'}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
