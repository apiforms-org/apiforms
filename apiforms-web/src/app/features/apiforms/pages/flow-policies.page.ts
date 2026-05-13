import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiFormsService } from '../services/apiforms.service';

@Component({
  selector: 'apiforms-flow-policies-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="wrap">
      <h1>Políticas</h1>
      <p>Formulario ID: <strong>{{ formId }}</strong></p>
      <p *ngIf="formName">Formulario: <strong>{{ formName }}</strong></p>

      <div class="panel">
        <label>Policy ID (SmartQL)</label>
        <input [(ngModel)]="policyId" placeholder="smartql_form_submit_default" />

        <label>Evento</label>
        <select [(ngModel)]="eventName">
          <option value="ON form.submit">ON form.submit</option>
          <option value="ON api.create">ON api.create</option>
          <option value="ON api.update">ON api.update</option>
          <option value="ON api.delete">ON api.delete</option>
        </select>

        <label>SmartQL Script</label>
        <textarea [(ngModel)]="script" rows="14"></textarea>

        <div class="actions">
          <button type="button" (click)="probeConnection()">Probar conexión SmartQL</button>
          <button type="button" (click)="loadTemplate()">Plantilla MVP</button>
          <button type="button" (click)="save()">Guardar</button>
        </div>

        <p *ngIf="message" class="ok">{{ message }}</p>
        <p *ngIf="probeMessage" class="ok">{{ probeMessage }}</p>
        <p *ngIf="errorMessage" class="error">{{ errorMessage }}</p>
      </div>
    </section>
  `,
  styles: [`.wrap{max-width:900px;margin:24px auto;padding:0 16px}.panel{background:#fff;border:1px solid #d6dde8;border-radius:12px;padding:14px;display:flex;flex-direction:column;gap:10px}input,select,textarea,button{padding:9px}.actions{display:flex;gap:10px}.ok{color:#166534;background:#dcfce7;padding:10px;border-radius:8px;border:1px solid #bbf7d0}.error{color:#b91c1c;background:#fee2e2;padding:10px;border-radius:8px;border:1px solid #fecaca}`]
})
export class FlowPoliciesPage implements OnInit {
  formId = '';
  formName = '';
  policyId = '';
  eventName = 'ON form.submit';
  script = '';
  message = '';
  errorMessage = '';
  probeMessage = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiFormsService) {}

  ngOnInit(): void {
    this.formId = this.route.snapshot.paramMap.get('formId') ?? '';
    this.policyId = `smartql_${this.formId}_default`;
    this.loadTemplate();
    this.loadPolicy();
    this.loadForm();
  }

  loadTemplate(): void {
    this.script = `${this.eventName}
REQUIRE input.nombre
SET input.nombre = UPPER(input.nombre)
IF input.email NOT_MATCH /.+@.+\\..+/ THEN REJECT "Correo inválido"
RETURN input`;
  }

  save(): void {
    this.message = '';
    this.errorMessage = '';
    this.probeMessage = '';

    if (!this.policyId.startsWith('smartql_')) {
      this.errorMessage = 'El policyId debe iniciar con "smartql_".';
      return;
    }
    if (!this.script.trim().startsWith('ON ')) {
      this.errorMessage = 'El script SmartQL debe iniciar con un evento ON.';
      return;
    }

    const payload = {
      policyId: this.policyId.trim(),
      event: this.eventName,
      smartQl: this.script.trim(),
      enabled: true,
      priority: 100
    };

    this.api.upsertSmartQlPolicy(this.formId, payload).subscribe({
      next: () => this.message = 'Política SmartQL guardada en backend.',
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo guardar política.`;
      }
    });
  }

  private loadPolicy(): void {
    this.api.getSmartQlPolicy(this.formId, this.policyId).subscribe({
      next: (p) => {
        this.eventName = p.event;
        this.script = p.smartQl;
      },
      error: (err: HttpErrorResponse) => {
        if (err.status !== 404) {
          this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar política.`;
        }
      }
    });
  }

  probeConnection(): void {
    this.message = '';
    this.errorMessage = '';
    this.probeMessage = '';
    // Real connectivity probe against the same PUT route used by "Guardar".
    // We intentionally send an invalid policyId so backend should answer 400 if route exists.
    const payload = {
      policyId: '__smartql_probe_invalid__',
      event: 'ON form.submit',
      smartQl: 'ON form.submit\nRETURN input',
      enabled: true,
      priority: 100
    };
    this.api.upsertSmartQlPolicy(this.formId, payload).subscribe({
      next: () => {
        this.probeMessage = 'Conexión SmartQL OK (endpoint de guardado disponible).';
      },
      error: (err: HttpErrorResponse) => {
        if (err.status === 400) {
          this.probeMessage = 'Conexión SmartQL OK (endpoint de guardado disponible).';
          return;
        }
        if (err.status === 401) {
          this.errorMessage = 'SmartQL disponible, pero tu sesión/token no es válido (401).';
          return;
        }
        if (err.status === 404) {
          this.errorMessage = 'La ruta de guardado SmartQL no existe en el backend actual (404). Debes reiniciar con la versión nueva.';
          return;
        }
        if (err.status === 0) {
          this.errorMessage = 'No hay conexión al backend o CORS bloqueó la llamada.';
          return;
        }
        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo validar conexión SmartQL.`;
      }
    });
  }

  private loadForm(): void {
    this.api.getFormById(this.formId).subscribe({
      next: (f) => (this.formName = f.name),
      error: (err: HttpErrorResponse) => {
        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar formulario.`;
      }
    });
  }
}
