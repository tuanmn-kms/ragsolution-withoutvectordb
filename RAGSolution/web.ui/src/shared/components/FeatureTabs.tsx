type FeatureTabsProps = {
  activeTab: 'chat' | 'ingest'
  onTabChange: (tab: 'chat' | 'ingest') => void
}

export function FeatureTabs({ activeTab, onTabChange }: FeatureTabsProps) {
  return (
    <nav className="tabs" aria-label="Feature tabs" role="tablist">
      <button
        type="button"
        role="tab"
        id="tab-chat"
        aria-controls="panel-chat"
        aria-selected={activeTab === 'chat'}
        className={`tab-button ${activeTab === 'chat' ? 'active' : ''}`}
        onClick={() => onTabChange('chat')}
      >
        Chat
      </button>
      <button
        type="button"
        role="tab"
        id="tab-ingest"
        aria-controls="panel-ingest"
        aria-selected={activeTab === 'ingest'}
        className={`tab-button ${activeTab === 'ingest' ? 'active' : ''}`}
        onClick={() => onTabChange('ingest')}
      >
        Ingest
      </button>
    </nav>
  )
}
