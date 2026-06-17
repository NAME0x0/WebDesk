// JSON-RPC client over the WebView2 message channel.
(function () {
  const pending = new Map();
  let seq = 0;
  const wv = window.chrome && window.chrome.webview;

  if (wv) {
    wv.addEventListener('message', (e) => {
      let msg;
      try { msg = typeof e.data === 'string' ? JSON.parse(e.data) : e.data; }
      catch { return; }
      const p = pending.get(msg.id);
      if (!p) return;
      pending.delete(msg.id);
      if (msg.ok) p.resolve(msg.result);
      else p.reject(new Error(msg.error || 'bridge error'));
    });
  }

  // window.api(method, params) -> Promise<result>
  window.api = function (method, params) {
    if (!wv) return Promise.reject(new Error('WebView2 bridge unavailable (open inside WebDesk)'));
    return new Promise((resolve, reject) => {
      const id = ++seq;
      pending.set(id, { resolve, reject });
      wv.postMessage(JSON.stringify({ id, method, params: params || {} }));
    });
  };
})();
