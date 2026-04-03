/**
 * Type definitions for RAG API
 * Matches the C# models in Web.API
 */

/**
 * Request model for asking questions to the RAG system
 * @see Web.API.Models.Rag.AskQuestionRequest
 */
export interface AskQuestionRequest {
  /** The question to ask the RAG system */
  question: string;
  /** Number of top relevant documents to retrieve (1-20, default: 3) */
  topK?: number;
  /** Maximum number of sentences in the generated answer (1-10, default: 3) */
  maxAnswerSentences?: number;
  /** Optional model selection for generating the answer */
  model?: string;
}

/**
 * Request model for ingesting a document into the RAG system
 * @see Web.API.Models.Rag.IngestDocumentRequest
 */
export interface IngestDocumentRequest {
  /** Unique identifier for the document */
  id: string;
  /** Title or name of the document */
  title: string;
  /** Full text content of the document to be indexed */
  content: string;
  /** Optional metadata key-value pairs for the document */
  metadata?: Record<string, string>;
}

/**
 * Source document information with relevance score
 * @see Web.API.Models.Rag.RagSource
 */
export interface RagSource {
  /** Unique identifier of the source document */
  documentId: string;
  /** Title of the source document */
  title: string;
  /** Relevant snippet/excerpt from the document */
  snippet: string;
  /** Relevance score (0.0-1.0, higher is more relevant) */
  score: number;
}

/**
 * Response model containing the AI-generated answer and source documents
 * @see Web.API.Models.Rag.RagResponse
 */
export interface RagResponse {
  /** The original question asked */
  question: string;
  /** AI-generated answer based on retrieved documents */
  answer: string;
  /** List of source documents used to generate the answer */
  sources: RagSource[];
}

/**
 * Response from document ingestion
 */
export interface IngestResponse {
  /** Success message */
  message?: string;
}

/**
 * Health check response
 * @see Web.API.Services.Rag.RagDatabaseHealth
 */
export interface RagDatabaseHealth {
  isHealthy: boolean;
  server: string;
  database: string;
  message: string;
}

/**
 * Chat message for UI display
 */
export interface ChatMessage {
  id: string;
  question: string;
  answer: string;
  createdAt: string;
  sources: RagSource[];
  isError?: boolean;
}

/**
 * Configuration from seed documents
 */
export interface SeedDocumentOptions {
  id: string;
  title: string;
  content: string;
  metadata?: Record<string, string>;
}
