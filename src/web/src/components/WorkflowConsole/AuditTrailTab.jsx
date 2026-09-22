import {useEffect, useState} from 'react';
import {workflowApi} from '../../api/channelCenterApi';
import {formatDate} from './workflowConsoleUtils';

export default function AuditTrailTab() {
    const [logs, setLogs] = useState([]);
    const [error, setError] = useState('');
    useEffect(() => {
        workflowApi.audit().then((result) => setLogs(result.items || result)).catch((requestError) => setError(requestError.message));
    }, []);
    return <div className="workflow-page">
        <header className="page-header">
            <div>
              <p className="eyebrow">Governance / Audit trail</p>
              <h1>Audit trail</h1>
              <p>Every agent handoff and
                administrative decision in one chronological view.</p>
            </div>
        </header>
        <section className="panel audit-panel">
            <div className="panel-heading">
                <div>
                  <h2>Recent activity</h2>
                  <p>Summaries are returned by the secured API.</p>
                </div>
            </div>
            {error ?
                <div className="empty-state error-state"><strong>Unable to load audit logs</strong><span>{error}</span>
                </div> : logs.length === 0 ?
                    <div className="empty-state"><strong>No audit entries</strong><span>The API has not returned any audit activity.</span>
                    </div> :
                    <div className="audit-list">{logs.map((log) =>
                        <div className="audit-row" key={log.id}>
                          <span className="audit-icon">◇</span>
                          <div>
                            <strong>{log.agentName}</strong>
                            <span>{log.toolCalled} · {log.toolOutput}</span>
                          </div>
                          <time>{formatDate(log.createdAt)}</time>
                        </div>)}
                    </div>}
        </section>
    </div>;
}
