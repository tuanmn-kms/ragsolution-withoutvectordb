import { useIngestFeature } from './hooks/useIngestFeature'

type IngestFeatureProps = {
  ingestApiUrl: string
}

const MAX_INGEST_CONTENT_LENGTH = 25000

export function IngestFeature({ ingestApiUrl }: IngestFeatureProps) {
  const {
    ingestTitle,
    setIngestTitle,
    ingestPath,
    setIngestPath,
    ingestContent,
    setIngestContent,
    isIngesting,
    ingestResult,
    trimmedIngestContent,
    submitIngest,
    resetIngestForm,
  } = useIngestFeature({ ingestApiUrl })

  return (
    <section className="ingest-card" aria-label="Ingest document">
      <h2>Ingest Document</h2>
      <p>
        Store document content into SQL-backed RAG so it can be used by ask
        endpoint.
      </p>
      <form className="ingest-form" onSubmit={submitIngest}>
        <div className="ingest-grid">
          <label>
            Title (optional)
            <input
              type="text"
              value={ingestTitle}
              onChange={(event) => setIngestTitle(event.target.value)}
              placeholder="Architecture Overview"
              disabled={isIngesting}
            />
          </label>
          <label>
            Path or source (optional)
            <input
              type="text"
              value={ingestPath}
              onChange={(event) => setIngestPath(event.target.value)}
              placeholder="docs/architecture.md"
              disabled={isIngesting}
            />
          </label>
        </div>

        <label>
          Content
          <textarea
            value={ingestContent}
            maxLength={MAX_INGEST_CONTENT_LENGTH}
            onChange={(event) => setIngestContent(event.target.value)}
            placeholder="Paste document text to ingest..."
            disabled={isIngesting}
          />
        </label>

        <div className="ingest-footer">
          <span>
            {ingestContent.length}/{MAX_INGEST_CONTENT_LENGTH}
          </span>
          <div className="actions">
            <button
              type="button"
              onClick={resetIngestForm}
              disabled={isIngesting || (!ingestTitle && !ingestPath && !ingestContent)}
            >
              Reset
            </button>
            <button
              type="submit"
              disabled={!trimmedIngestContent || isIngesting}
            >
              {isIngesting ? 'Ingesting...' : 'Ingest'}
            </button>
          </div>
        </div>

        {ingestResult && (
          <p className={`ingest-status ${ingestResult.status}`}>
            {ingestResult.message}
          </p>
        )}
      </form>
    </section>
  )
}
