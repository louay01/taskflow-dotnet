/**
 * Projects panel — list, create, detail, member management.
 */
const Projects = {
  current: null,
  allUsers: [],

  async loadList() {
    const grid = document.getElementById('projectGrid');
    grid.innerHTML = '<div class="loading"><div class="spinner"></div></div>';
    try {
      const projects = await API.get('/projects');
      if (!projects.length) {
        grid.innerHTML = `<div class="empty"><div class="empty-icon">📋</div><p>No projects yet.</p></div>`;
        return;
      }
      grid.innerHTML = projects.map(p => `
        <div class="card project-card" onclick="Projects.openDetail(${p.id})">
          <div class="project-name">${esc(p.name)}</div>
          <div class="project-desc">${esc(p.description) || '<em>No description</em>'}</div>
          <div class="project-meta">
            ${statusBadge(p.status)}
            <span class="badge badge-gray">${p.members?.length || 0} members</span>
            ${p.dueDate ? `<span style="font-size:11px;color:var(--gray-400)">Due ${fmtDate(p.dueDate)}</span>` : ''}
          </div>
        </div>
      `).join('');
    } catch (ex) { grid.innerHTML = `<div class="alert alert-error">${ex.message}</div>`; }
  },

  async openDetail(id) {
    try {
      const p = await API.get(`/projects/${id}`);
      this.current = p;
      document.getElementById('detailName').textContent = p.name;
      document.getElementById('detailDesc').textContent = p.description || '—';
      document.getElementById('detailStatus').innerHTML = statusBadge(p.status);
      document.getElementById('detailDue').textContent = p.dueDate ? fmtDate(p.dueDate) : '—';
      document.getElementById('detailOwner').textContent = p.ownerName;

      const canManage = Auth.isAdmin() || p.ownerId === Auth.getUserInfo().id;
      document.getElementById('editProjectBtn').classList.toggle('hidden', !canManage);
      document.getElementById('addMemberSection').classList.toggle('hidden', !canManage);

      this.renderMembers(p.members, canManage);
      Dashboard.showPanel('projectDetail');

      // Load tasks for this project
      Tasks.projectId = id;
      await Tasks.loadList();
    } catch (ex) { alert(ex.message); }
  },

  renderMembers(members, canManage) {
    const list = document.getElementById('memberList');
    const userId = Auth.getUserInfo().id;
    list.innerHTML = (members || []).map(m => `
      <div class="member-item">
        <div>
          <div class="member-name">${esc(m.fullName)}</div>
          <div class="member-email">${esc(m.email)}</div>
        </div>
        ${canManage && m.userId !== this.current?.ownerId ? `
          <button class="btn btn-danger btn-sm" onclick="Projects.removeMember('${m.userId}')">Remove</button>
        ` : ''}
      </div>
    `).join('') || '<p style="color:var(--gray-400);font-size:13px">No members.</p>';
  },

  async addMember() {
    const select = document.getElementById('addMemberSelect');
    const userId = select.value;
    if (!userId) return;
    try {
      await API.post(`/projects/${this.current.id}/members`, { userId });
      await this.openDetail(this.current.id);
    } catch (ex) { alert(ex.message); }
  },

  async removeMember(userId) {
    if (!confirm('Remove this member?')) return;
    try {
      await API.delete(`/projects/${this.current.id}/members/${userId}`);
      await this.openDetail(this.current.id);
    } catch (ex) { alert(ex.message); }
  },

  async loadUsersForSelect() {
    if (!Auth.isAdmin()) return;
    try {
      this.allUsers = await API.get('/users') || [];
      const select = document.getElementById('addMemberSelect');
      if (select) {
        select.innerHTML = '<option value="">Select user...</option>' +
          this.allUsers.map(u => `<option value="${u.id}">${esc(u.firstName)} ${esc(u.lastName)} (${esc(u.email)})</option>`).join('');
      }
    } catch {}
  },

  openCreateModal() {
    document.getElementById('projectFormTitle').textContent = 'New Project';
    document.getElementById('projectForm').reset();
    document.getElementById('projectModal').classList.remove('hidden');
  },

  closeModal() {
    document.getElementById('projectModal').classList.add('hidden');
  },

  async saveProject() {
    const name = document.getElementById('pName').value.trim();
    const description = document.getElementById('pDesc').value.trim();
    const dueDate = document.getElementById('pDue').value || null;
    if (!name) return;

    try {
      await API.post('/projects', { name, description, dueDate });
      this.closeModal();
      await this.loadList();
    } catch (ex) { alert(ex.message); }
  },
};
