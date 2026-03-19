/**
 * Centralized API wrapper.
 * Automatically attaches the JWT Bearer token to every request.
 * On 401, attempts a silent token refresh, then retries once.
 */
const API = {
  baseUrl: '/api',

  async request(method, path, body = null, retry = true) {
    const headers = { 'Content-Type': 'application/json' };
    const token = localStorage.getItem('accessToken');
    if (token) headers['Authorization'] = `Bearer ${token}`;

    const options = { method, headers };
    if (body !== null) options.body = JSON.stringify(body);

    const res = await fetch(`${this.baseUrl}${path}`, options);

    if (res.status === 401 && retry) {
      const refreshed = await this.tryRefresh();
      if (refreshed) return this.request(method, path, body, false);
      Auth.logout();
      return null;
    }

    if (res.status === 204) return null;

    const data = await res.json().catch(() => null);
    if (!res.ok) throw new Error(data?.error || `HTTP ${res.status}`);
    return data;
  },

  async tryRefresh() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (!refreshToken) return false;
    try {
      const res = await fetch(`${this.baseUrl}/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ refreshToken }),
      });
      if (!res.ok) return false;
      const data = await res.json();
      localStorage.setItem('accessToken', data.accessToken);
      localStorage.setItem('refreshToken', data.refreshToken);
      return true;
    } catch {
      return false;
    }
  },

  get:    (path)       => API.request('GET',    path),
  post:   (path, body) => API.request('POST',   path, body),
  put:    (path, body) => API.request('PUT',    path, body),
  patch:  (path, body) => API.request('PATCH',  path, body),
  delete: (path)       => API.request('DELETE', path),
};
