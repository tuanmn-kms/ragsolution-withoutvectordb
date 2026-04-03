import { useState } from 'react'
import { AskFeature } from './features/ask/AskFeature'
import { IngestFeature } from './features/ingest/IngestFeature'
import { AppHeader } from './shared/components/AppHeader'
import { FeatureTabs } from './shared/components/FeatureTabs'
import { ASK_API_URL, INGEST_API_URL } from './shared/config/apiConfig'
import './App.css'

function App() {
  const [activeTab, setActiveTab] = useState<'chat' | 'ingest'>('chat')

  return (
    <div className="app-shell">
      <AppHeader
        activeTab={activeTab}
        askApiUrl={ASK_API_URL}
        ingestApiUrl={INGEST_API_URL}
      />

      <FeatureTabs activeTab={activeTab} onTabChange={setActiveTab} />

      <section
        className="feature-panel"
        role="tabpanel"
        id={activeTab === 'chat' ? 'panel-chat' : 'panel-ingest'}
        aria-labelledby={activeTab === 'chat' ? 'tab-chat' : 'tab-ingest'}
      >
        {activeTab === 'chat' ? (
          <AskFeature askApiUrl={ASK_API_URL} />
        ) : (
          <IngestFeature ingestApiUrl={INGEST_API_URL} />
        )}
      </section>
    </div>
  )
}

export default App
