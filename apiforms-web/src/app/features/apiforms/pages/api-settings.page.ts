import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ApiForm, FormPermission } from '../models/apiform.models';
import { ApiFormsService } from '../services/apiforms.service';

@Component({
  selector: 'apiforms-api-settings-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="wrap">
      <h1>API Settings</h1>

      <label>Formulario</label>
      <select [(ngModel)]="selectedFormId" (ngModelChange)="onFormChange()">
        <option value="" disabled>Selecciona un formulario</option>
        <option *ngFor="let f of forms" [value]="f.id">{{ f.name }} ({{ f.slug }})</option>
      </select>

      <section *ngIf="selectedForm" class="panel">
        <h3>CRUD generado</h3>
        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.slug }}/data</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>POST /api/forms/{{ selectedForm.slug }}/data</code>
          <span class="badge" [class.off]="!permissions.create">{{ permissions.create ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.slug }}/data/{{ '{' }}id{{ '}' }}</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>PUT /api/forms/{{ selectedForm.slug }}/data/{{ '{' }}id{{ '}' }}</code>
          <span class="badge" [class.off]="!permissions.update">{{ permissions.update ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>DELETE /api/forms/{{ selectedForm.slug }}/data/{{ '{' }}id{{ '}' }}</code>
          <span class="badge" [class.off]="!permissions.delete">{{ permissions.delete ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>POST /api/forms/public/{{ selectedForm.id }}/{{ selectedForm.slug }}/submit</code>
          <span class="badge" [class.off]="!permissions.publicSubmit">{{ permissions.publicSubmit ? 'enabled' : 'disabled' }}</span>
        </div>

        <h3>Permisos</h3>
        <label><input type="checkbox" [(ngModel)]="permissions.create" /> create</label>
        <label><input type="checkbox" [(ngModel)]="permissions.read" /> read</label>
        <label><input type="checkbox" [(ngModel)]="permissions.update" /> update</label>
        <label><input type="checkbox" [(ngModel)]="permissions.delete" /> delete</label>
        <label><input type="checkbox" [(ngModel)]="permissions.publicSubmit" /> publicSubmit</label>

        <button type="button" (click)="save()">Guardar</button>
      </section>

      <p *ngIf="message" class="ok">{{ message }}</p>
      <p *ngIf="errorMessage" class="error">{{ errorMessage }}</p>
    </section>
  `,
  styles: [`.wrap{max-width:900px;margin:24px auto;padding:0 16px;display:flex;flex-direction:column;gap:10px;color:#dbe7fb}.wrap h1,.wrap h3{color:#f8fafc}.panel{background:linear-gradient(180deg,#0b1528 0%,#0a1322 100%);border:1px solid #253754;border-radius:14px;padding:16px;display:flex;flex-direction:column;gap:10px;box-shadow:0 16px 40px rgba(2,8,23,.45)}.endpoint{display:flex;justify-content:space-between;align-items:center;gap:12px;flex-wrap:wrap}.endpoint code{color:#dbeafe;background:#0a1a31;border:1px solid #1f3558;padding:5px 8px;border-radius:8px}label{color:#dbe7fb}select,button{padding:9px;max-width:420px;border-radius:8px;border:1px solid #2c3f5f}select{background:#0a1a31;color:#e2e8f0}.badge{font-size:12px;padding:4px 8px;border-radius:999px;background:#dcfce7;color:#166534;border:1px solid #bbf7d0}.badge.off{background:#fee2e2;color:#b91c1c;border-color:#fecaca}button{background:#22c55e;color:#052e16;font-weight:700}.ok{color:#166534;background:#dcfce7;padding:10px;border-radius:8px;border:1px solid #bbf7d0}.error{color:#fecaca;background:#3b0a0a;padding:10px;border-radius:8px;border:1px solid #7f1d1d}`]
})
export class ApiSettingsPage implements OnInit {
  forms: ApiForm[] = [];
  selectedFormId = '';
  selectedForm?: ApiForm;
  message = '';
  errorMessage = '';
  permissions: FormPermission = {
    create: true,
    read: true,
    update: true,
    delete: true,
    publicSubmit: true
  };

  constructor(private readonly api: ApiFormsService) {}

  ngOnInit(): void {
    this.api.listForms().subscribe({
      next: (forms) => {
        this.forms = forms;
        if (forms.length > 0) {
          this.selectedFormId = forms[0].id;
          this.onFormChange();
        }
      },
      error: (err: HttpErrorResponse) => this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar formularios.`
    });
  }

  onFormChange(): void {
    this.message = '';
    this.errorMessage = '';
    this.selectedForm = this.forms.find((f) => f.id === this.selectedFormId);
    if (!this.selectedFormId) return;

    this.api.getPermissions(this.selectedFormId).subscribe({
      next: (p) => this.permissions = p,
      error: (err: HttpErrorResponse) => this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar permisos.`
    });
  }

  save(): void {
    this.message = '';
    this.errorMessage = '';
    if (!this.selectedFormId) return;

    this.api.updatePermissions(this.selectedFormId, this.permissions).subscribe({
      next: () => this.message = 'Permisos guardados.',
      error: (err: HttpErrorResponse) => this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo guardar permisos.`
    });
  }
}
