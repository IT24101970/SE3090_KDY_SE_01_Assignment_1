import {useEffect, useState} from 'react';
import {workflowApi} from '../../api/channelCenterApi';
import {formatDate} from './workflowConsoleUtils';
import './WorkflowConsole.css';

export default function AuditTrailTab() {
    const [logs, setLogs] = useState([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        setLoading(true);
        workflowApi.audit()
            .then((result) => setLogs(result.items || result))
            .catch((requestError) => setError(requestError.message))
            .finally(() => setLoading(false));
    }, []);

    return (
        <div className="wc-container">
            <header className="wc-header">
                <div className="wc-header-titles">
                    <p className="wc-eyebrow">Governance / Audit trail</p>
                    <h1>Audit trail</h1>
                    <p>Every agent handoff and administrative decision in one chronological view.</p>
                </div>
            </header>

            <section className="wc-card">
                <div className="wc-card-header">
                    <div>
                        <h2>Recent activity</h2>
                        <p>Summaries are returned by the secured API.</p>
                    </div>
                </div>

                {loading ? (
                    <div className="wc-loading-box">
                        <div className="wc-spinner"></div>
                        <h3>Loading audit logs…</h3>
                        <p>Fetching governance logs from API.</p>
                    </div>
                ) : error ? (
                    <div className="wc-empty-state">
                        <div className="icon">⚠️</div>
                        <h3>Unable to load audit logs</h3>
                        <p>{error}</p>
                    </div>
                ) : logs.length === 0 ? (
                    <div className="wc-empty-state">
                        <div className="icon">🔍</div>
                        <h3>No audit entries</h3>
                        <p>The API has not returned any audit activity.</p>
                    </div>
                ) : (
                    <div className="wc-audit-list">
                        {logs.map((log) => (
                            <div className="wc-audit-row" key={log.id}>
                                <span className="wc-audit-icon">◇</span>
                                <div className="wc-audit-details">
                                    <strong>{log.agentName}</strong>
                                    <span>{log.toolCalled} · {log.toolOutput}</span>
                                </div>
                                <time className="wc-audit-time">{formatDate(log.createdAt)}</time>
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}
