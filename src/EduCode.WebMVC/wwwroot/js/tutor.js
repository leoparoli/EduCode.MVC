document.addEventListener('DOMContentLoaded', () => {
  const form = document.querySelector('[data-tutor-form]');
  if (!form) return;

  const animateRobot = (animation, options = {}) => {
    document.dispatchEvent(new CustomEvent('educode:robot', {
      detail: { animation, ...options }
    }));
  };

  const messagesContainer = document.querySelector('[data-tutor-messages]');
  const input = form.querySelector('[data-tutor-input]');
  const historyInput = form.querySelector('[data-tutor-history]');
  const sendButton = form.querySelector('[data-tutor-send]');
  const counter = document.querySelector('[data-tutor-counter]');
  const quota = document.querySelector('[data-tutor-quota]');
  const clearButton = document.querySelector('[data-tutor-clear]');
  const suggestionsContainer = document.querySelector('[data-tutor-suggestions]');
  const maxLength = Number(form.dataset.maxLength) || 800;
  const tutorUserId = String(form.dataset.tutorUserId || '').trim();
  const storagePrefix = 'educode.tutor.historial.2.9.4';
  const storageKey = tutorUserId
    ? `${storagePrefix}.usuario.${tutorUserId}`
    : '';
  const legacyStorageKeys = [
    'educode.tutor.historial.2.9.3',
    'educode.tutor.historial.2.9.2',
    'educode.tutor.historial.2.9.1',
    'educode.tutor.historial.2.9.0',
    'educode.tutor.historial.2.8'
  ];
  let sending = false;
  const tutorEnabled = form.dataset.tutorEnabled !== 'false';
  const initialLimit = Number(form.dataset.quotaLimit) || 0;
  const initialRemaining = Number(form.dataset.quotaRemaining) || 0;
  let quotaExhausted = initialLimit > 0 && initialRemaining <= 0;
  if (!tutorEnabled || quotaExhausted) animateRobot('senalar', { force: true });
  removeLegacySharedHistory();
  let history = loadHistory();

  history.forEach(message => appendMessage(message.role, message.content, {
    provider: message.provider || '',
    warning: message.warning || '',
    persist: false
  }));
  syncHistoryField();
  updateCounter();
  scrollToBottom(false);

  document.querySelectorAll('[data-tutor-suggestion]').forEach(button => {
    button.addEventListener('click', () => selectSuggestion(button.textContent || ''));
  });

  input?.addEventListener('input', updateCounter);
  input?.addEventListener('keydown', event => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      form.requestSubmit();
    }
  });

  clearButton?.addEventListener('click', () => {
    if (history.length > 0 && !window.confirm('¿Iniciar una conversación nueva?')) return;
    history = [];
    animateRobot('idle', { force: true });
    removeCurrentUserHistory();
    syncHistoryField();
    messagesContainer?.querySelectorAll('.tutor-message:not(.tutor-message-welcome)').forEach(message => message.remove());
    renderSuggestions([
      '¿Cómo recorro un array sin salir de rango?',
      '¿Cómo agrego y elimino elementos de una lista?',
      '¿Qué funcionalidades puedo usar para trabajar con strings?',
      '¿Cuál es la diferencia entre for y while?',
      '¿Por qué una función puede devolver undefined?',
      'Quiero practicar clases y objetos dentro del recorrido'
    ]);
    if (input) {
      input.value = '';
      input.focus();
      updateCounter();
    }
  });

  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (sending || !input || !sendButton) return;

    const question = input.value.trim();
    if (!question) {
      input.focus();
      input.classList.add('is-invalid');
      animateRobot('senalar', { force: true });
      window.setTimeout(() => input.classList.remove('is-invalid'), 900);
      return;
    }

    if (question.length > maxLength) {
      appendMessage(
        'error',
        `La consulta puede tener hasta ${maxLength} caracteres. Actualmente contiene ${question.length}.`,
        { persist: false }
      );
      input.focus();
      animateRobot('senalar', { force: true });
      return;
    }

    const contextHistory = history.slice(-6).map(message => ({
      rol: message.role,
      contenido: message.content
    }));

    appendMessage('user', question, { persist: false });
    input.value = '';
    updateCounter();
    setSending(true);
    animateRobot('pensar', { loop: true, force: true });
    const typingMessage = appendTyping();

    try {
      const formData = new FormData(form);
      formData.set('Consulta', question);
      formData.set('HistorialJson', JSON.stringify(contextHistory));

      const response = await fetch(form.action, {
        method: 'POST',
        body: formData,
        headers: { 'X-Requested-With': 'XMLHttpRequest' }
      });

      const data = await readResponse(response);
      typingMessage?.remove();

      if (!response.ok) {
        appendMessage('error', data.error || 'El Tutor IA no pudo responder en este momento.', {
          persist: false
        });
        animateRobot('senalar', { force: true });
        return;
      }

      updateQuota(data.consultasRestantes, data.limiteDiario);

      if (!data.aceptada) {
        appendMessage('rejected', data.motivoRechazo || data.respuesta, {
          provider: data.clasificacion || 'Revisión del alcance',
          persist: false
        });
        renderSuggestions(data.sugerencias || []);
        animateRobot('senalar', { force: true });
        return;
      }

      if (quotaExhausted) {
        animateRobot('senalar', { force: true });
      } else {
        animateRobot('asentir', { force: true });
      }
      history.push({ role: 'user', content: question });
      history.push({
        role: 'assistant',
        content: data.respuesta,
        provider: providerLabel(data),
        warning: data.advertencia || ''
      });
      trimAndSaveHistory();
      appendMessage('assistant', data.respuesta, {
        provider: providerLabel(data),
        warning: data.advertencia || '',
        persist: false
      });
      renderSuggestions(data.sugerencias || []);
    } catch (_) {
      typingMessage?.remove();
      appendMessage(
        'error',
        'El tutor no pudo responder en este momento. Intentá nuevamente en unos segundos.',
        { persist: false }
      );
      animateRobot('senalar', { force: true });
    } finally {
      setSending(false);
      input?.focus();
    }
  });

  function providerLabel(data) {
    return data?.usoProveedorIA
      ? 'Tutor EduCode · IA'
      : 'Tutor EduCode';
  }

  function appendMessage(role, content, options = {}) {
    if (!messagesContainer) return null;

    const article = document.createElement('article');
    const normalizedRole = role === 'user'
      ? 'user'
      : role === 'rejected'
        ? 'rejected'
        : role === 'error'
          ? 'error'
          : 'assistant';
    article.className = `tutor-message tutor-message-${normalizedRole}`;

    const avatar = document.createElement('div');
    avatar.className = 'tutor-message-avatar';
    avatar.setAttribute('aria-hidden', 'true');
    avatar.textContent = normalizedRole === 'user'
      ? 'Tú'
      : normalizedRole === 'error'
        ? '!'
        : normalizedRole === 'rejected'
          ? '↺'
          : 'E';

    const body = document.createElement('div');
    body.className = 'tutor-message-body';

    const meta = document.createElement('div');
    meta.className = 'tutor-message-meta';
    const author = document.createElement('strong');
    author.textContent = normalizedRole === 'user'
      ? 'Vos'
      : normalizedRole === 'error'
        ? 'No pudimos responder ahora'
        : normalizedRole === 'rejected'
          ? 'Alcance del Tutor IA'
          : 'Tutor EduCode';
    const detail = document.createElement('span');
    detail.textContent = options.provider || 'Ahora';
    meta.append(author, detail);

    const messageContent = document.createElement('div');
    messageContent.className = 'tutor-message-content';
    renderRichText(messageContent, content || '');

    body.append(meta, messageContent);

    if (options.warning) {
      const warning = document.createElement('div');
      warning.className = 'tutor-message-warning';
      warning.setAttribute('role', 'note');
      warning.textContent = options.warning;
      body.appendChild(warning);
    }
    article.append(avatar, body);
    messagesContainer.appendChild(article);

    if (options.persist) {
      history.push({
        role,
        content,
        provider: options.provider || '',
        warning: options.warning || ''
      });
      trimAndSaveHistory();
    }

    scrollToBottom();
    return article;
  }

  function appendTyping() {
    if (!messagesContainer) return null;
    const article = document.createElement('article');
    article.className = 'tutor-message tutor-message-assistant tutor-message-typing';

    const avatar = document.createElement('div');
    avatar.className = 'tutor-message-avatar';
    avatar.setAttribute('aria-hidden', 'true');
    avatar.textContent = 'E';

    const body = document.createElement('div');
    body.className = 'tutor-message-body';
    const meta = document.createElement('div');
    meta.className = 'tutor-message-meta';
    const author = document.createElement('strong');
    author.textContent = 'Tutor EduCode';
    const status = document.createElement('span');
    status.textContent = 'Analizando la consulta';
    meta.append(author, status);

    const typing = document.createElement('div');
    typing.className = 'tutor-typing';
    typing.setAttribute('aria-label', 'El tutor está redactando');
    typing.append(document.createElement('i'), document.createElement('i'), document.createElement('i'));

    body.append(meta, typing);
    article.append(avatar, body);
    messagesContainer.appendChild(article);
    scrollToBottom();
    return article;
  }

  function renderRichText(container, content) {
    content = String(content || '').slice(0, 7000);
    const pattern = /```(?:javascript|js)?\s*\n?([\s\S]*?)```/gi;
    let cursor = 0;
    let match;

    while ((match = pattern.exec(content)) !== null) {
      renderTextBlock(container, content.slice(cursor, match.index));
      renderCodeBlock(container, match[1].replace(/\n$/, ''));
      cursor = pattern.lastIndex;
    }
    renderTextBlock(container, content.slice(cursor));
  }

  function renderTextBlock(container, text) {
    const lines = text.replace(/\r/g, '').split('\n');
    let paragraph = [];
    let list = [];

    const flushParagraph = () => {
      if (!paragraph.length) return;
      const p = document.createElement('p');
      appendInline(p, paragraph.join(' ').trim());
      if (p.textContent?.trim()) container.appendChild(p);
      paragraph = [];
    };

    const flushList = () => {
      if (!list.length) return;
      const ul = document.createElement('ul');
      list.forEach(item => {
        const li = document.createElement('li');
        appendInline(li, item);
        ul.appendChild(li);
      });
      container.appendChild(ul);
      list = [];
    };

    lines.forEach(line => {
      const trimmed = line.trim();
      if (!trimmed) {
        flushParagraph();
        flushList();
        return;
      }
      if (/^[-•]\s+/.test(trimmed)) {
        flushParagraph();
        list.push(trimmed.replace(/^[-•]\s+/, ''));
      } else {
        flushList();
        paragraph.push(trimmed);
      }
    });

    flushParagraph();
    flushList();
  }

  function appendInline(element, text) {
    const tokenPattern = /(\[[^\]]+\]\((?:\/Ejercicios\/Detalle\/\d+|\/Temas(?:\/[^)]*)?)\)|`[^`]+`|\*\*[^*]+\*\*)/g;
    let cursor = 0;
    let match;
    while ((match = tokenPattern.exec(text)) !== null) {
      element.appendChild(document.createTextNode(text.slice(cursor, match.index)));
      const token = match[0];
      if (token.startsWith('[')) {
        const linkMatch = token.match(/^\[([^\]]+)\]\(([^)]+)\)$/);
        if (linkMatch) {
          const link = document.createElement('a');
          link.href = linkMatch[2];
          link.textContent = linkMatch[1].replace(/\*\*/g, '');
          link.className = 'tutor-inline-link';
          element.appendChild(link);
        } else {
          element.appendChild(document.createTextNode(token));
        }
      } else if (token.startsWith('`')) {
        const code = document.createElement('code');
        code.textContent = token.slice(1, -1);
        element.appendChild(code);
      } else {
        const strong = document.createElement('strong');
        strong.textContent = token.slice(2, -2);
        element.appendChild(strong);
      }
      cursor = tokenPattern.lastIndex;
    }
    element.appendChild(document.createTextNode(text.slice(cursor)));
  }

  function renderCodeBlock(container, codeText) {
    const wrapper = document.createElement('div');
    wrapper.className = 'tutor-code-block';
    const toolbar = document.createElement('div');
    toolbar.className = 'tutor-code-toolbar';
    const language = document.createElement('span');
    language.textContent = 'JavaScript';
    const copy = document.createElement('button');
    copy.type = 'button';
    copy.textContent = 'Copiar';
    copy.addEventListener('click', async () => {
      try {
        await navigator.clipboard.writeText(codeText);
        copy.textContent = 'Copiado';
      } catch (_) {
        copy.textContent = 'Seleccioná el código';
      }
      window.setTimeout(() => { copy.textContent = 'Copiar'; }, 1800);
    });
    toolbar.append(language, copy);

    const pre = document.createElement('pre');
    const code = document.createElement('code');
    code.textContent = codeText;
    pre.appendChild(code);
    wrapper.append(toolbar, pre);
    container.appendChild(wrapper);
  }

  function renderSuggestions(suggestions) {
    if (!suggestionsContainer || !Array.isArray(suggestions) || !suggestions.length) return;
    const wrapper = suggestionsContainer.querySelector('div');
    if (!wrapper) return;
    wrapper.replaceChildren();
    suggestions.slice(0, 4).forEach(suggestion => {
      const button = document.createElement('button');
      button.type = 'button';
      button.textContent = suggestion;
      button.addEventListener('click', () => selectSuggestion(suggestion));
      wrapper.appendChild(button);
    });
  }

  function selectSuggestion(question) {
    if (!input || input.disabled || !tutorEnabled || quotaExhausted) return;
    input.value = question.trim();
    updateCounter();
    input.focus();
  }

  function setSending(value) {
    sending = value;
    if (input) input.disabled = value || !tutorEnabled || quotaExhausted;
    if (sendButton) {
      sendButton.disabled = value || !tutorEnabled || quotaExhausted;
      sendButton.classList.toggle('is-loading', value);
      sendButton.replaceChildren();
      if (value) {
        const spinner = document.createElement('span');
        spinner.className = 'tutor-button-spinner';
        spinner.setAttribute('aria-hidden', 'true');
        sendButton.append(spinner, document.createTextNode(' Consultando…'));
      } else {
        const arrow = document.createElement('span');
        arrow.setAttribute('aria-hidden', 'true');
        arrow.textContent = '→';
        sendButton.append(document.createTextNode('Consultar '), arrow);
      }
    }
  }

  function updateCounter() {
    if (!input || !counter) return;
    counter.textContent = `${input.value.length} / ${maxLength}`;
    counter.classList.toggle('near-limit', input.value.length >= maxLength * 0.85);
  }

  function updateQuota(remaining, limit) {
    if (!quota) return;
    quota.textContent = limit > 0
      ? `${remaining} de ${limit} consultas disponibles`
      : 'Uso disponible';
    quotaExhausted = limit > 0 && remaining <= 0;
    quota.closest('.tutor-quota')?.classList.toggle('is-empty', quotaExhausted);
    if (quotaExhausted) {
      animateRobot('senalar', { force: true });
      if (input) {
        input.disabled = true;
        input.placeholder = 'Alcanzaste el límite diario. Volvé mañana para realizar nuevas consultas.';
      }
      if (sendButton) sendButton.disabled = true;
    }
  }

  function syncHistoryField() {
    if (!historyInput) return;
    historyInput.value = JSON.stringify(history.slice(-6).map(message => ({
      rol: message.role,
      contenido: message.content
    })));
  }

  function trimAndSaveHistory() {
    history = history.slice(-10);
    syncHistoryField();
    if (!storageKey || !tutorUserId) return;

    try {
      window.sessionStorage.setItem(storageKey, JSON.stringify({
        version: 1,
        usuarioId: tutorUserId,
        mensajes: history
      }));
    } catch (_) {
      // El chat sigue funcionando aunque el navegador no permita sessionStorage.
    }
  }

  function loadHistory() {
    if (!storageKey || !tutorUserId) return [];

    try {
      const raw = window.sessionStorage.getItem(storageKey);
      if (!raw) return [];

      const envelope = JSON.parse(raw);
      if (!envelope ||
          String(envelope.usuarioId || '') !== tutorUserId ||
          !Array.isArray(envelope.mensajes)) {
        window.sessionStorage.removeItem(storageKey);
        return [];
      }

      return envelope.mensajes.filter(message =>
        message &&
        (message.role === 'user' || message.role === 'assistant') &&
        typeof message.content === 'string'
      ).map(message => ({
        role: message.role,
        content: message.content.slice(0, 7000),
        provider: typeof message.provider === 'string' ? message.provider.slice(0, 120) : '',
        warning: typeof message.warning === 'string' ? message.warning.slice(0, 400) : ''
      })).slice(-10);
    } catch (_) {
      removeCurrentUserHistory();
      return [];
    }
  }

  function removeCurrentUserHistory() {
    if (!storageKey) return;
    try {
      window.sessionStorage.removeItem(storageKey);
    } catch (_) {
      // No se bloquea el chat si el almacenamiento del navegador no está disponible.
    }
  }

  function removeLegacySharedHistory() {
    try {
      legacyStorageKeys.forEach(key => window.sessionStorage.removeItem(key));
    } catch (_) {
      // Las claves antiguas no son necesarias para que el Tutor funcione.
    }
  }

  async function readResponse(response) {
    const text = await response.text();
    if (!text) return {};
    try {
      return JSON.parse(text);
    } catch (_) {
      return { error: text };
    }
  }

  function scrollToBottom(smooth = true) {
    if (!messagesContainer) return;
    window.requestAnimationFrame(() => {
      messagesContainer.scrollTo({
        top: messagesContainer.scrollHeight,
        behavior: smooth ? 'smooth' : 'auto'
      });
    });
  }
});
