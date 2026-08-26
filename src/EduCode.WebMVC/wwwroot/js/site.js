document.addEventListener('DOMContentLoaded', () => {
  const animateRobot = (animation, options = {}) => {
    document.dispatchEvent(new CustomEvent('educode:robot', {
      detail: { animation, ...options }
    }));
  };
  const navToggle = document.querySelector('[data-nav-toggle]');
  const nav = document.querySelector('[data-nav]');
  const siteHeader = document.querySelector('.site-header');

  const syncHeaderState = () => {
    siteHeader?.classList.toggle('is-scrolled', window.scrollY > 6);
  };

  syncHeaderState();
  window.addEventListener('scroll', syncHeaderState, { passive: true });

  navToggle?.addEventListener('click', () => {
    const isOpen = nav?.classList.toggle('open') ?? false;
    navToggle.setAttribute('aria-expanded', String(isOpen));
  });

  const dismissAlert = alert => {
    if (!alert || alert.classList.contains('is-leaving')) return;
    alert.classList.add('is-leaving');
    window.setTimeout(() => alert.remove(), 260);
  };

  document.querySelectorAll('.alert').forEach(alert => {
    alert.querySelector('[data-dismiss]')?.addEventListener('click', () => dismissAlert(alert));

    const duration = Number(alert.dataset.autoDismiss);
    const timer = alert.querySelector('.alert-timer');
    if (!Number.isFinite(duration) || duration <= 0 || !timer) return;

    alert.style.setProperty('--alert-duration', `${duration}ms`);
    timer.addEventListener('animationend', () => dismissAlert(alert), { once: true });
    window.requestAnimationFrame(() => alert.classList.add('is-counting'));
  });

  document.querySelectorAll('[data-password-toggle]').forEach(btn => {
    btn.addEventListener('click', () => {
      const input = btn.parentElement?.querySelector('input');
      if (!input) return;

      const showing = input.type === 'text';
      input.type = showing ? 'password' : 'text';
      btn.textContent = showing ? 'Ver' : 'Ocultar';
      btn.setAttribute('aria-label', showing ? 'Mostrar contraseña' : 'Ocultar contraseña');
    });
  });

  const requestLoader = document.querySelector('[data-request-loader]');
  const requestLoaderMessage = requestLoader?.querySelector('[data-loader-message]');

  const showRequestLoader = message => {
    if (!requestLoader) return;
    if (requestLoaderMessage && message) requestLoaderMessage.textContent = message;
    requestLoader.hidden = false;
    document.body.classList.add('request-loading');
    window.requestAnimationFrame(() => requestLoader.classList.add('is-visible'));
  };

  const resetRequestLoader = () => {
    document.body.classList.remove('request-loading');
    requestLoader?.classList.remove('is-visible');
    if (requestLoader) requestLoader.hidden = true;

    document.querySelectorAll('[data-auth-form][data-submitting="true"]').forEach(form => {
      delete form.dataset.submitting;
      const button = form.querySelector('button[type="submit"]');
      if (!button) return;
      button.disabled = false;
      button.removeAttribute('aria-busy');
      if (button.dataset.originalHtml) {
        button.innerHTML = button.dataset.originalHtml;
        delete button.dataset.originalHtml;
      }
      button.classList.remove('is-loading');
    });
  };

  document.querySelectorAll('[data-auth-form]').forEach(form => {
    form.addEventListener('submit', event => {
      if (form.dataset.submitting === 'true') {
        event.preventDefault();
        return;
      }

      if (typeof form.checkValidity === 'function' && !form.checkValidity()) return;

      form.dataset.submitting = 'true';
      const button = event.submitter instanceof HTMLButtonElement
        ? event.submitter
        : form.querySelector('button[type="submit"]');

      if (button) {
        button.dataset.originalHtml = button.innerHTML;
        button.textContent = 'Procesando…';
        button.disabled = true;
        button.setAttribute('aria-busy', 'true');
        button.classList.add('is-loading');
      }

      showRequestLoader(form.dataset.loadingMessage || 'Procesando tu solicitud…');
    });
  });

  window.addEventListener('pageshow', resetRequestLoader);

  const code = document.querySelector('#codeInput');
  const lines = document.querySelector('[data-lines]');
  const updateLines = () => {
    if (!code || !lines) return;
    const count = Math.max(1, code.value.split('\n').length);
    lines.innerHTML = Array.from({ length: count }, (_, i) => `<span>${i + 1}</span>`).join('');
  };

  if (code) {
    updateLines();
    code.addEventListener('input', updateLines);
    code.addEventListener('scroll', () => {
      if (lines) lines.scrollTop = code.scrollTop;
    });
    code.addEventListener('keydown', e => {
      if (e.key !== 'Tab') return;
      e.preventDefault();
      const start = code.selectionStart;
      const end = code.selectionEnd;
      code.value = code.value.substring(0, start) + '    ' + code.value.substring(end);
      code.selectionStart = code.selectionEnd = start + 4;
      updateLines();
    });
  }

  document.querySelector('[data-reset-code]')?.addEventListener('click', () => {
    if (!code) return;
    if (confirm('¿Restablecer el código a la plantilla inicial?')) {
      code.value = code.dataset.initial || '';
      updateLines();
    }
  });

  const timer = document.querySelector('#timer');
  const elapsed = document.querySelector('#elapsedSeconds');
  if (timer && elapsed) {
    let seconds = 0;
    window.setInterval(() => {
      seconds++;
      elapsed.value = seconds;
      const minutes = String(Math.floor(seconds / 60)).padStart(2, '0');
      const remainingSeconds = String(seconds % 60).padStart(2, '0');
      timer.textContent = `${minutes}:${remainingSeconds}`;
    }, 1000);
  }

  document.querySelectorAll('[data-ai-form]').forEach(form => {
    form.addEventListener('submit', event => {
      if (form.dataset.submitting === 'true') {
        event.preventDefault();
        return;
      }

      const button = event.submitter instanceof HTMLButtonElement
        ? event.submitter
        : form.querySelector('button[type="submit"]');
      if (!button || button.disabled) return;

      form.dataset.submitting = 'true';
      button.disabled = true;
      button.dataset.originalText = button.textContent || '';
      button.textContent = 'Evaluando solución…';
      button.classList.add('is-loading');

      if (!form.querySelector('.ai-wait-message')) {
        const message = document.createElement('div');
        message.className = 'ai-wait-message';
        message.textContent = 'EduCode está ejecutando los casos de prueba y analizando el código localmente.';
        form.appendChild(message);
      }
    });
  });

  const celebration = document.querySelector('[data-achievement-celebration]');
  if (celebration) {
    let celebrationShown = false;
    const showCelebration = () => {
      if (celebrationShown) return;
      celebrationShown = true;
      animateRobot('celebrar', { force: true });
      celebration.hidden = false;
      document.body.classList.add('achievement-open');
      window.requestAnimationFrame(() => celebration.classList.add('is-visible'));
      window.setTimeout(closeCelebration, 7000);
      ['pointerdown', 'wheel', 'keydown', 'touchstart', 'scroll'].forEach(type => {
        window.removeEventListener(type, onFirstInteraction, passiveOptions);
      });
    };
    const closeCelebration = () => {
      celebration.classList.remove('is-visible');
      document.body.classList.remove('achievement-open');
      window.setTimeout(() => {
        if (!celebration.classList.contains('is-visible')) {
          celebration.hidden = true;
        }
      }, 220);
    };
    const passiveOptions = { passive: true };
    const onFirstInteraction = () => showCelebration();

    ['pointerdown', 'wheel', 'keydown', 'touchstart', 'scroll'].forEach(type => {
      window.addEventListener(type, onFirstInteraction, passiveOptions);
    });

    celebration.querySelectorAll('[data-achievement-close]').forEach(element => {
      element.addEventListener('click', closeCelebration);
    });
  }

  const feedbackCard = document.querySelector('[data-feedback-poll="true"]');
  if (feedbackCard) {
    const url = feedbackCard.dataset.feedbackUrl;
    const status = feedbackCard.querySelector('[data-feedback-status]');
    let attempts = 0;
    const maxAttempts = 20;

    const poll = async () => {
      if (!url || attempts >= maxAttempts) {
        if (status && attempts >= maxAttempts) {
          status.textContent = 'La nota técnica ya está disponible. La explicación personalizada podrá consultarse más tarde desde el historial.';
        }
        return;
      }

      attempts++;
      try {
        const response = await fetch(url, {
          headers: { 'X-Requested-With': 'XMLHttpRequest' },
          cache: 'no-store'
        });
        if (!response.ok) throw new Error('No fue posible consultar el estado.');
        const data = await response.json();
        if (data.disponible) {
          if (status) status.textContent = 'Explicación disponible. Actualizando la vista…';
          window.location.reload();
          return;
        }
      } catch (_) {
        // La evaluación local ya está disponible. Un fallo de polling no bloquea la vista.
      }

      window.setTimeout(poll, 2500);
    };

    window.setTimeout(poll, 1800);
  }
});
