declare module '@n8n/chat' {
  interface ChatOptions {
    webhookUrl: string;
    target?: string | Element;
    mode?: 'window' | 'fullscreen';
    enableStreaming?: boolean;
    showWelcomeScreen?: boolean;
    loadPreviousSession?: boolean;
    initialMessages?: string[];
    beforeMessageSent?: (message: string) => void | Promise<void>;
    i18n?: Record<string, {
      title: string;
      subtitle: string;
      footer: string;
      getStarted: string;
      inputPlaceholder: string;
      closeButtonTooltip: string;
    }>;
  }

  interface ChatApplication {
    unmount(): void;
  }

  export function createChat(options?: Partial<ChatOptions>): ChatApplication;
}
