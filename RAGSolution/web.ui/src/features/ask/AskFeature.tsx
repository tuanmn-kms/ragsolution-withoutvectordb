import { useEffect, useRef, useState } from "react";
import { useAskFeature } from "./hooks/useAskFeature";

type AskFeatureProps = {
  askApiUrl: string;
};

const MAX_QUESTION_LENGTH = 1200;

export function AskFeature({ askApiUrl }: AskFeatureProps) {
  const formRef = useRef<HTMLFormElement>(null);
  const latestMessageRef = useRef<HTMLElement | null>(null);
  const previousMessagesCountRef = useRef(0);
  const containerRef = useRef<HTMLDivElement | null>(null);
  const endRef = useRef<HTMLDivElement | null>(null);

  const [autoFollow, setAutoFollow] = useState(true);
  const [isFormOpen, setIsFormOpen] = useState(true);

  const {
    question,
    setQuestion,
    messages,
    isLoading,
    showAdvancedSettings,
    setShowAdvancedSettings,
    topK,
    setTopK,
    maxAnswerSentences,
    setMaxAnswerSentences,
    selectedModel,
    setSelectedModel,
    showSources,
    setShowSources,
    trimmedQuestion,
    submitQuestion,
    setSampleQuestion,
  } = useAskFeature({ askApiUrl });

  useEffect(() => {
    const currentMessagesCount = messages.length;

    if (currentMessagesCount > previousMessagesCountRef.current) {
      latestMessageRef.current?.scrollIntoView({
        behavior: "smooth",
        block: "end",
      });
    }

    previousMessagesCountRef.current = currentMessagesCount;
  }, [messages]);

  useEffect(() => {
    const onScroll = () => {
      const el = containerRef.current;
      if (!el) return;
      const nearBottom = el.scrollHeight - el.scrollTop - el.clientHeight < 40;
      setAutoFollow(nearBottom);
    };

    onScroll();
    window.addEventListener("scroll", onScroll);

    return () => window.removeEventListener("scroll", onScroll);
  }, [messages, autoFollow]);

  useEffect(() => {
    if (autoFollow) {
      endRef.current?.scrollIntoView({ behavior: "smooth", block: "end" });
    }
  }, [messages, autoFollow]);

  return (
    <>
      <section className="settings-card" aria-label="Search settings">
        <div className="settings-header-row">
          <h2>Search Settings</h2>
          <button
            type="button"
            className="settings-toggle"
            onClick={() => setShowAdvancedSettings((previous) => !previous)}
            disabled={isLoading}
            aria-expanded={showAdvancedSettings}
            aria-controls="advanced-search-settings"
          >
            {showAdvancedSettings ? "Hide advanced" : "Show advanced"}
          </button>
        </div>
        {showAdvancedSettings && (
          <div className="settings-grid">
            <label>
              <span className="label-text">
                Top K Documents
                <span className="help-text">
                  Number of relevant documents to retrieve (1-20)
                </span>
              </span>
              <input
                type="number"
                min="1"
                max="20"
                value={topK}
                onChange={(event) => setTopK(Number(event.target.value))}
                disabled={isLoading}
              />
            </label>
            <label id="advanced-search-settings">
              <span className="label-text">
                Max Answer Sentences
                <span className="help-text">
                  Maximum sentences in AI response (1-10)
                </span>
              </span>
              <input
                type="number"
                min="1"
                max="10"
                value={maxAnswerSentences}
                onChange={(event) =>
                  setMaxAnswerSentences(Number(event.target.value))
                }
                disabled={isLoading}
              />
            </label>
            <label id="selected-model-settings">
              <span className="label-text">
                Model Selection
                <span className="help-text">
                  Choose the AI model for generating responses
                </span>
              </span>
              <select
                value={selectedModel}
                onChange={(event) => setSelectedModel(event.target.value)}
                disabled={isLoading}
              >
                <option value="gpt-4.1-2025-04-14">gpt-4.1</option>
                <option value="gpt-5.4-2026-03-05">gpt-5.4</option>
                <option value="gpt-3.5-turbo-0125">gpt-3.5-turbo</option>
              </select>
            </label>
            <label id="show-sources-settings">
              <span className="label-text">
                Check to show sources in responses
                <span className="help-text">
                  Toggle to display source references in AI responses
                </span>
              </span>
             <input type="checkbox" checked={showSources} onChange={(event) => setShowSources(event.target.checked)} disabled={isLoading} />
            </label>
          </div>
        )}
      </section>

      <section className="samples" aria-label="Sample questions">
        <button
          type="button"
          onClick={() => setSampleQuestion("What is KMS policy?")}
        >
          What is KMS policy?
        </button>
        <button
          type="button"
          onClick={() =>
            setSampleQuestion("What is the assistant's name?")
          }
        >
          What is the assistant's name?
        </button>
        <button
          type="button"
          onClick={() =>
            setSampleQuestion("What is this RAG project about?")
          }
        >
          This RAG project information
        </button>
      </section>

      <main className="chat-region" aria-live="polite">
        {messages.length === 0 ? (
          <div className="empty-state">
            <h2>Ask your first question</h2>
            <p>
              Responses will appear here with optional source references
              returned by your backend.
            </p>
          </div>
        ) : (
          messages.map((message, messageIndex) => (
            <article
              key={message.id}
              className={`message-card ${message.isError ? "error" : ""}`}
              ref={messageIndex === messages.length - 1 ? latestMessageRef : undefined}
            >
              <div className="message-block">
                <span className="role user">Question</span>
                <p>{message.question}</p>
              </div>

              <div className="message-block">
                <span className="role assistant">
                  Answer
                  <span className="ai-badge">✨ AI</span>
                </span>
                <p>{message.answer}</p>
              </div>

              {message.sources.length > 0 && showSources && (
                <div className="sources">
                  <h3>Sources ({message.sources.length})</h3>
                  <ul>
                    {message.sources.map((source, index) => (
                      <li key={`${message.id}-${source.documentId ?? index}`}>
                        <p className="source-title">
                          {source.title ||
                            source.documentId ||
                            `Source ${index + 1}`}
                        </p>
                        {source.snippet && (
                          <p className="source-snippet">{source.snippet}</p>
                        )}
                        <small className="source-meta">
                          <span>Document ID: {source.documentId}</span>
                          <span className="score">
                            Relevance: {(source.score * 100).toFixed(1)}%
                          </span>
                        </small>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </article>
          ))
        )}
      </main>

      <div>
        {!isFormOpen && (
          <button
            type="button"
            className="open-form"
            onClick={() => setIsFormOpen(true)}
          >
            Open
          </button>
        )}
        {isFormOpen && (
          <form
            style={{ position: "fixed", left: 0, bottom: 0, width: 380 }}
            ref={formRef}
            className="composer"
            onSubmit={submitQuestion}
          >
            <label htmlFor="question" className="composer-label">
              Your question{" "}
              <button
                type="button"
                className="close-form"
                onClick={() => setIsFormOpen(false)}
              >
                Close
              </button>
            </label>
            <textarea
              id="question"
              value={question}
              maxLength={MAX_QUESTION_LENGTH}
              onChange={(event) => setQuestion(event.target.value)}
              onKeyDown={(event) => {
                if (event.key === "Enter" && !event.shiftKey) {
                  event.preventDefault();
                  formRef.current?.requestSubmit();
                }
              }}
              placeholder="Ask a question grounded in your project documents..."
              disabled={isLoading}
            />
            <div className="composer-footer">
              <span>
                {question.length}/{MAX_QUESTION_LENGTH}
              </span>
              <div className="actions">
                <button
                  type="button"
                  onClick={() => setQuestion("")}
                  disabled={isLoading || !question}
                >
                  Clear
                </button>
                <button type="submit" disabled={!trimmedQuestion || isLoading}>
                  {isLoading ? "Generating..." : "Ask"}
                </button>
              </div>
            </div>
          </form>
        )}
      </div>
    </>
  );
}
