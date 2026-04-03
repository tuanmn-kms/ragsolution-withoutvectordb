type AppHeaderProps = {
  activeTab: 'chat' | 'ingest'
  askApiUrl: string
  ingestApiUrl: string
}

export function AppHeader({ activeTab, askApiUrl, ingestApiUrl }: AppHeaderProps) {
  return (
    <header className="app-header">
      <div>
        <p className="badge">RAG UI • AI-Powered</p>
        <h1>Question & Answer</h1>
        <p className="subtitle">
          Retrieval-Augmented Generation with OpenAI • Lower cost, simpler stack
        </p>
      </div>
      <div className="meta-card">
        <span className="meta-label">Active Endpoint</span>
        <strong className="endpoint-value">
          {activeTab === 'chat' ? askApiUrl : ingestApiUrl}
        </strong>
        <small>{activeTab === 'chat' ? 'Chat API' : 'Ingest API'}</small>
      </div>
    </header>
  )
}
