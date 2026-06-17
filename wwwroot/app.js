'use strict';

const $ = (s, r = document) => r.querySelector(s);
const $$ = (s, r = document) => [...r.querySelectorAll(s)];

const state = {
  settings: {},
  current: null,
  paused: false,
  version: '2.0.0',
  startupEnabled: false,
  monitors: [],
  target: 'all',
  catalog: null,
  filter: 'all',
  discoverSource: 'catalog',
  whQuery: '',
  whPage: 1,
  whLast: 1,
};

const REPO_URL = 'https://github.com/NAME0x0/WebDesk';

// ---------- boot ----------
async function boot() {
  try {
    const st = await api('getState');
    state.settings = st.settings || {};
    state.current = st.current;
    state.paused = st.paused;
    state.version = st.version || state.version;
    state.startupEnabled = st.startupEnabled;
    state.monitors = st.monitors || [];
    if (state.startupEnabled && !state.settings.runAtStartup) state.settings.runAtStartup = true;
  } catch (e) {
    console.warn('bridge unavailable', e);
  }

  $('#ver').textContent = 'v' + state.version;
  applyTheme();
  populateMonitors();
  updateStatus();
  updatePauseBtn();
  bindNav();
  bindTopbar();
  route(currentRoute());
}

function applyTheme() {
  const s = state.settings || {};
  document.documentElement.dataset.theme = s.theme === 'light' ? 'light' : 'dark';
  if (s.accent) document.documentElement.style.setProperty('--accent', s.accent);
}

function currentRoute() { return location.hash.replace('#', '') || 'home'; }

function bindNav() {
  $$('#nav a').forEach((a) => (a.onclick = () => navigate(a.dataset.route)));
  window.addEventListener('hashchange', () => route(currentRoute()));
}
function navigate(r) { location.hash = r; }
function route(r) {
  const parts = r.split('/');
  const base = parts[0] || 'home';
  if (browserActive && base !== 'browser') leaveBrowser();
  $$('#nav a').forEach((a) => a.classList.toggle('active', a.dataset.route === base));

  // Deep-link sub-routes: #discover/wallhaven/<query>, #library/playlist, etc.
  if (base === 'discover') {
    state.discoverSource = parts[1] === 'wallhaven' ? 'wallhaven' : 'catalog';
    if (parts[2]) state.whQuery = decodeURIComponent(parts[2]);
  }
  if (base === 'library') libraryTab = parts[1] || 'saved';

  ({
    home: renderHome, discover: renderDiscover, browser: renderBrowser,
    library: renderLibrary, settings: renderSettings,
  }[base] || renderHome)();
}

function bindTopbar() {
  $('#urlBtn').onclick = applyUrl;
  $('#urlInput').addEventListener('keydown', (e) => { if (e.key === 'Enter') applyUrl(); });
  $('#pauseBtn').onclick = togglePause;
  $('#monitorSel').onchange = (e) => { state.target = e.target.value; };
}

function populateMonitors() {
  const sel = $('#monitorSel');
  if (state.monitors.length <= 1) { sel.style.display = 'none'; return; }
  sel.style.display = '';
  sel.innerHTML = '<option value="all">All monitors</option>' +
    state.monitors.map((m, i) => `<option value="${esc(m.id)}">${esc('Monitor ' + (i + 1) + ' · ' + m.label)}</option>`).join('');
}
function monitorLabel(id) {
  const m = state.monitors.find((x) => x.id === id);
  return m ? m.label : id;
}

// ---------- pages ----------
async function renderHome() {
  const v = $('#view');
  v.innerHTML = '';

  const cur = state.current;
  const hero = document.createElement('div');
  hero.className = 'hero';
  hero.innerHTML = `
    <div class="eyebrow">Current wallpaper</div>
    <h1>${cur ? esc(cur.title) : 'No wallpaper set'}</h1>
    <p>${cur
      ? esc(cur.type.toUpperCase() + ' · ' + (cur.author || cur.origin))
      : 'Pick something from Discover, your library, or paste a URL above.'}</p>
    <div class="hero-actions">
      <button id="heroDiscover">Browse Discover</button>
      <button id="heroPause" class="ghost">${state.paused ? 'Resume' : 'Pause'}</button>
    </div>`;
  v.appendChild(hero);
  $('#heroDiscover').onclick = () => navigate('discover');
  $('#heroPause').onclick = togglePause;

  const cat = await getCatalog();
  if (cat.length) v.appendChild(section('Featured', cat.slice(0, 8)));
}

function renderDiscover() {
  const v = $('#view');
  v.innerHTML = '';

  const head = document.createElement('div');
  head.className = 'page-head';
  head.innerHTML = `
    <div class="src-tabs">
      <button data-src="catalog" class="${state.discoverSource === 'catalog' ? 'active' : ''}">Catalog</button>
      <button data-src="wallhaven" class="${state.discoverSource === 'wallhaven' ? 'active' : ''}">Wallhaven</button>
    </div>
    <button id="submitBtn" class="ghost">+ Submit a wallpaper</button>`;
  v.appendChild(head);
  head.querySelectorAll('.src-tabs button').forEach((b) =>
    (b.onclick = () => { state.discoverSource = b.dataset.src; renderDiscover(); }));
  $('#submitBtn').onclick = openSubmitModal;

  if (state.discoverSource === 'wallhaven') renderWallhaven(v);
  else renderCatalog(v);
}

async function renderCatalog(v) {
  const loading = document.createElement('div');
  loading.className = 'loading';
  loading.textContent = 'Loading catalog…';
  v.appendChild(loading);
  const cat = await getCatalog();
  loading.remove();

  const tags = ['all', ...new Set(cat.flatMap((w) => w.tags || []))];
  const bar = document.createElement('div');
  bar.className = 'filters';
  tags.forEach((t) => {
    const b = document.createElement('button');
    b.className = 'chip' + (state.filter === t ? ' active' : '');
    b.textContent = t;
    b.onclick = () => { state.filter = t; renderDiscover(); };
    bar.appendChild(b);
  });
  v.appendChild(bar);

  const list = state.filter === 'all' ? cat : cat.filter((w) => (w.tags || []).includes(state.filter));
  const grid = document.createElement('div');
  grid.className = 'grid';
  if (!list.length) grid.innerHTML = '<div class="empty">Nothing here yet.</div>';
  list.forEach((w) => grid.appendChild(card(w)));
  v.appendChild(grid);
}

function renderWallhaven(v) {
  const sr = document.createElement('div');
  sr.className = 'search-row';
  sr.innerHTML = `<input id="whq" class="b-url" type="text" spellcheck="false"
    placeholder="Search Wallhaven — nature, anime, cyberpunk…" value="${esc(state.whQuery)}">
    <button id="whGo">Search</button>`;
  v.appendChild(sr);

  const grid = document.createElement('div');
  grid.className = 'grid';
  v.appendChild(grid);
  const more = document.createElement('div');
  more.className = 'load-more';
  v.appendChild(more);

  async function search(reset) {
    if (reset) { state.whPage = 1; grid.innerHTML = '<div class="loading">Searching…</div>'; }
    let r;
    try { r = await api('searchProvider', { provider: 'wallhaven', query: state.whQuery, page: state.whPage }); }
    catch (e) { grid.innerHTML = `<div class="empty">${esc(e.message)}</div>`; return; }

    state.whLast = r.lastPage || 1;
    if (reset) grid.innerHTML = '';
    (r.wallpapers || []).forEach((w) => grid.appendChild(card(w)));
    if (!grid.children.length) grid.innerHTML = '<div class="empty">No results.</div>';

    more.innerHTML = '';
    if (state.whPage < state.whLast) {
      const b = document.createElement('button');
      b.className = 'ghost';
      b.textContent = 'Load more';
      b.onclick = () => { state.whPage++; search(false); };
      more.appendChild(b);
    }
  }

  $('#whGo').onclick = () => { state.whQuery = $('#whq').value.trim(); search(true); };
  $('#whq').addEventListener('keydown', (e) => { if (e.key === 'Enter') { state.whQuery = $('#whq').value.trim(); search(true); } });
  search(true);
}

async function renderLibrary() {
  const v = $('#view');
  v.innerHTML = '';

  const tabs = document.createElement('div');
  tabs.className = 'tabs';
  tabs.innerHTML =
    '<button data-tab="saved" class="active">Saved &amp; Starred</button>' +
    '<button data-tab="local">Local Folder</button>' +
    '<button data-tab="playlist">Playlist</button>';
  v.appendChild(tabs);

  const body = document.createElement('div');
  v.appendChild(body);

  tabs.querySelectorAll('button').forEach((b) => {
    b.classList.toggle('active', b.dataset.tab === libraryTab);
    b.onclick = () => {
      tabs.querySelectorAll('button').forEach((x) => x.classList.remove('active'));
      b.classList.add('active');
      fill(b.dataset.tab);
    };
  });

  async function fill(tab) {
    body.innerHTML = '<div class="loading">Loading…</div>';

    if (tab === 'playlist') { renderPlaylist(body); return; }

    if (tab === 'saved') {
      const lib = await api('getLibrary');
      body.innerHTML = '';
      if (!lib.length) {
        body.innerHTML = '<div class="empty">No saved wallpapers yet. Save some from Discover.</div>';
        return;
      }
      const grid = document.createElement('div');
      grid.className = 'grid';
      lib.forEach((w) => grid.appendChild(libCard(w)));
      body.appendChild(grid);
      return;
    }

    if (!state.settings.libraryFolder) {
      body.innerHTML = '<div class="empty">No library folder set. <button id="pickHere">Choose folder</button></div>';
      $('#pickHere', body).onclick = async () => { if (await pickFolder()) fill('local'); };
      return;
    }

    const loc = await api('getLocal');
    body.innerHTML =
      `<div class="folderbar">Folder: <code>${esc(state.settings.libraryFolder)}</code>` +
      ' <button id="changeFolder" class="ghost">Change</button></div>';
    $('#changeFolder', body).onclick = async () => { if (await pickFolder()) fill('local'); };

    const grid = document.createElement('div');
    grid.className = 'grid';
    if (!loc.length) grid.innerHTML = '<div class="empty">No HTML, video, or image files found in that folder.</div>';
    loc.forEach((w) => grid.appendChild(card(w)));
    body.appendChild(grid);
  }

  fill(libraryTab);
}

async function renderSettings() {
  const s = state.settings || {};
  const fps = s.fpsCap || 0;
  const v = $('#view');
  v.innerHTML = `
    <div class="settings">
      <h1>Settings</h1>
      <h2 class="group">General</h2>
      ${settingRow('Run at startup', 'Launch WebDesk (to tray) when you sign in.', toggle('setStartup', s.runAtStartup))}
      ${settingRow('Check for updates', 'Automatically look for new releases.', toggle('setUpdates', s.autoCheckUpdates))}
      ${settingRow('Mute audio', 'Silence video wallpapers.', toggle('setMute', s.muteAudio))}
      ${settingRow('Library folder', `<code id="folderText">${esc(s.libraryFolder || 'Not set')}</code>`,
        '<button id="setFolder" class="ghost">Choose…</button>')}

      <h2 class="group">Appearance</h2>
      ${settingRow('Theme', 'Light or dark interface.',
        `<select id="setTheme" class="select">
           <option value="dark" ${s.theme !== 'light' ? 'selected' : ''}>Dark</option>
           <option value="light" ${s.theme === 'light' ? 'selected' : ''}>Light</option>
         </select>`)}
      ${settingRow('Accent color', 'Highlight color across the app.',
        `<input id="setAccent" type="color" class="color" value="${esc(s.accent || '#6c8cff')}">`)}

      <h2 class="group">Performance</h2>
      ${settingRow('Pause when an app is fullscreen', 'Stops the wallpaper during games and fullscreen video.', toggle('setFs', s.pauseOnFullscreen))}
      ${settingRow('Pause on battery', 'Stops the wallpaper while running on battery power.', toggle('setBat', s.pauseOnBattery))}
      ${settingRow('Frame rate cap', 'Limits shader wallpapers to save GPU/battery.',
        `<select id="setFps" class="select">
           <option value="0" ${fps === 0 ? 'selected' : ''}>Unlimited</option>
           <option value="60" ${fps === 60 ? 'selected' : ''}>60 FPS</option>
           <option value="30" ${fps === 30 ? 'selected' : ''}>30 FPS</option>
         </select>`)}

      <h2 class="group">About</h2>
      ${settingRow('Updates', `Current version v${esc(state.version)}.`, '<button id="checkUpdate" class="ghost">Check now</button>')}
      <div class="about">
        <img src="logo.svg" alt="">
        <div><b>WebDesk</b> v${esc(state.version)}<br><a id="repoLink" href="#">github.com/NAME0x0/WebDesk</a></div>
      </div>
    </div>`;

  const save = async () => {
    const next = {
      ...state.settings,
      runAtStartup: $('#setStartup').checked,
      autoCheckUpdates: $('#setUpdates').checked,
      muteAudio: $('#setMute').checked,
      pauseOnFullscreen: $('#setFs').checked,
      pauseOnBattery: $('#setBat').checked,
      fpsCap: parseInt($('#setFps').value, 10),
      theme: $('#setTheme').value,
      accent: $('#setAccent').value,
    };
    state.settings = await api('setSettings', { settings: next });
    applyTheme();
    toast('Settings saved');
  };
  ['setStartup', 'setUpdates', 'setMute', 'setFs', 'setBat', 'setFps', 'setTheme', 'setAccent']
    .forEach((id) => ($('#' + id).onchange = save));
  $('#setFolder').onclick = async () => {
    if (await pickFolder()) $('#folderText').textContent = state.settings.libraryFolder || 'Not set';
  };
  $('#checkUpdate').onclick = checkUpdate;
  $('#repoLink').onclick = (e) => { e.preventDefault(); api('openExternal', { url: REPO_URL }); };
}

function settingRow(title, sub, control) {
  return `<div class="row"><div><div class="row-t">${title}</div><div class="row-s">${sub}</div></div>${control}</div>`;
}
function toggle(id, on) {
  return `<label class="switch"><input type="checkbox" id="${id}" ${on ? 'checked' : ''}><span></span></label>`;
}

// ---------- browser ----------
const BOOKMARKS = [
  { name: 'Moewalls', url: 'https://moewalls.com/' },
  { name: 'MotionBGs', url: 'https://motionbgs.com/' },
  { name: 'MyLiveWallpapers', url: 'https://mylivewallpapers.com/' },
  { name: '4KWallpapers', url: 'https://4kwallpapers.com/' },
  { name: 'Unsplash', url: 'https://unsplash.com/wallpapers' },
  { name: 'Pinterest', url: 'https://www.pinterest.com/' },
  { name: 'X', url: 'https://x.com/' },
];
let browserActive = false;
let browserRO = null;
let libraryTab = 'saved';

function renderBrowser() {
  const v = $('#view');
  v.innerHTML = '';
  v.classList.add('browser-page');

  const bar = document.createElement('div');
  bar.className = 'browser-bar';
  bar.innerHTML = `
    <div class="nav-btns">
      <button id="bBack" class="ghost" title="Back">‹</button>
      <button id="bFwd" class="ghost" title="Forward">›</button>
      <button id="bReload" class="ghost" title="Reload">⟳</button>
    </div>
    <input id="bUrl" class="b-url" type="text" spellcheck="false" placeholder="Search or enter a website…">
    <button id="bGo">Go</button>
    <div class="cap-btns">
      <button id="capMedia" title="Set the main image/video on this page">Set image / video</button>
      <button id="capPage" class="ghost" title="Set this whole page, live">Set live page</button>
      <button id="capSave" class="ghost">Save</button>
    </div>`;

  const marks = document.createElement('div');
  marks.className = 'bookmarks';
  BOOKMARKS.forEach((b) => {
    const x = document.createElement('button');
    x.className = 'chip';
    x.textContent = b.name;
    x.onclick = () => navTo(b.url);
    marks.appendChild(x);
  });

  const slot = document.createElement('div');
  slot.id = 'browserSlot';
  slot.className = 'browser-slot';
  slot.innerHTML = '<div class="browser-hint">Pick a site, browse to a wallpaper, then “Set image / video” or “Set live page”.</div>';

  v.append(bar, marks, slot);

  $('#bGo').onclick = () => navTo($('#bUrl').value.trim());
  $('#bUrl').addEventListener('keydown', (e) => { if (e.key === 'Enter') navTo($('#bUrl').value.trim()); });
  $('#bBack').onclick = () => api('browserBack');
  $('#bFwd').onclick = () => api('browserForward');
  $('#bReload').onclick = () => api('browserReload');
  $('#capMedia').onclick = () => capture('media', false);
  $('#capPage').onclick = () => capture('page', false);
  $('#capSave').onclick = () => capture('media', true);

  browserActive = true;
  positionBrowser(true);
  browserRO = new ResizeObserver(() => positionBrowser(false));
  browserRO.observe(slot);
  window.addEventListener('resize', onBrowserResize);
}

function onBrowserResize() { if (browserActive) positionBrowser(false); }

function leaveBrowser() {
  browserActive = false;
  if (browserRO) { browserRO.disconnect(); browserRO = null; }
  window.removeEventListener('resize', onBrowserResize);
  $('#view').classList.remove('browser-page');
  api('browserHide');
}

function slotRect() {
  const el = $('#browserSlot');
  if (!el) return null;
  const r = el.getBoundingClientRect();
  return { x: Math.round(r.left), y: Math.round(r.top), w: Math.round(r.width), h: Math.round(r.height) };
}
async function positionBrowser(initial) {
  const r = slotRect();
  if (!r) return;
  if (initial) await api('browserShow', r);
  else api('browserMove', r);
}

function navTo(input) {
  if (!input) return;
  let url = input;
  if (!/:\/\//.test(url)) {
    url = /\.\w/.test(url) && !url.includes(' ')
      ? 'https://' + url
      : 'https://duckduckgo.com/?q=' + encodeURIComponent(url);
  }
  $('#bUrl').value = url;
  api('browserNavigate', { url });
}

async function capture(mode, save) {
  try {
    const w = await api('browserCapture', { mode });
    if (!w || !w.source) { toast('Nothing to capture on this page', true); return; }
    if (save) { await api('save', { wallpaper: w }); toast('Saved to library'); }
    else await applyWp(w);
  } catch (e) { toast(e.message, true); }
}

async function renderPlaylist(body) {
  const pl = await api('getPlaylist');
  const s = state.settings || {};
  body.innerHTML = `
    <div class="playlist-controls">
      <div class="ctl">Rotate every
        <select id="rotSel" class="select">
          <option value="0">Off</option><option value="5">5 min</option><option value="10">10 min</option>
          <option value="15">15 min</option><option value="30">30 min</option><option value="60">60 min</option>
        </select>
      </div>
      <div class="ctl">Shuffle <label class="switch"><input type="checkbox" id="rotShuffle"><span></span></label></div>
    </div>
    <div id="plGrid" class="grid"></div>`;
  $('#rotSel').value = String(s.rotateMinutes || 0);
  $('#rotShuffle').checked = !!s.shuffle;

  const saveRot = async () => {
    const minutes = parseInt($('#rotSel').value, 10);
    const shuffle = $('#rotShuffle').checked;
    await api('setRotation', { minutes, shuffle });
    state.settings.rotateMinutes = minutes;
    state.settings.shuffle = shuffle;
    toast('Rotation updated');
  };
  $('#rotSel').onchange = saveRot;
  $('#rotShuffle').onchange = saveRot;

  const grid = $('#plGrid');
  if (!pl.length) {
    grid.innerHTML = '<div class="empty">Playlist is empty. Add wallpapers with the ＋ button on any card.</div>';
    return;
  }
  pl.forEach((w) => {
    const el = card(w);
    const rm = document.createElement('button');
    rm.className = 'ghost remove';
    rm.textContent = 'Remove';
    rm.onclick = async () => { await api('playlistRemove', { id: w.id }); el.remove(); };
    $('.actions', el).appendChild(rm);
    grid.appendChild(el);
  });
}

async function addToPlaylist(w) {
  try { await api('playlistAdd', { wallpaper: w }); toast('Added to playlist'); }
  catch (e) { toast(e.message, true); }
}

// ---------- submit modal ----------
function openSubmitModal() {
  const overlay = document.createElement('div');
  overlay.className = 'modal';
  overlay.innerHTML = `
    <div class="modal-box">
      <h2>Submit a wallpaper</h2>
      <p class="row-s">Opens a pre-filled GitHub issue and copies the catalog entry to your clipboard.</p>
      <label>Title<input id="mTitle" type="text"></label>
      <label>Type
        <select id="mType" class="select">
          <option value="url">URL (live web page)</option>
          <option value="image">Image</option>
          <option value="video">Video</option>
          <option value="shader">Shader (GLSL)</option>
        </select>
      </label>
      <label>Source (URL, or GLSL for shader)<textarea id="mSource" rows="3"></textarea></label>
      <label>Tags (comma separated)<input id="mTags" type="text" placeholder="nature, abstract"></label>
      <label>Author<input id="mAuthor" type="text"></label>
      <div class="modal-actions">
        <button id="mCancel" class="ghost">Cancel</button>
        <button id="mSubmit">Submit</button>
      </div>
    </div>`;
  document.body.appendChild(overlay);

  const close = () => overlay.remove();
  overlay.onclick = (e) => { if (e.target === overlay) close(); };
  $('#mCancel', overlay).onclick = close;
  $('#mSubmit', overlay).onclick = async () => {
    const title = $('#mTitle', overlay).value.trim();
    const source = $('#mSource', overlay).value.trim();
    if (!title || !source) { toast('Title and source are required', true); return; }
    const type = $('#mType', overlay).value;
    const w = {
      id: 'submit:' + title.toLowerCase().replace(/\s+/g, '-'),
      title, type, source,
      tags: $('#mTags', overlay).value.split(',').map((t) => t.trim()).filter(Boolean),
      author: $('#mAuthor', overlay).value.trim() || undefined,
      audio: type === 'shader',
      origin: 'custom',
    };
    try { await api('submitWallpaper', { wallpaper: w }); toast('Opening GitHub submission…'); close(); }
    catch (e) { toast(e.message, true); }
  };
}

// ---------- components ----------
function section(title, list) {
  const wrap = document.createElement('section');
  wrap.className = 'block';
  wrap.innerHTML = `<h2>${esc(title)}</h2>`;
  const grid = document.createElement('div');
  grid.className = 'grid';
  list.forEach((w) => grid.appendChild(card(w)));
  wrap.appendChild(grid);
  return wrap;
}

function card(w) {
  const el = document.createElement('div');
  el.className = 'card';
  const thumb = thumbFor(w);
  el.innerHTML = `
    <div class="thumb" ${thumb ? `style="background-image:url('${cssUrl(thumb)}')"` : ''}>
      <span class="badge">${esc(w.type)}</span>
      ${thumb ? '' : `<div class="noimg">${iconFor(w.type)}</div>`}
    </div>
    <div class="meta">
      <div class="title" title="${esc(w.title)}">${esc(w.title)}</div>
      ${w.author ? `<div class="author">${esc(w.author)}</div>` : ''}
    </div>
    <div class="actions">
      <button class="apply">Set</button>
      <button class="save ghost">Save</button>
      <button class="star ghost" title="Star">${w.starred ? '★' : '☆'}</button>
      <button class="plist ghost" title="Add to playlist">＋</button>
    </div>`;
  $('.apply', el).onclick = () => applyWp(w);
  $('.save', el).onclick = () => saveWp(w);
  $('.star', el).onclick = (e) => starWp(w, e.currentTarget);
  $('.plist', el).onclick = () => addToPlaylist(w);
  return el;
}

function libCard(w) {
  const el = card(w);
  const rm = document.createElement('button');
  rm.className = 'ghost remove';
  rm.textContent = 'Remove';
  rm.onclick = async () => { await api('remove', { id: w.id }); el.remove(); };
  $('.actions', el).appendChild(rm);
  return el;
}

// ---------- actions ----------
async function applyWp(w) {
  try {
    await api('apply', { wallpaper: w, target: state.target });
    if (state.target === 'all') {
      state.current = w;
      state.paused = false;
      updateStatus();
      updatePauseBtn();
    }
    toast('Applied: ' + w.title + (state.target !== 'all' ? ' → ' + monitorLabel(state.target) : ''));
  } catch (e) { toast(e.message, true); }
}

async function saveWp(w) {
  try { await api('save', { wallpaper: w }); toast('Saved to library'); }
  catch (e) { toast(e.message, true); }
}

async function starWp(w, btn) {
  try {
    await api('save', { wallpaper: w });
    const r = await api('toggleStar', { id: w.id });
    btn.textContent = r.starred ? '★' : '☆';
  } catch (e) { toast(e.message, true); }
}

async function applyUrl() {
  let url = $('#urlInput').value.trim();
  if (!url) return;
  if (!/:\/\//.test(url)) url = 'https://' + url;
  await applyWp({ id: 'custom:' + url, title: hostOf(url), type: 'url', source: url, origin: 'custom', tags: [] });
  $('#urlInput').value = '';
}

async function togglePause() {
  try {
    const r = await api('pauseToggle');
    state.paused = r.paused;
    updatePauseBtn();
    updateStatus();
    const hp = $('#heroPause');
    if (hp) hp.textContent = state.paused ? 'Resume' : 'Pause';
  } catch (e) { toast(e.message, true); }
}

async function pickFolder() {
  const r = await api('pickFolder');
  if (!r || !r.folder) return false;
  state.settings = await api('setSettings', { settings: { ...state.settings, libraryFolder: r.folder } });
  return true;
}

async function checkUpdate() {
  toast('Checking for updates…');
  try {
    const r = await api('checkUpdate');
    if (r.update) {
      toast('Update ' + r.update.version + ' available');
      if (confirm('WebDesk ' + r.update.version + ' is available.\n\nOpen the releases page?'))
        api('openExternal', { url: REPO_URL + '/releases/latest' });
    } else {
      toast("You're up to date");
    }
  } catch (e) { toast(e.message, true); }
}

async function getCatalog(refresh) {
  if (!refresh && state.catalog) return state.catalog;
  try { state.catalog = await api('getCatalog', { refresh: !!refresh }); }
  catch { state.catalog = []; }
  return state.catalog;
}

// ---------- status ----------
function updateStatus() {
  const s = $('#status');
  if (!state.current) { s.innerHTML = '<span class="dot off"></span>No wallpaper'; return; }
  const cls = state.paused ? 'paused' : 'live';
  s.innerHTML = `<span class="dot ${cls}"></span>${state.paused ? 'Paused' : 'Live'} · ${esc(state.current.title)}`;
}
function updatePauseBtn() { $('#pauseBtn').textContent = state.paused ? 'Resume' : 'Pause'; }

// ---------- toast ----------
let toastTimer;
function toast(msg, isError) {
  const t = $('#toast');
  t.textContent = msg;
  t.className = 'toast show' + (isError ? ' error' : '');
  clearTimeout(toastTimer);
  toastTimer = setTimeout(() => (t.className = 'toast'), 2600);
}

// ---------- helpers ----------
function esc(s) {
  return String(s).replace(/[&<>"']/g, (c) =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
}
function cssUrl(u) { return String(u).replace(/['"\\]/g, '\\$&'); }
function thumbFor(w) {
  if (w.thumbnail) return w.thumbnail;
  if (w.type === 'url') return 'https://image.thum.io/get/width/600/' + w.source;
  return null;
}
function iconFor(t) { return { video: '▶', image: '🖼', html: '&lt;/&gt;', url: '🌐', shader: '◈' }[t] || '🌐'; }
function hostOf(u) { try { return new URL(u).hostname.replace(/^www\./, ''); } catch { return u; } }

boot();
