let THREE = null;
let GLTFLoader = null;

const THREE_DEPENDENCY_SOURCES = Object.freeze([
  {
    name: 'esm.sh',
    three: 'https://esm.sh/three@0.185.1?target=es2022',
    loader: 'https://esm.sh/three@0.185.1/examples/jsm/loaders/GLTFLoader.js?target=es2022'
  },
  {
    name: 'jsDelivr ESM',
    three: 'https://cdn.jsdelivr.net/npm/three@0.185.1/+esm',
    loader: 'https://cdn.jsdelivr.net/npm/three@0.185.1/examples/jsm/loaders/GLTFLoader.js/+esm'
  },
  {
    name: 'unpkg modules',
    three: 'https://unpkg.com/three@0.185.1/build/three.module.js?module',
    loader: 'https://unpkg.com/three@0.185.1/examples/jsm/loaders/GLTFLoader.js?module'
  }
]);

const THREE_PROVIDER_STORAGE_KEY = 'educode.robot.three-provider.v1';
const ROBOT_EVENT = 'educode:robot';
const READY_EVENT = 'educode:robot-ready';
const widget = document.querySelector('[data-educode-robot]');

let queuedCommand = null;
let navigationPending = false;

function getOrderedDependencySources() {
  let preferred = '';
  try {
    preferred = sessionStorage.getItem(THREE_PROVIDER_STORAGE_KEY) || '';
  } catch {
    // sessionStorage puede no estar disponible en contextos endurecidos.
  }

  if (!preferred) return THREE_DEPENDENCY_SOURCES;

  return [
    ...THREE_DEPENDENCY_SOURCES.filter(source => source.name === preferred),
    ...THREE_DEPENDENCY_SOURCES.filter(source => source.name !== preferred)
  ];
}

async function loadThreeDependencies() {
  const failures = [];

  for (const source of getOrderedDependencySources()) {
    try {
      window.EduCodeRobotBoot?.status(`Cargando motor 3D desde ${source.name}…`);
      const [threeModule, loaderModule] = await Promise.all([
        import(source.three),
        import(source.loader)
      ]);

      if (typeof threeModule.WebGLRenderer !== 'function' || typeof loaderModule.GLTFLoader !== 'function') {
        throw new Error(`El proveedor ${source.name} devolvió módulos incompletos.`);
      }

      try {
        sessionStorage.setItem(THREE_PROVIDER_STORAGE_KEY, source.name);
      } catch {
        // No afecta el funcionamiento si el navegador no permite persistirlo.
      }

      return {
        THREE: threeModule,
        GLTFLoader: loaderModule.GLTFLoader,
        provider: source.name
      };
    } catch (error) {
      failures.push(`${source.name}: ${error?.message || error}`);
      console.warn(`EDU-02: falló ${source.name}; se probará el siguiente proveedor.`, error);
    }
  }

  throw new Error(`No se pudo cargar Three.js. ${failures.join(' | ')}`);
}

document.addEventListener(ROBOT_EVENT, event => {
  const detail = event.detail || {};
  queuedCommand = detail;
  const accepted = window.eduCodeRobot?.play(detail.animation, detail) ?? false;
  if (accepted) queuedCommand = null;
});

function isInternalNavigationClick(event) {
  if (event.defaultPrevented || event.button !== 0) return false;
  if (event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) return false;

  const link = event.target?.closest?.('a[href]');
  if (!link || link.hasAttribute('download')) return false;
  if (link.target && link.target.toLowerCase() !== '_self') return false;

  let target;
  try {
    target = new URL(link.href, window.location.href);
  } catch {
    return false;
  }

  if (target.origin !== window.location.origin) return false;
  const current = new URL(window.location.href);
  const sameDocument = target.pathname === current.pathname &&
    target.search === current.search &&
    target.hash && target.hash !== current.hash;

  return !sameDocument;
}

document.addEventListener('click', event => {
  if (!isInternalNavigationClick(event)) return;
  navigationPending = true;
  window.eduCodeRobot?.suspendForNavigation();
});

document.addEventListener('submit', event => {
  queueMicrotask(() => {
    if (event.defaultPrevented) return;
    navigationPending = true;
    window.eduCodeRobot?.suspendForNavigation();
  });
});

window.addEventListener('pageshow', () => {
  navigationPending = false;
});

if (widget) scheduleRobotStart(widget);

function scheduleRobotStart(container) {
  let started = false;
  let observer = null;

  const launch = () => {
    if (started) return;
    started = true;
    observer?.disconnect();

    const run = () => {
      if (navigationPending) return;

      startRobot(container).catch(error => {
        if (navigationPending || error?.name === 'AbortError') return;

        console.error('EduCode Robot:', error);
        container.classList.remove('is-booting', 'is-ready');
        container.classList.add('has-error');
        container.querySelector('[data-robot-loading]')?.setAttribute('hidden', '');
        container.querySelector('[data-robot-fallback]')?.removeAttribute('hidden');
        const message = container.querySelector('[data-robot-error]');
        if (message) message.textContent = error?.message || 'No se pudo iniciar EDU-02.';
        window.EduCodeRobotBoot?.fail(error);
      });
    };

    if (globalThis.scheduler?.postTask) {
      globalThis.scheduler.postTask(run, { priority: 'background', delay: 80 });
    } else if ('requestIdleCallback' in window) {
      window.requestIdleCallback(run, { timeout: 1000 });
    } else {
      window.setTimeout(run, 80);
    }
  };

  if ('IntersectionObserver' in window) {
    observer = new IntersectionObserver(entries => {
      if (entries.some(entry => entry.isIntersecting)) launch();
    }, { rootMargin: '260px 0px', threshold: 0.01 });
    observer.observe(container);
  } else {
    launch();
  }
}

async function startRobot(container) {
  const dependencies = await loadThreeDependencies();
  THREE = dependencies.THREE;
  GLTFLoader = dependencies.GLTFLoader;
  container.dataset.threeProvider = dependencies.provider;
  window.EduCodeRobotBoot?.status('Cargando modelo optimizado…');

  const controller = new EduCodeRobot(container);
  window.eduCodeRobot = controller;
  await controller.initialize();

  if (queuedCommand?.animation) {
    controller.play(queuedCommand.animation, queuedCommand);
    queuedCommand = null;
  } else {
    controller.playInitialBehavior();
  }

  window.EduCodeRobotBoot?.ready();
  document.dispatchEvent(new CustomEvent(READY_EVENT, {
    detail: {
      provider: container.dataset.threeProvider || 'unknown',
      strategy: controller.manifest?.playback_strategy || 'split-clips'
    }
  }));
}

class EduCodeRobot {
  constructor(container) {
    this.container = container;
    this.canvas = container.querySelector('[data-robot-canvas]');
    this.loading = container.querySelector('[data-robot-loading]');
    this.fallback = container.querySelector('[data-robot-fallback]');
    this.context = container.dataset.context || 'idle';
    this.initialAnimation = container.dataset.initialAnimation || 'idle';
    this.modelUrl = container.dataset.modelUrl;
    this.manifestUrl = container.dataset.manifestUrl;
    this.outcome = container.dataset.outcome || '';

    this.reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    this.scene = new THREE.Scene();
    this.camera = new THREE.PerspectiveCamera(28, 1, 0.01, 100);
    this.clock = new THREE.Clock();
    this.modelRoot = new THREE.Group();
    this.modelRoot.name = 'EduCodeWebRoot';
    this.scene.add(this.modelRoot);

    this.renderer = null;
    this.mixer = null;
    this.actions = new Map();
    this.clipPromises = new Map();
    this.currentAction = null;
    this.currentName = '';
    this.currentLoop = false;
    this.manifest = null;
    this.robotScene = null;
    this.headTarget = null;
    this.disposed = false;
    this.pageVisible = !document.hidden;
    this.widgetVisible = true;
    this.navigationSuspended = false;
    this.readyForRender = false;
    this.renderLoopActive = false;
    this.targetFps = 24;
    this.frameInterval = 1000 / this.targetFps;
    this.lastFrameAt = 0;
    this.lastTrigger = new Map();
    this.abortController = new AbortController();
    this.commandToken = 0;
    this.pendingPlay = null;
    this.prefetchStarted = false;

    this.pointer = { targetX: 0, targetY: 0, x: 0, y: 0 };
    this.headOffset = new THREE.Quaternion();
    this.headEuler = new THREE.Euler(0, 0, 0, 'YXZ');

    this.onMixerFinished = this.onMixerFinished.bind(this);
    this.render = this.render.bind(this);
    this.onPointerMove = this.onPointerMove.bind(this);
    this.onPointerLeave = this.onPointerLeave.bind(this);
    this.onVisibilityChange = this.onVisibilityChange.bind(this);
    this.onPageHide = this.onPageHide.bind(this);
    this.onPageShow = this.onPageShow.bind(this);

    // Los eventos de UI se registran inmediatamente. Si una animación todavía
    // no está cargada, play() la encola sin bloquear el hover o el click.
    this.bindContextInteractions();
    this.bindLifecycle();
  }

  async initialize() {
    window.EduCodeRobotBoot?.status('Inicializando WebGL…');
    this.createRenderer();
    this.createLights();
    this.observeSize();
    this.observeVisibility();

    window.EduCodeRobotBoot?.status('Descargando EDU-02…');
    const [manifest, gltf] = await Promise.all([
      this.loadManifest(),
      this.loadModel()
    ]);

    if (this.disposed) return;

    this.manifest = manifest;
    this.robotScene = gltf.scene;
    this.modelRoot.add(this.robotScene);
    this.mixer = new THREE.AnimationMixer(this.robotScene);
    this.mixer.addEventListener('finished', this.onMixerFinished);

    // Sólo idle se interpreta antes del primer render. El resto ya está
    // separado en archivos pequeños y se carga cuando realmente hace falta.
    window.EduCodeRobotBoot?.status('Preparando respiración…');
    await this.ensureAction('idle');
    if (this.disposed) return;

    this.activateAction('idle', { immediate: true, force: true });
    this.mixer.update(0);
    this.robotScene.updateMatrixWorld(true);

    window.EduCodeRobotBoot?.status('Preparando materiales y cámara…');
    this.prepareModel();
    this.resolveHeadTarget();

    this.loading?.setAttribute('hidden', '');
    this.container.classList.add('is-ready');
    this.readyForRender = true;

    if (!this.reducedMotion) {
      this.syncRenderLoop();
    } else {
      this.renderStaticFrame('idle', 0);
    }

    const pending = this.pendingPlay;
    this.pendingPlay = null;
    if (pending?.name && pending.name !== 'idle') {
      this.play(pending.name, pending.options);
    }

    this.scheduleContextPrefetch();
  }

  createRenderer() {
    if (!this.canvas) throw new Error('No se encontró el canvas de EDU-02.');

    this.renderer = new THREE.WebGLRenderer({
      canvas: this.canvas,
      alpha: true,
      antialias: true,
      powerPreference: 'high-performance'
    });

    this.canvas.addEventListener('webglcontextlost', event => {
      event.preventDefault();
      window.EduCodeRobotBoot?.fail(new Error('El contexto WebGL se perdió. Presioná Reintentar.'));
    }, { once: true });

    const compactViewport = window.matchMedia('(max-width: 820px)').matches;
    this.renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, compactViewport ? 1 : 1.25));
    this.renderer.outputColorSpace = THREE.SRGBColorSpace;
    this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
    this.renderer.toneMappingExposure = 0.92;
  }

  createLights() {
    const hemisphere = new THREE.HemisphereLight(0xf3fbff, 0x162235, 2.15);
    const key = new THREE.DirectionalLight(0xffffff, 3.3);
    key.position.set(4, 5.5, 6);

    const fill = new THREE.DirectionalLight(0x8fe8ff, 1.35);
    fill.position.set(-4, 2.2, 4);

    const rim = new THREE.DirectionalLight(0x8b96ff, 0.9);
    rim.position.set(2, 4, -5);

    this.scene.add(hemisphere, key, fill, rim);
  }

  async loadManifest() {
    if (!this.manifestUrl) throw new Error('Falta la ruta del manifiesto de animaciones.');
    const response = await fetch(this.manifestUrl, {
      cache: 'force-cache',
      credentials: 'same-origin',
      signal: this.abortController.signal
    });
    if (!response.ok) throw new Error(`No se pudo cargar el manifiesto (${response.status}).`);
    const manifest = await response.json();

    if (!manifest || !manifest.clips || !manifest.clips.idle || Number(manifest.clip_count) < 1) {
      throw new Error('El manifiesto optimizado del robot no es válido.');
    }

    return manifest;
  }

  async loadModel() {
    if (!this.modelUrl) throw new Error('Falta la ruta del modelo 3D.');
    return this.loadGlbFromUrl(this.modelUrl, 'modelo base');
  }

  async loadGlbFromUrl(url, label = 'GLB') {
    const loader = new GLTFLoader();
    let response;

    try {
      response = await fetch(url, {
        cache: 'force-cache',
        credentials: 'same-origin',
        priority: 'low',
        signal: this.abortController.signal
      });
    } catch (error) {
      if (error?.name === 'AbortError') throw error;
      throw new Error(`No se pudo descargar ${label}. ${error?.message || error}`);
    }

    if (!response.ok) throw new Error(`No se pudo cargar ${label} (${response.status}).`);

    const buffer = await response.arrayBuffer();
    if (this.abortController.signal.aborted) {
      throw new DOMException('Carga de EDU-02 cancelada por navegación.', 'AbortError');
    }

    const absoluteUrl = new URL(url, window.location.href);
    const resourcePath = absoluteUrl.href.slice(0, absoluteUrl.href.lastIndexOf('/') + 1);

    return new Promise((resolve, reject) => {
      const parse = () => {
        if (this.abortController.signal.aborted) {
          reject(new DOMException('Carga de EDU-02 cancelada por navegación.', 'AbortError'));
          return;
        }

        loader.parse(
          buffer,
          resourcePath,
          resolve,
          error => reject(new Error(`No se pudo interpretar ${label}. ${error?.message || error}`))
        );
      };

      // La interpretación GLB sigue siendo una operación del hilo principal,
      // pero se agenda con prioridad de fondo para no adelantarse a clicks,
      // hover, scroll o navegación que el usuario ya esté realizando.
      if (globalThis.scheduler?.postTask) {
        globalThis.scheduler.postTask(parse, { priority: 'background' }).catch(reject);
      } else {
        window.setTimeout(parse, 0);
      }
    });
  }

  resolveClipUrl(name) {
    const definition = this.manifest?.clips?.[name];
    if (!definition?.file) return '';
    const manifestAbsolute = new URL(this.manifestUrl, window.location.href);
    const directory = new URL('.', manifestAbsolute);
    const url = new URL(definition.file, directory);
    url.searchParams.set('v', '3.1.2p2');
    return url.href;
  }

  ensureAction(name) {
    if (this.actions.has(name)) return Promise.resolve(this.actions.get(name));
    if (this.clipPromises.has(name)) return this.clipPromises.get(name);
    if (!this.manifest?.clips?.[name] || !this.mixer) {
      return Promise.reject(new Error(`Animación no registrada: ${name}`));
    }

    const promise = (async () => {
      const url = this.resolveClipUrl(name);
      const gltf = await this.loadGlbFromUrl(url, `animación ${name}`);
      const clip = gltf.animations?.[0];
      if (!clip) throw new Error(`El archivo de ${name} no contiene animación.`);

      const definition = this.manifest.clips[name];
      const action = this.mixer.clipAction(clip);
      action.enabled = true;
      action.clampWhenFinished = !Boolean(definition.loop);
      action.setLoop(definition.loop ? THREE.LoopRepeat : THREE.LoopOnce, definition.loop ? Infinity : 1);

      const result = {
        action,
        defaultLoop: Boolean(definition.loop),
        duration: clip.duration
      };
      this.actions.set(name, result);
      return result;
    })().finally(() => {
      this.clipPromises.delete(name);
    });

    this.clipPromises.set(name, promise);
    return promise;
  }

  playInitialBehavior() {
    if (this.context === 'result') {
      if (document.querySelector('.result-hero.approved')) this.play('celebrar', { force: true });
      else this.play('idle', { force: true });
      return;
    }

    if (this.context === 'tutor') {
      const form = document.querySelector('[data-tutor-form]');
      const limit = Number(form?.dataset.quotaLimit) || 0;
      const remaining = Number(form?.dataset.quotaRemaining) || 0;
      if (form?.dataset.tutorEnabled === 'false' || (limit > 0 && remaining <= 0)) {
        this.play('senalar', { force: true });
        return;
      }
    }

    this.play(this.initialAnimation || 'idle', { force: true });
  }

  play(name, options = {}) {
    if (!name || this.disposed) return false;

    const cooldownKey = options.cooldownKey || '';
    if (cooldownKey && !options.force) {
      const now = performance.now();
      const previous = this.lastTrigger.get(cooldownKey) || 0;
      const cooldown = Number(options.cooldownMs) || 4200;
      if (now - previous < cooldown) return false;
      this.lastTrigger.set(cooldownKey, now);
    }

    const token = ++this.commandToken;

    if (!this.manifest || !this.mixer) {
      this.pendingPlay = { name, options };
      return true;
    }

    const knownDefinition = this.manifest.clips?.[name];
    if (!knownDefinition) {
      console.warn(`EDU-02: animación no registrada: ${name}`);
      return false;
    }

    const requestedLoop = typeof options.loop === 'boolean' ? options.loop : Boolean(knownDefinition.loop);
    if (this.currentName === name && this.currentLoop === requestedLoop && !options.force) return true;

    this.ensureAction(name)
      .then(definition => {
        if (this.disposed || token !== this.commandToken || this.navigationSuspended) return;
        if (this.reducedMotion) {
          this.activateStaticAction(name, definition, options);
        } else {
          this.activateAction(name, options, definition);
        }
      })
      .catch(error => {
        if (this.disposed || error?.name === 'AbortError') return;
        console.warn(`EDU-02: no se pudo preparar ${name}.`, error);
        if (name !== 'idle') this.play('idle', { force: true });
      });

    return true;
  }

  activateAction(name, options = {}, suppliedDefinition = null) {
    const definition = suppliedDefinition || this.actions.get(name);
    if (!definition || this.disposed) return false;

    const loop = typeof options.loop === 'boolean' ? options.loop : definition.defaultLoop;
    const next = definition.action;
    const previous = this.currentAction;
    const fadeSeconds = options.immediate ? 0 : 0.2;

    next.enabled = true;
    next.reset();
    next.setEffectiveTimeScale(Number(options.speed) || 1);
    next.setEffectiveWeight(1);
    next.setLoop(loop ? THREE.LoopRepeat : THREE.LoopOnce, loop ? Infinity : 1);
    next.clampWhenFinished = !loop;
    next.fadeIn(fadeSeconds).play();

    if (previous && previous !== next) {
      previous.fadeOut(fadeSeconds);
      const stale = previous;
      window.setTimeout(() => {
        if (!this.disposed && stale !== this.currentAction) stale.stop();
      }, Math.ceil(fadeSeconds * 1000) + 40);
    }

    this.currentAction = next;
    this.currentName = name;
    this.currentLoop = loop;
    this.container.dataset.robotAnimation = name;
    return true;
  }

  activateStaticAction(name, definition, options = {}) {
    if (!definition || !this.mixer) return;
    this.actions.forEach(({ action }) => action.stop());
    const action = definition.action;
    action.reset().play();
    action.paused = true;
    action.time = definition.duration * THREE.MathUtils.clamp(options.endFrame ? 0.82 : 0.2, 0, 0.98);
    this.mixer.update(0);
    this.currentAction = action;
    this.currentName = name;
    this.currentLoop = false;
    this.renderer?.render(this.scene, this.camera);
  }

  renderStaticFrame(name, ratio = 0) {
    const definition = this.actions.get(name);
    if (!definition || !this.mixer) return;
    this.actions.forEach(({ action }) => action.stop());
    const action = definition.action;
    action.reset().play();
    action.paused = true;
    action.time = definition.duration * THREE.MathUtils.clamp(ratio, 0, 0.98);
    this.mixer.update(0);
    this.currentAction = action;
    this.currentName = name;
    this.currentLoop = false;
    this.renderer?.render(this.scene, this.camera);
  }

  onMixerFinished(event) {
    if (event.action !== this.currentAction || this.currentLoop || this.disposed) return;
    window.setTimeout(() => {
      if (event.action === this.currentAction && !this.disposed) this.play('idle', { force: true });
    }, 140);
  }

  scheduleContextPrefetch() {
    if (this.prefetchStarted || this.disposed) return;
    this.prefetchStarted = true;

    const names = this.context === 'auth'
      ? ['asentir', 'negar', 'pulgar_arriba']
      : this.context === 'tutor'
        ? ['pensar', 'senalar', 'asentir']
        : this.context === 'ranking' || this.context === 'result'
          ? ['celebrar']
          : ['asentir'];

    // Sólo calentamos el caché HTTP. No interpretamos GLB ni construimos
    // AnimationClips en segundo plano, para que ninguna tarea opcional pueda
    // secuestrar el hilo principal mientras el usuario usa la página.
    let index = 0;
    const prefetchNext = () => {
      if (this.disposed || this.navigationSuspended || index >= names.length) return;
      const name = names[index++];
      const url = this.resolveClipUrl(name);
      if (!url) return;

      fetch(url, {
        cache: 'force-cache',
        credentials: 'same-origin',
        priority: 'low',
        signal: this.abortController.signal
      })
        .then(response => response.ok ? response.arrayBuffer() : null)
        .catch(error => {
          if (error?.name !== 'AbortError') {
            console.debug(`EDU-02: precarga de red opcional omitida (${name}).`, error);
          }
        })
        .finally(() => {
          if (!this.disposed && !this.navigationSuspended && index < names.length) {
            window.setTimeout(prefetchNext, 350);
          }
        });
    };

    window.setTimeout(prefetchNext, 1200);
  }

  prepareModel() {
    this.modelRoot.position.set(0, 0, 0);
    this.modelRoot.scale.setScalar(1);
    this.modelRoot.updateMatrixWorld(true);
    this.robotScene.updateMatrixWorld(true);

    const box = new THREE.Box3().setFromObject(this.robotScene);
    if (box.isEmpty()) throw new Error('El robot no posee geometría visible.');

    const center = box.getCenter(new THREE.Vector3());
    const size = box.getSize(new THREE.Vector3());
    const targetHeight = this.context === 'home'
      ? 3.25
      : this.context === 'ranking' || this.context === 'result' || this.context === 'dashboard'
        ? 3.15
        : this.context === 'practice'
          ? 2.9
          : 3.05;
    const scale = targetHeight / Math.max(size.y, 0.001);

    this.modelRoot.scale.setScalar(scale);
    this.modelRoot.position.set(
      -center.x * scale,
      -box.min.y * scale,
      -center.z * scale
    );
    this.modelRoot.updateMatrixWorld(true);

    const fitted = new THREE.Box3().setFromObject(this.modelRoot);
    const fittedSize = fitted.getSize(new THREE.Vector3());
    const fittedCenter = fitted.getCenter(new THREE.Vector3());
    const verticalFov = THREE.MathUtils.degToRad(this.camera.fov);
    const horizontalFov = 2 * Math.atan(Math.tan(verticalFov / 2) * Math.max(this.camera.aspect, 0.01));
    const distanceHeight = fittedSize.y / (2 * Math.tan(verticalFov / 2));
    const distanceWidth = fittedSize.x / (2 * Math.tan(horizontalFov / 2));
    const actionMargin = this.context === 'ranking' || this.context === 'result'
      ? 1.72
      : this.context === 'auth'
        ? 1.5
        : 1.4;
    const distance = Math.max(distanceHeight, distanceWidth, 2.7) * actionMargin;

    fittedCenter.y += fittedSize.y * 0.03;
    this.camera.position.set(fittedCenter.x, fittedCenter.y, fittedCenter.z + distance);
    this.camera.near = Math.max(0.01, distance / 1000);
    this.camera.far = distance * 20;
    this.camera.lookAt(fittedCenter);
    this.camera.updateProjectionMatrix();
  }

  resolveHeadTarget() {
    if (!this.robotScene) return;
    const candidates = [
      'HDF_V702_HeadProportionRoot',
      'HDF_HeadV5Root',
      'HDF_HeadV5Shell'
    ];

    this.headTarget = candidates
      .map(name => this.robotScene.getObjectByName(name))
      .find(Boolean) || null;
  }

  bindContextInteractions() {
    if (this.context === 'home') {
      this.bindHover('[class*="hero-actions"] .button, .home-cta-v296 .button', 'asentir', 'home-button');
    }

    if (this.context === 'dashboard') {
      this.bindHover('.module-row, .activity-card .button, .dashboard-hero-actions .button', 'asentir', 'dashboard-exercise');
    }

    if (this.context === 'practice') {
      this.bindHover(
        '.exercise-row .button, .topic-card .card-link, [data-practice-action]',
        'asentir',
        'practice-action'
      );
    }

    if (this.context === 'auth') {
      const forms = document.querySelectorAll('[data-auth-form]');
      forms.forEach(form => {
        form.addEventListener('focusin', event => {
          if (event.target.matches('input, textarea, select')) {
            this.play('asentir', { cooldownKey: 'auth-entry', cooldownMs: 5200 });
          }
        });

        form.addEventListener('input', event => {
          if (event.target.matches('input, textarea, select')) {
            this.play('asentir', { cooldownKey: 'auth-entry', cooldownMs: 7200 });
          }
        });

        form.addEventListener('invalid', () => this.play('negar', { force: true }), true);
        form.addEventListener('submit', () => {
          if (typeof form.checkValidity !== 'function' || form.checkValidity()) {
            this.play('asentir', { force: true });
          }
        });
      });
    }

    this.bindHover('[data-robot-hover]', null, 'explicit-hover');
  }

  bindHover(selector, defaultAnimation, cooldownPrefix) {
    document.querySelectorAll(selector).forEach(element => {
      element.addEventListener('pointerenter', () => {
        const animation = element.dataset.robotHover || defaultAnimation;
        if (!animation) return;
        this.play(animation, {
          cooldownKey: cooldownPrefix,
          cooldownMs: 4400
        });
      }, { passive: true });
    });
  }

  bindLifecycle() {
    document.addEventListener('pointermove', this.onPointerMove, { passive: true });
    document.documentElement.addEventListener('pointerleave', this.onPointerLeave, { passive: true });
    document.addEventListener('visibilitychange', this.onVisibilityChange);
    window.addEventListener('pagehide', this.onPageHide);
    window.addEventListener('pageshow', this.onPageShow);
  }

  onVisibilityChange() {
    this.pageVisible = !document.hidden;
    this.clock.getDelta();
    this.syncRenderLoop();
  }

  onPageHide(event) {
    this.suspendForNavigation();
    if (!event?.persisted) this.abortController?.abort();
  }

  onPageShow() {
    if (this.disposed) return;
    this.navigationSuspended = false;
    this.pageVisible = !document.hidden;
    this.clock.getDelta();
    this.syncRenderLoop();
  }

  suspendForNavigation() {
    if (this.disposed || this.navigationSuspended) return;
    this.navigationSuspended = true;
    ++this.commandToken;
    this.syncRenderLoop();
  }

  onPointerMove(event) {
    if (this.reducedMotion) return;
    this.pointer.targetX = THREE.MathUtils.clamp((event.clientX / window.innerWidth) * 2 - 1, -1, 1);
    this.pointer.targetY = THREE.MathUtils.clamp((event.clientY / window.innerHeight) * 2 - 1, -1, 1);
  }

  onPointerLeave() {
    this.pointer.targetX = 0;
    this.pointer.targetY = 0;
  }

  applyCursorLook(delta) {
    if (!this.headTarget || this.reducedMotion) return;

    this.pointer.x = THREE.MathUtils.damp(this.pointer.x, this.pointer.targetX, 7.5, delta);
    this.pointer.y = THREE.MathUtils.damp(this.pointer.y, this.pointer.targetY, 7.5, delta);

    const yaw = this.pointer.x * 0.18;
    const pitch = -this.pointer.y * 0.105;
    this.headEuler.set(pitch, yaw, 0, 'YXZ');
    this.headOffset.setFromEuler(this.headEuler);
    this.headTarget.quaternion.multiply(this.headOffset);
    this.headTarget.updateMatrixWorld(true);
  }

  observeSize() {
    const observer = new ResizeObserver(entries => {
      const rect = entries[0]?.contentRect;
      if (!rect || !this.renderer) return;
      const width = Math.max(1, rect.width);
      const height = Math.max(1, rect.height);
      this.camera.aspect = width / height;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(width, height, false);
      if (this.robotScene) this.prepareModel();
    });

    observer.observe(this.container);
    this.resizeObserver = observer;
  }

  observeVisibility() {
    const observer = new IntersectionObserver(entries => {
      this.widgetVisible = entries[0]?.isIntersecting ?? true;
      this.clock.getDelta();
      this.syncRenderLoop();
    }, { threshold: 0.01 });

    observer.observe(this.container);
    this.visibilityObserver = observer;
  }

  syncRenderLoop() {
    if (!this.renderer || this.reducedMotion || !this.readyForRender) return;

    const shouldRender = !this.disposed &&
      !this.navigationSuspended &&
      this.pageVisible &&
      this.widgetVisible;

    if (shouldRender && !this.renderLoopActive) {
      this.lastFrameAt = 0;
      this.clock.getDelta();
      this.renderer.setAnimationLoop(this.render);
      this.renderLoopActive = true;
      return;
    }

    if (!shouldRender && this.renderLoopActive) {
      this.renderer.setAnimationLoop(null);
      this.renderLoopActive = false;
    }
  }

  render(timestamp = performance.now()) {
    if (this.disposed || !this.renderer || this.navigationSuspended) return;
    if (!this.pageVisible || !this.widgetVisible) return;

    if (this.lastFrameAt > 0 && timestamp - this.lastFrameAt < this.frameInterval) return;
    this.lastFrameAt = timestamp;

    const delta = Math.min(this.clock.getDelta(), 0.08);
    this.mixer?.update(delta);
    this.applyCursorLook(delta);
    this.renderer.render(this.scene, this.camera);
  }

  dispose() {
    if (this.disposed) return;
    this.disposed = true;
    this.navigationSuspended = true;
    ++this.commandToken;
    this.abortController?.abort();

    if (this.renderLoopActive) {
      this.renderer?.setAnimationLoop(null);
      this.renderLoopActive = false;
    }

    this.mixer?.removeEventListener('finished', this.onMixerFinished);
    this.resizeObserver?.disconnect();
    this.visibilityObserver?.disconnect();
    document.removeEventListener('pointermove', this.onPointerMove);
    document.documentElement.removeEventListener('pointerleave', this.onPointerLeave);
    document.removeEventListener('visibilitychange', this.onVisibilityChange);
    window.removeEventListener('pagehide', this.onPageHide);
    window.removeEventListener('pageshow', this.onPageShow);
  }
}
