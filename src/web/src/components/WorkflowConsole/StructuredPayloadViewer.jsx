import { useState } from 'react';
import './WorkflowConsole.css';

export default function StructuredPayloadViewer({
    payload,
    title,
    collapsible = false,
    defaultExpanded = true
}) {
    const [isExpanded, setIsExpanded] = useState(defaultExpanded);
    const [showRaw, setShowRaw] = useState(false);

    if (payload === null || payload === undefined || payload === '') {
        return <span className="wc-payload-empty">No payload output</span>;
    }

    let parsed = null;
    let isJson = false;

    if (typeof payload === 'object') {
        parsed = payload;
        isJson = true;
    } else if (typeof payload === 'string') {
        const trimmed = payload.trim();
        if ((trimmed.startsWith('{') && trimmed.endsWith('}')) || (trimmed.startsWith('[') && trimmed.endsWith(']'))) {
            try {
                parsed = JSON.parse(trimmed);
                isJson = true;
            } catch (e) {
                isJson = false;
            }
        }
    }

    const formatKey = (key) => {
        return key
            .replace(/([A-Z])/g, ' $1')
            .replace(/^./, (str) => str.toUpperCase())
            .replace(/_/g, ' ');
    };

    const renderValue = (val) => {
        if (val === null || val === undefined) return <span className="wc-payload-null">null</span>;
        if (typeof val === 'boolean') {
            return (
                <span className={`wc-payload-pill ${val ? 'success' : 'neutral'}`}>
                    {val ? '✓ True' : '✗ False'}
                </span>
            );
        }
        if (typeof val === 'number') {
            return <strong className="wc-payload-number">{val}</strong>;
        }
        if (typeof val === 'string') {
            const lower = val.toLowerCase();
            if (['high', 'critical', 'urgent', 'safefailed', 'rejected'].includes(lower)) {
                return <span className="wc-payload-pill danger">{val}</span>;
            }
            if (['medium', 'moderate', 'pausedforapproval', 'paused', 'revised'].includes(lower)) {
                return <span className="wc-payload-pill warning">{val}</span>;
            }
            if (['low', 'completed', 'approved', 'safe'].includes(lower)) {
                return <span className="wc-payload-pill success">{val}</span>;
            }
            return <span className="wc-payload-text">{val}</span>;
        }
        if (typeof val === 'object') {
            return <code className="wc-payload-code-inline">{JSON.stringify(val)}</code>;
        }
        return String(val);
    };

    if (!isJson || !parsed) {
        if (collapsible) {
            return (
                <div className="wc-payload-dropdown-container">
                    <button
                        type="button"
                        className="wc-payload-dropdown-btn"
                        onClick={() => setIsExpanded(!isExpanded)}
                    >
                        <span className="wc-payload-dropdown-icon">{isExpanded ? '▼' : '▶'}</span>
                        <span className="wc-payload-dropdown-title">
                            {title || 'Execution Output'}
                        </span>
                    </button>
                    {isExpanded && (
                        <div className="wc-payload-box raw" style={{ marginTop: '8px' }}>
                            <p className="wc-payload-raw-text">{String(payload)}</p>
                        </div>
                    )}
                </div>
            );
        }
        return (
            <div className="wc-payload-box raw">
                <p className="wc-payload-raw-text">{String(payload)}</p>
            </div>
        );
    }

    const rawJsonString = JSON.stringify(parsed, null, 2);
    const isArray = Array.isArray(parsed);
    const entries = isArray ? null : Object.entries(parsed);
    const itemCount = isArray ? parsed.length : (entries ? entries.length : 0);

    return (
        <div className="wc-payload-container">
            {collapsible ? (
                <div className="wc-payload-dropdown-header">
                    <button
                        type="button"
                        className="wc-payload-dropdown-btn"
                        onClick={() => setIsExpanded(!isExpanded)}
                    >
                        <span className="wc-payload-dropdown-icon">{isExpanded ? '▼' : '▶'}</span>
                        <span className="wc-payload-dropdown-title">
                            {title || 'Execution Payload'}
                        </span>
                        <span className="wc-payload-count-pill">{itemCount} fields</span>
                    </button>

                    {isExpanded && (
                        <button
                            type="button"
                            className="wc-payload-toggle-btn"
                            onClick={() => setShowRaw(!showRaw)}
                        >
                            {showRaw ? '📋 List View' : '{ } JSON'}
                        </button>
                    )}
                </div>
            ) : (
                <div className="wc-payload-header">
                    {title && <span className="wc-payload-title">Payload: {title}</span>}
                    <button
                        type="button"
                        className="wc-payload-toggle-btn"
                        onClick={() => setShowRaw(!showRaw)}
                    >
                        {showRaw ? '📋 List View' : '{ } JSON'}
                    </button>
                </div>
            )}

            {isExpanded && (
                <div className="wc-payload-body" style={{ marginTop: collapsible ? '10px' : '6px' }}>
                    {showRaw ? (
                        <pre className="wc-payload-json-block">{rawJsonString}</pre>
                    ) : isArray ? (
                        <ul className="wc-payload-ul">
                            {parsed.map((item, idx) => (
                                <li key={idx} className="wc-payload-li">
                                    <span className="wc-payload-item-label">Item #{idx + 1}:</span>
                                    <div className="wc-payload-item-val">
                                        {typeof item === 'object' ? (
                                            <StructuredPayloadViewer payload={item} collapsible={false} defaultExpanded={true} />
                                        ) : (
                                            renderValue(item)
                                        )}
                                    </div>
                                </li>
                            ))}
                        </ul>
                    ) : entries.length === 0 ? (
                        <span className="wc-payload-empty">Empty payload {"{}"}</span>
                    ) : (
                        <ul className="wc-payload-ul">
                            {entries.map(([key, val]) => (
                                <li key={key} className="wc-payload-li">
                                    <span className="wc-payload-item-label">{formatKey(key)}:</span>
                                    <span className="wc-payload-item-val">{renderValue(val)}</span>
                                </li>
                            ))}
                        </ul>
                    )}
                </div>
            )}
        </div>
    );
}