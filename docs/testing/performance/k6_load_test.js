import http from 'k6/http';
import { check, sleep } from 'k6';

// Performance & Load Testing Configuration (SE3110 Non-Functional Testing)
export const options = {
  stages: [
    { duration: '10s', target: 10 }, // Ramp-up to 10 users
    { duration: '30s', target: 50 }, // Sustained load of 50 concurrent virtual users
    { duration: '10s', target: 0 },  // Ramp-down to 0
  ],
  thresholds: {
    http_req_duration: ['p(95)<300'], // 95% of requests must complete within 300ms
    http_req_failed: ['rate<0.01'],    // Error rate must be less than 1%
  },
};

const BASE_URL = __ENV.BASE_URL || 'http://localhost:5000/api';

export default function () {
  // 1. Health check & Doctor Schedules lookup
  const scheduleRes = http.get(`${BASE_URL}/appointments?upcomingOnly=true`);
  check(scheduleRes, {
    'Schedules status is 200': (r) => r.status === 200,
    'Response time < 250ms': (r) => r.timings.duration < 250,
  });

  sleep(1);

  // 2. Patient Directory search under load
  const patientRes = http.get(`${BASE_URL}/patients?pageSize=10`);
  check(patientRes, {
    'Patients status is 200': (r) => r.status === 200,
  });

  sleep(1);

  // 3. AI Triage endpoint load check
  const triagePayload = JSON.stringify({
    patientId: 1,
    symptomsText: "I have had severe migraine and high fever for 2 days",
    age: 34,
    gender: "Female"
  });

  const headers = { 'Content-Type': 'application/json' };
  const triageRes = http.post(`${BASE_URL}/triage/assess`, triagePayload, { headers });
  check(triageRes, {
    'Triage assessment status is 200 or 401': (r) => r.status === 200 || r.status === 401,
  });

  sleep(2);
}
