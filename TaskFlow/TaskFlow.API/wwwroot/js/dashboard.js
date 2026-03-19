/**
 * Dashboard bootstrap — auth guard, navigation, shared utilities.
 */

// ── Shared helpers ────────────────────────────────────────────────
function esc(str) {
  if (!str) return '';
  return String(str)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

function fmtDate(iso) {
  if (!iso) return '';
  return new Date(iso).toLocaleDateString('en-US', { month: 'short', day: 'numeric', year: 'numeric' });
}

function statusBadge(status) {
  const map = {
    'Active': 'badge-green', 'Todo': 'badge-blue', 'InProgress': 'badge-purple',
    'InReview': 'badge-yellow', 'Done': 'badge-green', 'Cancelled': 'badge-gray',
    'OnHold': 'badge-yellow', 'Completed': 'badge-green', 'Archived': 'badge-gray',
  };
  return `<span class="badge ${map[status] || 'badge-gray'}">${status}</span>`;
}

function priorityBadge(priority) {
  const map = { 'Low': 'badge-blue', 'Medium': 'badge-gray', 'High': 'badge-yellow', 'Critical': 'badge-red' };
  return `<span class="badge ${map[priority] || 'badge-gray'}">${priority}</span>`;
}

// ── Dashboard ─────────────────────────────────────────────────────
const Dashboard = {
  init() {
    if (!Auth.isLoggedIn()) { window.location.href = '/index.html'; return; }

    const user = Auth.getUserInfo();
    document.getElementById('sidebarName').textContent = `${user.firstName} ${user.lastName}`;
    document.getElementById('sidebarEmail').textContent = user.email;

    // Show admin nav item only for admins
    document.getElementById('navUsers').classList.toggle('hidden', !Auth.isAdmin());

    // Logout
    document.getElementById('logoutBtn').addEventListener('click', () => Auth.logout());

    // Navigation
    document.querySelectorAll('[data-panel]').forEach(el => {
      el.addEventListener('click', () => this.showPanel(el.dataset.panel));
    });

    // Default panel
    this.showPanel('projects');
  },

  async showPanel(name) {
    document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));

    const panel = document.getElementById(`panel-${name}`);
    if (panel) panel.classList.add('active');

    const navItem = document.querySelector(`[data-panel="${name}"]`);
    if (navItem) navItem.classList.add('active');

    // Update header
    const titles = {
      projects: 'My Projects', projectDetail: 'Project Detail',
      users: 'All Users', myTasks: 'My Tasks',
    };
    document.getElementById('panelTitle').textContent = titles[name] || '';

    // Show/hide add buttons
    document.getElementById('btnNewProject').classList.toggle('hidden', name !== 'projects' || !Auth.isManager());
    document.getElementById('btnNewTask').classList.toggle('hidden', name !== 'projectDetail' || !Auth.isManager());
    document.getElementById('btnBackToProjects').classList.toggle('hidden', name !== 'projectDetail');

    // Load data
    if (name === 'projects')     await Projects.loadList();
    if (name === 'users')        await Users.loadList();
    if (name === 'myTasks')      await this.loadMyTasks();
    if (name === 'projects' || name === 'projectDetail') await Projects.loadUsersForSelect();
  },

  async loadMyTasks() {
    const tbody = document.getElementById('myTasksBody');
    tbody.innerHTML = '<tr><td colspan="5"><div class="loading"><div class="spinner"></div></div></td></tr>';
    try {
      // Get all projects the user is in, then collect tasks assigned to current user
      const projects = await API.get('/projects');
      const userId = Auth.getUserInfo().id;
      let allTasks = [];
      for (const p of (projects || [])) {
        const tasks = await API.get(`/projects/${p.id}/tasks?assigneeId=${userId}`);
        if (tasks) allTasks.push(...tasks.map(t => ({ ...t, projectName: p.name })));
      }
      if (!allTasks.length) {
        tbody.innerHTML = '<tr><td colspan="5" style="text-align:center;padding:40px;color:var(--gray-400)">No tasks assigned to you.</td></tr>';
        return;
      }
      tbody.innerHTML = allTasks.map(t => `
        <tr>
          <td>${esc(t.title)}</td>
          <td>${esc(t.projectName)}</td>
          <td>${statusBadge(t.status)}</td>
          <td>${priorityBadge(t.priority)}</td>
          <td>${t.dueDate ? fmtDate(t.dueDate) : '—'}</td>
        </tr>
      `).join('');
    } catch (ex) {
      tbody.innerHTML = `<tr><td colspan="5"><div class="alert alert-error">${ex.message}</div></td></tr>`;
    }
  },
};

// ── Users panel ───────────────────────────────────────────────────
const Users = {
  async loadList() {
    const tbody = document.getElementById('usersBody');
    tbody.innerHTML = '<tr><td colspan="5"><div class="loading"><div class="spinner"></div></div></td></tr>';
    try {
      const users = await API.get('/users');
      tbody.innerHTML = (users || []).map(u => `
        <tr>
          <td>${esc(u.firstName)} ${esc(u.lastName)}</td>
          <td>${esc(u.email)}</td>
          <td>${u.roles.map(r => `<span class="badge badge-blue">${r}</span>`).join(' ')}</td>
          <td><span class="badge ${u.isActive ? 'badge-green' : 'badge-red'}">${u.isActive ? 'Active' : 'Inactive'}</span></td>
          <td style="display:flex;gap:6px;flex-wrap:wrap">
            ${!u.roles.includes('Manager') && !u.roles.includes('Admin') ? `<button class="btn btn-secondary btn-sm" onclick="Users.promote('${u.id}')">Promote</button>` : ''}
            ${u.roles.includes('Manager') && !u.roles.includes('Admin') ? `<button class="btn btn-secondary btn-sm" onclick="Users.demote('${u.id}')">Demote</button>` : ''}
            ${u.isActive && !u.roles.includes('Admin') ? `<button class="btn btn-danger btn-sm" onclick="Users.deactivate('${u.id}')">Deactivate</button>` : ''}
          </td>
        </tr>
      `).join('');
    } catch (ex) {
      tbody.innerHTML = `<tr><td colspan="5"><div class="alert alert-error">${ex.message}</div></td></tr>`;
    }
  },

  async promote(id) {
    try { await API.post(`/users/${id}/promote`); await this.loadList(); } catch (ex) { alert(ex.message); }
  },
  async demote(id) {
    try { await API.post(`/users/${id}/demote`); await this.loadList(); } catch (ex) { alert(ex.message); }
  },
  async deactivate(id) {
    if (!confirm('Deactivate this user?')) return;
    try { await API.delete(`/users/${id}`); await this.loadList(); } catch (ex) { alert(ex.message); }
  },
};

// Boot when DOM ready
document.addEventListener('DOMContentLoaded', () => Dashboard.init());
