import { createRoot } from 'react-dom/client';
import '@n8n/chat/style.css';
import './styles.css';
import './i18n';
import App from './App';

createRoot(document.getElementById('root')!).render(<App />);
