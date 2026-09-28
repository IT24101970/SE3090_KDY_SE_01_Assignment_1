import { useEffect, useState, useMemo } from 'react';
import { workflowApi } from '../../api/channelCenterApi';
import { formatDate } from './workflowConsoleUtils';
import StructuredPayloadViewer from './StructuredPayloadViewer';
import './WorkflowConsole.css';

export default function AuditTrailTab() {
    const [logs, setLogs] = useState([]);
    const [error, setError] = useState('');
    const [loading, setLoading] = useState(true);
    const [search, setSearch] = useState('');
    const [agentFilter, setAgentFilter] = useState('All');

    const fetchAuditLogs = () => {
        setLoading(true);
        setError('');
        workflowApi.audit()
            .then((result) => setLogs(result.items || result))
            .catch((requestError) => setError(requestError.message))
            .finally(() => setLoading(false));
    };

    useEffect(() => {
        fetchAuditLogs();
    }, []);

    const getAgentIcon = (agentName = '') => {
        const name = agentName.toLowerCase();
        if (name.includes('intake')) return '🤖';
        if (name.includes('triage')) return '🩺';
        if (name.includes('safety') || name.includes('auditor')) return '🛡️';
        if (name.includes('doctor') || name.includes('schedul')) return '📋';
        if (name.includes('admin') || name.includes('user')) return '👤';
        return '⚙️';
    };

    const agentOptions = useMemo(() => {
        const set = new Set(logs.map((l) => l.agentName).filter(Boolean));
        return ['All', ...Array.from(set)];
    }, [logs]);

    const filteredLogs = useMemo(() => {
        return logs.filter((log) => {
            const matchesAgent = agentFilter === 'All' || log.agentName === agentFilter;
            const textContent = `${log.agentName || ''} ${log.toolCalled || ''} ${log.toolOutput || ''} ${log.workflowId || ''}`.toLowerCase();
            const matchesSearch = !search || textContent.includes(search.toLowerCase());
            return matchesAgent && matchesSearch;
        });
    }, [logs, agentFilter, search]);

    const stats = useMemo(() => ({
        total: logs.length,
        agents: new Set(logs.map((l) => l.agentName).filter(Boolean)).size,
        tools: new Set(logs.map((l) => l.toolCalled).filter(Boolean)).size,
        latest: logs[0] ? formatDate(logs[0].createdAt) : 'None'
    }), [logs]);

    return (
        <div className="wc-container">
            <header className="wc-header">
                <div className="wc-header-titles">
                    <p className="wc-eyebrow">Governance / Audit trail</p>
                    <h1>Audit trail</h1>
                    <p>Chronological evidence log of every agent execution, tool call, and admin decision.</p>
                </div>
                <div className="wc-header-actions">
                    <button className="wc-btn-sm outline" onClick={fetchAuditLogs} disabled={loading}>
                        🔄 Refresh Logs
                    </button>
                </div>
            </header>

            {/* KPI Stat Cards */}
            <div className="wc-stats-grid">
                <div className="wc-stat-card">
                    <div className="wc-stat-header">
                        <span className="wc-stat-label">Total Events</span>
                        <div className="wc-stat-icon blue">📊</div>
                    </div>
                    <div className="wc-stat-value">{stats.total}</div>
                    <span className="wc-stat-hint">Recorded governance entries</span>
                </div>
                <div className="wc-stat-card">
                    <div className="wc-stat-header">
                        <span className="wc-stat-label">Active Agents</span>
                        <div className="wc-stat-icon success">🤖</div>
                    </div>
                    <div className="wc-stat-value">{stats.agents}</div>
                    <span className="wc-stat-hint">Unique operational agents</span>
                </div>
                <div className="wc-stat-card">
                    <div className="wc-stat-header">
                        <span className="wc-stat-label">Tool Calls</span>
                        <div className="wc-stat-icon amber">⚡</div>
                    </div>
                    <div className="wc-stat-value">{stats.tools}</div>
                    <span className="wc-stat-hint">Distinct tools invoked</span>
                </div>
                <div className="wc-stat-card">
                    <div className="wc-stat-header">
                        <span className="wc-stat-label">Latest Log</span>
                        <div className="wc-stat-icon blue">⏱️</div>
                    </div>
                    <div className="wc-stat-value" style={{ fontSize: '16px', paddingTop: '6px' }}>
                        {stats.latest}
                    </div>
                    <span className="wc-stat-hint">Most recent activity</span>
                </div>
            </div>

            {/* Search & Filter Toolbar */}
            <div className="wc-toolbar">
                <div className="wc-toolbar-left">
                    <div className="wc-search-box">
                        <span className="wc-search-icon">🔍</span>
                        <input
                            type="text"
                            className="wc-search-input"
                            placeholder="Search by agent, tool, workflow ID, or payload content..."
                            value={search}
                            onChange={(e) => setSearch(e.target.value)}
                        />
                    </div>

                    <select
                        className="wc-select"
                        value={agentFilter}
                        onChange={(e) => setAgentFilter(e.target.value)}
                    >
                        {agentOptions.map((opt) => (
                            <option key={opt} value={opt}>
                                {opt === 'All' ? 'All Agents' : `Agent: ${opt}`}
                            </option>
                        ))}
                    </select>
                </div>
            </div>

            {/* Audit Logs Content */}
            <section className="wc-card">
                <div className="wc-card-header">
                    <div>
                        <h2>Activity log stream</h2>
                        <p>Showing {filteredLogs.length} of {logs.length} total audit entries.</p>
                    </div>
                </div>

                {loading ? (
                    <div className="wc-loading-box">
                        <div className="wc-spinner" />
                        <h3>Loading governance logs…</h3>
                        <p>Fetching detailed execution trail from backend API.</p>
                    </div>
                ) : error ? (
                    <div className="wc-empty-state">
                        <div className="icon">⚠️</div>
                        <h3>Unable to load audit logs</h3>
                        <p>{error}</p>
                        <button className="wc-btn-sm primary" onClick={fetchAuditLogs}>Retry</button>
                    </div>
                ) : filteredLogs.length === 0 ? (
                    <div className="wc-empty-state">
                        <div className="icon">🔍</div>
                        <h3>No matching audit entries</h3>
                        <p>No audit activity matches your current search or agent filter criteria.</p>
                    </div>
                ) : (
                    <div style={{ padding: '20px' }}>
                        {filteredLogs.map((log) => (
                            <div className="wc-audit-card" key={log.id}>
                                <div className="wc-audit-card-header">
                                    <div className="wc-audit-agent-tag">
                                        <span className="wc-audit-agent-icon">
                                            {getAgentIcon(log.agentName)}
                                        </span>
                                        <span>{log.agentName || 'System'}</span>
                                    </div>
                                    <div className="wc-audit-meta-right">
                                        {log.toolCalled && (
                                            <span className="wc-audit-tool-badge">
                                                ⚡ {log.toolCalled}
                                            </span>
                                        )}
                                        {log.workflowId && (
                                            <span className="wc-audit-wf-link">
                                                WF-{log.workflowId}
                                            </span>
                                        )}
                                        <time className="wc-stat-hint">{formatDate(log.createdAt)}</time>
                                    </div>
                                </div>

                                <StructuredPayloadViewer
                                    payload={log.toolOutput}
                                    title={log.toolCalled ? `Tool Output (${log.toolCalled})` : 'Execution Output'}
                                    collapsible={true}
                                    defaultExpanded={false}
                                />
                            </div>
                        ))}
                    </div>
                )}
            </section>
        </div>
    );
}
