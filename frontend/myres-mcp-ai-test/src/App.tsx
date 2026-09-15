import { useEffect, useRef, useState, type FormEvent } from 'react';
import { createChat } from '@n8n/chat';
import { useTranslation } from 'react-i18next';
import { LANGUAGE_STORAGE_KEY, type AppLanguage } from './i18n';

const chatUrl = import.meta.env.VITE_N8N_CHAT_URL?.trim();

function App() {
  const { t, i18n } = useTranslation();
  const chatHostRef = useRef<HTMLDivElement>(null);
  const initializedRef = useRef(false);
  const [hasConversation, setHasConversation] = useState(false);
  const [hasInput, setHasInput] = useState(false);
  const selectedLanguage: AppLanguage = i18n.resolvedLanguage === 'tr' ? 'tr' : 'en';

  const changeLanguage = (language: AppLanguage) => {
    void i18n.changeLanguage(language);
    localStorage.setItem(LANGUAGE_STORAGE_KEY, language);
  };

  const handleChatInput = (event: FormEvent<HTMLDivElement>) => {
    if (event.target instanceof HTMLTextAreaElement) {
      setHasInput(event.target.value.length > 0);
    }
  };

  useEffect(() => {
    if (!chatUrl || !chatHostRef.current || initializedRef.current) return;

    initializedRef.current = true;
    const chatApp = createChat({
      webhookUrl: chatUrl,
      target: chatHostRef.current,
      mode: 'fullscreen',
      enableStreaming: true,
      loadPreviousSession: false,
      showWelcomeScreen: false,
      initialMessages: [],
      beforeMessageSent: () => {
        setHasConversation(true);
        setHasInput(false);
      },
      i18n: {
        en: {
          title: '',
          subtitle: '',
          footer: '',
          getStarted: t('chat.newConversation'),
          inputPlaceholder: '',
          closeButtonTooltip: t('chat.closeButtonTooltip'),
        },
      },
    });

    return () => {
      chatApp.unmount();
      initializedRef.current = false;
    };
  }, []);

  return (
    <main className="page-shell">
      <section className="chat-area" aria-label={t('app.title')}>
        <header className="app-header">
          <div>
            <h1>{t('app.title')}</h1>
            <p>{t('app.subtitle')}</p>
          </div>
          <div className="language-switcher" role="group" aria-label={t('language.selectorLabel')}>
            <button
              type="button"
              className={selectedLanguage === 'tr' ? 'active' : ''}
              aria-pressed={selectedLanguage === 'tr'}
              aria-label={t('language.turkish')}
              onClick={() => changeLanguage('tr')}
            >
              TR
            </button>
            <span aria-hidden="true">|</span>
            <button
              type="button"
              className={selectedLanguage === 'en' ? 'active' : ''}
              aria-pressed={selectedLanguage === 'en'}
              aria-label={t('language.english')}
              onClick={() => changeLanguage('en')}
            >
              EN
            </button>
          </div>
        </header>
        {chatUrl ? (
          <div className="chat-stage" onInputCapture={handleChatInput}>
            <div id="n8n-chat" ref={chatHostRef} />
            {!hasConversation && (
              <div className="react-welcome-message">{t('chat.initialMessage')}</div>
            )}
            {!hasInput && (
              <span className="react-chat-placeholder" aria-hidden="true">
                {t('chat.inputPlaceholder')}
              </span>
            )}
          </div>
        ) : (
          <div className="configuration-message" role="status">
            <h2>{t('errors.endpointMissing')}</h2>
            <p>{t('errors.setEnvironmentVariable')}</p>
          </div>
        )}
      </section>
    </main>
  );
}

export default App;
