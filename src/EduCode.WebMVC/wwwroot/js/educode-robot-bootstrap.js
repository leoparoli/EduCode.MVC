(() => {
  'use strict';

  const widget = document.querySelector('[data-educode-robot]');
  if (!widget) return;

  const loading = widget.querySelector('[data-robot-loading]');
  const statusText = widget.querySelector('[data-robot-status]');
  const fallback = widget.querySelector('[data-robot-fallback]');
  const errorText = widget.querySelector('[data-robot-error]');
  const retry = widget.querySelector('[data-robot-retry]');
  let completed = false;

  const normalizeMessage = error => {
    if (!error) return 'No se pudo iniciar el visor 3D.';
    const value = error.message || error.reason?.message || error.reason || String(error);
    return String(value).replace(/^TypeError:\s*/i, '').slice(0, 220);
  };

  const status = message => {
    if (completed) return;
    widget.classList.add('is-booting');
    widget.dataset.robotBootStatus = message;
    if (statusText) statusText.textContent = message;
  };

  const ready = () => {
    completed = true;
    window.clearTimeout(timeoutId);
    widget.classList.remove('is-booting', 'has-error');
    widget.classList.add('is-ready');
    loading?.setAttribute('hidden', '');
    fallback?.setAttribute('hidden', '');
    widget.dataset.robotBootStatus = 'ready';
  };

  const fail = error => {
    completed = true;
    window.clearTimeout(timeoutId);
    widget.classList.remove('is-booting', 'is-ready');
    widget.classList.add('has-error');
    loading?.setAttribute('hidden', '');
    fallback?.removeAttribute('hidden');
    if (errorText) errorText.textContent = normalizeMessage(error);
    widget.dataset.robotBootStatus = 'error';
  };

  const reset = () => {
    const url = new URL(window.location.href);
    url.searchParams.set('robotRetry', Date.now().toString());
    window.location.replace(url.toString());
  };

  const isRobotError = value => {
    const text = normalizeMessage(value).toLowerCase();
    return ['educode-robot', 'three', 'gltf', 'webgl', 'module script', 'dynamically imported module']
      .some(token => text.includes(token));
  };

  retry?.addEventListener('click', reset);

  window.addEventListener('error', event => {
    const source = event.filename || event.target?.src || '';
    if (source.includes('educode-robot') || isRobotError(event.error || event.message)) {
      fail(event.error || event.message);
    }
  }, true);

  window.addEventListener('unhandledrejection', event => {
    if (isRobotError(event.reason)) fail(event.reason);
  });

  const timeoutId = window.setTimeout(() => {
    fail(new Error('EDU-02 superó el tiempo máximo de carga. Presioná Reintentar.'));
  }, 45000);

  widget.classList.add('is-booting');
  status('EDU-02 se está preparando');

  window.EduCodeRobotBoot = Object.freeze({ status, ready, fail, reset });
})();
