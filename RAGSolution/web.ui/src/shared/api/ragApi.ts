import type {
  AskQuestionRequest,
  IngestDocumentRequest,
  IngestResponse,
  RagResponse,
} from '../../types'

type JsonHeaders = {
  'Content-Type': 'application/json'
}

const JSON_HEADERS: JsonHeaders = {
  'Content-Type': 'application/json',
}

export async function askQuestion(
  apiUrl: string,
  request: AskQuestionRequest,
): Promise<RagResponse> {
  const response = await fetch(apiUrl, {
    method: 'POST',
    headers: JSON_HEADERS,
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(`API returned ${response.status}`)
  }

  return (await response.json()) as RagResponse
}

export async function ingestDocument(
  apiUrl: string,
  request: IngestDocumentRequest,
): Promise<IngestResponse> {
  const response = await fetch(apiUrl, {
    method: 'POST',
    headers: JSON_HEADERS,
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    const errorText = await response.text()
    throw new Error(`API returned ${response.status}: ${errorText}`)
  }

  return (await response.json()) as IngestResponse
}
