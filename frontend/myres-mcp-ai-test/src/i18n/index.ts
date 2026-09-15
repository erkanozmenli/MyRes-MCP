import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import en from './locales/en.json';
import tr from './locales/tr.json';

export type AppLanguage = 'en' | 'tr';

export const LANGUAGE_STORAGE_KEY = 'myres-ui-language';

export function normalizeLanguage(language: string | null | undefined): AppLanguage {
  return language?.toLowerCase().startsWith('tr') ? 'tr' : 'en';
}

function getInitialLanguage(): AppLanguage {
  const storedLanguage = localStorage.getItem(LANGUAGE_STORAGE_KEY);

  if (storedLanguage === 'tr' || storedLanguage === 'en') {
    return storedLanguage;
  }

  return normalizeLanguage(navigator.language);
}

void i18n.use(initReactI18next).init({
  resources: {
    en: { translation: en },
    tr: { translation: tr },
  },
  lng: getInitialLanguage(),
  fallbackLng: 'en',
  supportedLngs: ['en', 'tr'],
  interpolation: { escapeValue: false },
});

export default i18n;
