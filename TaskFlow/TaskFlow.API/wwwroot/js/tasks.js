/**
 * Tasks panel — list, create, detail modal, status patch, comments.
 */
const Tasks = {
  projectId: null,
  current: null,

  async loadList() {
    const tbody = document.getElementById('taskTableBody');
    tbody.innerHTML = '<tr><td colspan="6" class="loading"><div class="spinner"></div></td></tr>';
    try {
      const tasks = await API.get(`/projects/${this.projectId}/tasks`);
      if (!tasks.length) {
        tbody.innerHTML = '<tr><td colspan="6" class="empty"><div class="empty-icon">✅</div><p>No tasks yet.</p></td></tr>';
        return;
      }
      tbody.innerHTML = tasks.map(t => `
        <tr class="clickable" onclick="Tasks.openDetail(${t.id})">
          <td>${esc(t.title)}</td>
          <td>${statusBadge(t.status)}</td>
          <td>${priorityBadge(t.priority)}</td>
          <td>${esc(t.assigneeName) || '<span style="color:var(--gray-400)">Unassigned</span>'}</td>
          <td>${t.dueDate ? fmtDate(t.dueDate) : '—'}</td>
          <td>
            ${Auth.isManager() ? `<button class="btn btn-danger btn-sm" onclick="event.stopPropagation();Tasks.deleteTask(${t.id})">Delete</button>` : ''}
          </td>
        </tr>
      `).join('');
    } catch (ex) {
      tbody.innerHTML = `<tr><td colspan="6"><div class="alert alert-error">${ex.message}</div></td></tr>`;
    }
  },

  async openDetail(taskId) {
    try {
      const task = await API.get(`/projects/${this.projectId}/tasks/${taskId}`);
      this.current = task;
      this.renderModal(task);
      document.getElementById('taskModal').classList.remove('hidden');
    } catch (ex) { alert(ex.message); }
  },

  renderModal(task) {
    document.getElementById('taskModalTitle').textContent = task.title;
    document.getElementById('taskDesc').textContent = task.description || '—';
    document.getElementById('taskCreatedBy').textContent = task.createdByName;
    document.getElementById('taskCreatedAt').textContent = fmtDate(task.createdAt);
    document.getElementById('taskDue').textContent = task.dueDate ? fmtDate(task.dueDate) : '—';
    document.getElementById('taskAssignee').textContent = task.assigneeName || 'Unassigned';
    document.getElementById('taskPriority').innerHTML = priorityBadge(task.priority);

    // Status select — all members can update
    const statusSelect = document.getElementById('taskStatusSelect');
    statusSelect.value = this.statusToValue(task.status);

    this.loadComments(task.comments || []);
  },

  statusToValue(status) {
    const map = { 'Todo': '0', 'InProgress': '1', 'InReview': '2', 'Done': '3', 'Cancelled': '4' };
    return map[status] || '0';
  },

  async patchStatus() {
    const status = parseInt(document.getElementById('taskStatusSelect').value);
    try {
      await API.patch(`/projects/${this.projectId}/tasks/${this.current.id}/status`, { status });
      await this.loadList();
      document.getElementById('taskStatusFeedback').textContent = 'Status updated!';
      setTimeout(() => { document.getElementById('taskStatusFeedback').textContent = ''; }, 2000);
    } catch (ex) { alert(ex.message); }
  },

  async deleteTask(taskId) {
    if (!confirm('Delete this task?')) return;
    try {
      await API.delete(`/projects/${this.projectId}/tasks/${taskId}`);
      await this.loadList();
    } catch (ex) { alert(ex.message); }
  },

  closeModal() {
    document.getElementById('taskModal').classList.add('hidden');
    this.current = null;
  },

  openCreateModal() {
    document.getElementById('taskForm').reset();
    document.getElementById('taskCreateModal').classList.remove('hidden');
  },

  closeCreateModal() {
    document.getElementById('taskCreateModal').classList.add('hidden');
  },

  async saveTask() {
    const title = document.getElementById('tTitle').value.trim();
    const description = document.getElementById('tDesc').value.trim();
    const priority = parseInt(document.getElementById('tPriority').value);
    const dueDate = document.getElementById('tDue').value || null;
    if (!title) return;

    try {
      await API.post(`/projects/${this.projectId}/tasks`, { title, description, priority, dueDate });
      this.closeCreateModal();
      await this.loadList();
    } catch (ex) { alert(ex.message); }
  },

  loadComments(comments) {
    const list = document.getElementById('commentList');
    list.innerHTML = comments.length ? comments.map(c => `
      <div class="comment-item">
        <span class="comment-author">${esc(c.authorName)}</span>
        <span class="comment-date">${fmtDate(c.createdAt)}</span>
        <div class="comment-content">${esc(c.content)}</div>
      </div>
    `).join('') : '<p style="color:var(--gray-400);font-size:13px">No comments yet.</p>';
  },

  async addComment() {
    const input = document.getElementById('commentInput');
    const content = input.value.trim();
    if (!content || !this.current) return;
    try {
      await API.post(`/tasks/${this.current.id}/comments`, { content });
      input.value = '';
      const task = await API.get(`/projects/${this.projectId}/tasks/${this.current.id}`);
      this.loadComments(task.comments || []);
    } catch (ex) { alert(ex.message); }
  },
};
