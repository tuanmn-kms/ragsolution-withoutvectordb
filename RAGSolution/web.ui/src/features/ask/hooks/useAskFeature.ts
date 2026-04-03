import { useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import type { ChatMessage } from '../../../types'
import { askQuestion } from '../../../shared/api/ragApi'

type UseAskFeatureParams = {
  askApiUrl: string
}

export function useAskFeature({ askApiUrl }: UseAskFeatureParams) {
  const [question, setQuestion] = useState('')
  const [messages, setMessages] = useState<ChatMessage[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [showAdvancedSettings, setShowAdvancedSettings] = useState(false)
  const [topK, setTopK] = useState(3)
  const [maxAnswerSentences, setMaxAnswerSentences] = useState(3)
  const [selectedModel, setSelectedModel] = useState('gpt-4.1-2025-04-14')
  const [showSources, setShowSources] = useState(false)

  const trimmedQuestion = useMemo(() => question.trim(), [question])

  const submitQuestion = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    if (!trimmedQuestion || isLoading) {
      return
    }

    setIsLoading(true)

    try {
      const result = await askQuestion(askApiUrl, {
        question: trimmedQuestion,
        topK,
        maxAnswerSentences,
        model: selectedModel,
      })

      setMessages((previous) => [
        ...previous,
        {
          id: crypto.randomUUID(),
          question: result.question || trimmedQuestion,
          answer:
            result.answer?.trim() ||
            'No answer was returned. Please verify your RAG API response.',
          sources: result.sources || [],
          createdAt: new Date().toISOString(),
        },
      ])
      setQuestion('')
    } catch (error) {
      setMessages((previous) => [
        ...previous,
        {
          id: crypto.randomUUID(),
          question: trimmedQuestion,
          answer:
            'Unable to retrieve an answer right now. Check your API URL and backend availability. ' +
            (error instanceof Error ? error.message : ''),
          sources: [],
          createdAt: new Date().toISOString(),
          isError: true,
        },
      ])
    } finally {
      setIsLoading(false)
    }
  }

  const setSampleQuestion = (sample: string) => {
    if (isLoading) {
      return
    }

    setQuestion(sample)
  }

  return {
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
  }
}
