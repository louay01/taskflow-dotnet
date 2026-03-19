/**
 * Auth helpers — login, register, logout, token storage, role checks.
 */
const Auth = {
  getUser() {
    const token = localStorage.getItem('accessToken');
    if (!token) return null;
    try {
      return JSON.parse(atob(token.split('.')[1]));
    } catch {
      return null;
    }
  },

  isLoggedIn() { return !!localStorage.getItem('accessToken'); },

  hasRole(role) {
    const user = this.getUser();
    if (!user) return false;
    const roles = Array.isArray(user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
      ? user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']
      : [user['http://schemas.microsoft.com/ws/2008/06/identity/claims/role']];
    return roles.includes(role);
  },

  isAdmin()   { return this.hasRole('Admin'); },
  isManager() { return this.hasRole('Manager') || this.hasRole('Admin'); },

  getUserInfo() {
    return {
      id: localStorage.getItem('userId'),
      firstName: localStorage.getItem('firstName'),
      lastName: localStorage.getItem('lastName'),
      email: localStorage.getItem('userEmail'),
    };
  },

  saveSession(data) {
    localStorage.setItem('accessToken', data.accessToken);
    localStorage.setItem('refreshToken', data.refreshToken);
    localStorage.setItem('userId', data.user.id);
    localStorage.setItem('firstName', data.user.firstName);
    localStorage.setItem('lastName', data.user.lastName);
    localStorage.setItem('userEmail', data.user.email);
  },

  logout() {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      API.post('/auth/revoke', { refreshToken }).catch(() => {});
    }
    localStorage.clear();
    window.location.href = '/index.html';
  },
};

// ── Auth page logic ───────────────────────────────────────────────
if (document.getElementById('loginForm')) {
  // Redirect if already logged in
  if (Auth.isLoggedIn()) window.location.href = '/dashboard.html';

  const tabs = document.querySelectorAll('.tab-btn');
  tabs.forEach(btn => btn.addEventListener('click', () => {
    tabs.forEach(t => t.classList.remove('active'));
    btn.classList.add('active');
    document.querySelectorAll('.tab-panel').forEach(p => p.classList.add('hidden'));
    document.getElementById(btn.dataset.tab).classList.remove('hidden');
  }));

  document.getElementById('loginForm').addEventListener('submit', async e => {
    e.preventDefault();
    const btn = e.target.querySelector('button[type=submit]');
    const err = document.getElementById('loginError');
    btn.disabled = true;
    err.classList.add('hidden');
    try {
      const data = await API.post('/auth/login', {
        email: document.getElementById('loginEmail').value,
        password: document.getElementById('loginPassword').value,
      });
      Auth.saveSession(data);
      window.location.href = '/dashboard.html';
    } catch (ex) {
      err.textContent = ex.message;
      err.classList.remove('hidden');
    } finally { btn.disabled = false; }
  });

  document.getElementById('registerForm').addEventListener('submit', async e => {
    e.preventDefault();
    const btn = e.target.querySelector('button[type=submit]');
    const err = document.getElementById('registerError');
    btn.disabled = true;
    err.classList.add('hidden');
    try {
      const data = await API.post('/auth/register', {
        firstName: document.getElementById('regFirstName').value,
        lastName: document.getElementById('regLastName').value,
        email: document.getElementById('regEmail').value,
        password: document.getElementById('regPassword').value,
      });
      Auth.saveSession(data);
      window.location.href = '/dashboard.html';
    } catch (ex) {
      err.textContent = ex.message;
      err.classList.remove('hidden');
    } finally { btn.disabled = false; }
  });
}
