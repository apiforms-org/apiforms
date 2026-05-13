import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiForm, FormAuthSettings, FormPermission } from '../models/apiform.models';
import { ApiFormsService } from '../services/apiforms.service';

@Component({
  selector: 'apiforms-forms-list-page',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <section class="wrap">
      <header>
        <h1>Forms</h1>
        <a routerLink="/apiforms/form-builder" class="btn">Nuevo formulario</a>
      </header>

      <div class="grid" *ngIf="forms.length; else empty">
        <article class="card" *ngFor="let form of forms">
          <h3>{{ form.name }}</h3>
          <p>/api/forms/{{ form.id }}/{{ form.slug }}/data</p>
          <p>Estado: <strong>{{ form.status }}</strong></p>
          <div class="actions">
            <button class="btn" (click)="publish(form.id)" [disabled]="form.status === 'published'">Publicar</button>
            <button class="btn" (click)="publishAndOpen(form.id, form.slug)">Publicar y abrir</button>
            <button class="btn" (click)="openApiModal(form)">Editar API</button>
            <a class="btn" [routerLink]="'/apiforms/policies/' + form.id">Políticas</a>
            <a [routerLink]="'/f/' + form.id + '/' + form.slug">Abrir público</a>
            <a [routerLink]="'/apiforms/form-responses/' + form.id + '/' + form.slug">Respuestas</a>
          </div>
        </article>
      </div>

      <ng-template #empty>
        <p>No hay formularios aún.</p>
      </ng-template>
      <p *ngIf="errorMessage" class="error">{{ errorMessage }}</p>
    </section>

    <section class="modal-backdrop" *ngIf="apiModalOpen" (click)="closeApiModal()">
      <article class="modal" (click)="$event.stopPropagation()" *ngIf="selectedForm">
        <header class="modal-head">
          <h3>Editar API: {{ selectedForm.name }}</h3>
          <button type="button" class="close" (click)="closeApiModal()">x</button>
        </header>

        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>POST /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data</code>
          <span class="badge" [class.off]="!permissions.create">{{ permissions.create ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data/{{ '{' }}id{{ '}' }}</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>PUT /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data/{{ '{' }}id{{ '}' }}</code>
          <span class="badge" [class.off]="!permissions.update">{{ permissions.update ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>DELETE /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data/{{ '{' }}id{{ '}' }}</code>
          <span class="badge" [class.off]="!permissions.delete">{{ permissions.delete ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>POST /api/forms/public/{{ selectedForm.id }}/{{ selectedForm.slug }}/submit</code>
          <span class="badge" [class.off]="!permissions.publicSubmit">{{ permissions.publicSubmit ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data/search?field=nombre&value=diego</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data/search-by-question?question=hola&nombre=diego</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <div class="endpoint">
          <code>GET /api/forms/{{ selectedForm.id }}/{{ selectedForm.slug }}/data/search-by-question?question=hola&filters=nombre:diego,ciudad:cali</code>
          <span class="badge" [class.off]="!permissions.read">{{ permissions.read ? 'enabled' : 'disabled' }}</span>
        </div>
        <p class="hint">
          Referencias: búsqueda simple usa <code>field</code> y <code>value</code>. Búsqueda avanzada acepta varios filtros por <code>&</code> (ej: <code>&nombre=diego&ciudad=cali</code>) o por comas en <code>filters</code> con formato <code>campo:valor</code>.
        </p>

        <div class="checks">
          <label><input type="checkbox" [(ngModel)]="permissions.create" /> create</label>
          <label><input type="checkbox" [(ngModel)]="permissions.read" /> read</label>
          <label><input type="checkbox" [(ngModel)]="permissions.update" /> update</label>
          <label><input type="checkbox" [(ngModel)]="permissions.delete" /> delete</label>
          <label><input type="checkbox" [(ngModel)]="permissions.publicSubmit" /> publicSubmit</label>
        </div>

        <h4>Autenticación de consumo API</h4>
        <div class="checks">
          <label><input type="checkbox" [(ngModel)]="auth.requireJwt" /> Requerir JWT</label>
          <label><input type="checkbox" [(ngModel)]="auth.requireSubscriptionKey" /> Requerir Subscription Key</label>
        </div>
        <p *ngIf="auth.keyPreview">Key activa: <code>{{ auth.keyPreview }}</code></p>
        <p *ngIf="generatedKey"><strong>Nueva Key:</strong> <code>{{ generatedKey }}</code></p>
        <label>Nombre de la key (cliente)</label>
        <input type="text" [(ngModel)]="newKeyName" placeholder="ej: cliente-acme" />

        <div class="modal-actions">
          <button class="btn" (click)="savePermissions()">Guardar</button>
          <button class="btn" (click)="generateSubscriptionKey()">Generar Subscription Key</button>
          <button class="btn ghost" (click)="closeApiModal()">Cerrar</button>
        </div>
        <p *ngIf="modalMessage" class="ok">{{ modalMessage }}</p>
        <p *ngIf="modalError" class="error">{{ modalError }}</p>
      </article>
    </section>
  `,
  styles: [
    `.wrap{max-width:1000px;margin:24px auto;padding:0 16px;color:#dbe7fb}.wrap h1,.wrap h3,.wrap h4{color:#f8fafc}header{display:flex;justify-content:space-between;align-items:center}.grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(260px,1fr));gap:16px}.card{border:1px solid #253754;border-radius:14px;padding:14px;background:linear-gradient(180deg,#0b1528 0%,#0a1322 100%);box-shadow:0 16px 40px rgba(2,8,23,.45)}.card p{color:#9fb3cf}.actions{display:flex;gap:10px;flex-wrap:wrap}.btn{padding:8px 12px;background:#0e7490;color:#fff;border:0;border-radius:8px;cursor:pointer;text-decoration:none}.btn.ghost{background:#334155}.error{color:#fecaca;background:#3b0a0a;padding:10px;border-radius:8px;border:1px solid #7f1d1d;margin-top:10px}.ok{color:#166534;background:#dcfce7;padding:10px;border-radius:8px;border:1px solid #bbf7d0;margin-top:10px}.modal-backdrop{position:fixed;inset:0;background:#020617bf;display:flex;align-items:center;justify-content:center;padding:14px;z-index:50}.modal{width:min(900px,100%);max-height:90vh;overflow:auto;background:linear-gradient(180deg,#0b1528 0%,#0a1322 100%);color:#dbe7fb;border-radius:14px;padding:16px;border:1px solid #253754;display:flex;flex-direction:column;gap:10px;box-shadow:0 20px 60px rgba(0,0,0,.55)}.modal-head{display:flex;justify-content:space-between;align-items:center}.close{border:1px solid #314869;background:#0a1a31;color:#dbeafe;border-radius:8px;padding:6px 10px;cursor:pointer}.endpoint{display:flex;justify-content:space-between;gap:12px;align-items:center;flex-wrap:wrap}.endpoint code{color:#dbeafe;background:#0a1a31;border:1px solid #1f3558;padding:5px 8px;border-radius:8px}.checks{display:grid;grid-template-columns:repeat(auto-fit,minmax(160px,1fr));gap:8px}.badge{font-size:12px;padding:4px 8px;border-radius:999px;background:#dcfce7;color:#166534;border:1px solid #bbf7d0}.badge.off{background:#fee2e2;color:#b91c1c;border-color:#fecaca}.modal-actions{display:flex;gap:10px}.hint{font-size:13px;color:#c7d7ee;background:#0a1a31;border:1px solid #2b4367;border-radius:8px;padding:8px}input[type=text]{padding:8px;border:1px solid #2c3f5f;border-radius:8px;background:#0a1a31;color:#e2e8f0}`
  ]
})
export class FormsListPage implements OnInit {
  forms: ApiForm[] = [];
  errorMessage = '';
  apiModalOpen = false;
  selectedForm?: ApiForm;
  modalMessage = '';
  modalError = '';
  permissions: FormPermission = {
    create: true,
    read: true,
    update: true,
    delete: true,
    publicSubmit: true
  };
  auth: FormAuthSettings = {
    requireJwt: true,
    requireSubscriptionKey: false,
    hasActiveKey: false,
    keyPreview: ''
  };
  generatedKey = '';
  newKeyName = '';

  constructor(private readonly api: ApiFormsService, private readonly router: Router) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.errorMessage = '';
    this.api.listForms().subscribe({
      next: (data) => (this.forms = data),
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar formularios.`;
      }
    });
  }

  publish(id: string): void {
    this.errorMessage = '';
    this.api.publishForm(id).subscribe({
      next: () => this.load(),
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo publicar.`;
      }
    });
  }

  publishAndOpen(id: string, slug: string): void {
    this.errorMessage = '';
    this.api.publishForm(id).subscribe({
      next: () => this.router.navigateByUrl(`/f/${id}/${slug}`),
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo publicar y abrir.`;
      }
    });
  }

  openApiModal(form: ApiForm): void {
    this.apiModalOpen = true;
    this.selectedForm = form;
    this.modalMessage = '';
    this.modalError = '';
    this.api.getPermissions(form.id).subscribe({
      next: (p) => this.permissions = p,
      error: (err: HttpErrorResponse) => {
        this.modalError = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar permisos.`;
      }
    });
    this.api.getFormAuthSettings(form.id).subscribe({
      next: (a) => this.auth = a,
      error: (err: HttpErrorResponse) => {
        this.modalError = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar auth de API.`;
      }
    });
  }

  closeApiModal(): void {
    this.apiModalOpen = false;
    this.selectedForm = undefined;
  }

  savePermissions(): void {
    if (!this.selectedForm) return;
    this.modalMessage = '';
    this.modalError = '';
    this.api.updatePermissions(this.selectedForm.id, this.permissions).subscribe({
      next: () => {
        this.api.updateFormAuthSettings(this.selectedForm!.id, {
          requireJwt: this.auth.requireJwt,
          requireSubscriptionKey: this.auth.requireSubscriptionKey
        }).subscribe({
          next: () => this.modalMessage = 'Permisos y autenticación guardados.',
          error: (err: HttpErrorResponse) => {
            this.modalError = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo guardar auth de API.`;
          }
        });
      },
      error: (err: HttpErrorResponse) => {
        this.modalError = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo guardar permisos.`;
      }
    });
  }

  generateSubscriptionKey(): void {
    if (!this.selectedForm) return;
    this.modalMessage = '';
    this.modalError = '';
    this.generatedKey = '';
    const name = this.newKeyName.trim();
    if (!name) {
      this.modalError = 'Debes indicar un nombre para la key (cliente).';
      return;
    }
    this.api.createSubscriptionKey(this.selectedForm.id, name).subscribe({
      next: (res) => {
        this.generatedKey = res.key;
        this.auth.keyPreview = res.keyPreview;
        this.auth.hasActiveKey = true;
        this.modalMessage = `Subscription key '${res.name}' generada. Cópiala ahora.`;
        this.newKeyName = '';
      },
      error: (err: HttpErrorResponse) => {
        this.modalError = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo generar key.`;
      }
    });
  }
}
