import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { ingestDocument } from '../../../shared/api/ragApi'

type UseIngestFeatureParams = {
  ingestApiUrl: string
}

export function useIngestFeature({ ingestApiUrl }: UseIngestFeatureParams) {
  const [ingestTitle, setIngestTitle] = useState('')
  const [ingestPath, setIngestPath] = useState('')
  const [ingestContent, setIngestContent] = useState('')
  const [isIngesting, setIsIngesting] = useState(false)
  const [ingestResult, setIngestResult] = useState<{
    status: 'success' | 'error'
    message: string
  } | null>(null)

  const trimmedIngestContent = useMemo(
    () => ingestContent.trim(),
    [ingestContent],
  )
  const trimmedIngestTitle = useMemo(() => ingestTitle.trim(), [ingestTitle])
  const trimmedIngestPath = useMemo(() => ingestPath.trim(), [ingestPath])

  const submitIngest = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!trimmedIngestContent || isIngesting) {
      return
    }

    setIsIngesting(true)
    setIngestResult(null)

    try {
      const ingestId =
        trimmedIngestPath ||
        trimmedIngestTitle ||
        `doc-${new Date().toISOString()}-${crypto.randomUUID()}`

      const result = await ingestDocument(ingestApiUrl, {
        id: ingestId,
        title: trimmedIngestTitle || ingestId,
        content: trimmedIngestContent,
        metadata: trimmedIngestPath
          ? {
              path: trimmedIngestPath,
            }
          : undefined,
      })

      setIngestResult({
        status: 'success',
        message:
          result.message ||
          `Document '${ingestId}' ingested successfully! You can now ask questions about it.`,
      })
      setIngestTitle('')
      setIngestPath('')
      setIngestContent('')
    } catch (error) {
      setIngestResult({
        status: 'error',
        message:
          'Unable to ingest document. ' +
          (error instanceof Error
            ? error.message
            : 'Verify your request and backend availability.'),
      })
    } finally {
      setIsIngesting(false)
    }
  }

  const resetIngestForm = () => {
    setIngestTitle('')
    setIngestPath('')
    setIngestContent('')
    setIngestResult(null)
  }

  return {
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
  }
}
