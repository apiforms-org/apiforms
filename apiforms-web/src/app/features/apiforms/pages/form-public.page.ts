import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiForm } from '../models/apiform.models';
import { ApiFormsService } from '../services/apiforms.service';

@Component({
  selector: 'apiforms-form-public-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="wrap" *ngIf="form">
      <h1>{{ form.name }}</h1>
      <form (ngSubmit)="submit()" class="panel">
        <div *ngFor="let field of form.fields" class="item">
          <label>{{ field.label }}</label>
          <ng-container [ngSwitch]="field.type">
            <textarea *ngSwitchCase="'textarea'" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required"></textarea>
            <select *ngSwitchCase="'select'" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required">
              <option value="" disabled selected>Selecciona una opción</option>
              <option *ngFor="let opt of field.options" [value]="opt">{{ opt }}</option>
            </select>
            <input *ngSwitchCase="'email'" type="email" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required" />
            <input *ngSwitchCase="'number'" type="number" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required" />
            <input *ngSwitchCase="'decimal'" type="number" step="0.01" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required" />
            <input *ngSwitchCase="'date'" type="date" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required" />
            <input *ngSwitchCase="'datetime'" type="datetime-local" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required" />
            <label *ngSwitchCase="'boolean'" class="check">
              <input type="checkbox" [(ngModel)]="answers[field.id]" [name]="field.id" />
              <span>{{ field.required ? 'Requerido' : 'Opcional' }}</span>
            </label>
            <input *ngSwitchDefault type="text" [(ngModel)]="answers[field.id]" [name]="field.id" [required]="field.required" />
          </ng-container>
        </div>
        <button type="submit" [disabled]="submitting">{{ submitting ? 'Enviando...' : 'Enviar' }}</button>
      </form>
      <p *ngIf="done" class="ok">{{ submitMessage }}</p>
      <p *ngIf="!done && submitMessage" class="error">{{ submitMessage }}</p>
    </section>
    <section class="wrap" *ngIf="!form && errorMessage">
      <p class="error">{{ errorMessage }}</p>
    </section>
  `,
  styles: [`.wrap{max-width:700px;margin:24px auto;padding:0 16px}.panel{background:#fff;border:1px solid #d6dde8;padding:16px;border-radius:14px;display:flex;flex-direction:column;gap:10px}.item{display:flex;flex-direction:column;gap:6px}.check{display:flex;align-items:center;gap:8px}input,button,select,textarea{padding:9px}.error{color:#b91c1c;background:#fee2e2;padding:10px;border-radius:8px;border:1px solid #fecaca}.ok{color:#166534;background:#dcfce7;padding:10px;border-radius:8px;border:1px solid #bbf7d0}`]
})
export class FormPublicPage implements OnInit {
  form?: ApiForm;
  answers: Record<string, unknown> = {};
  done = false;
  errorMessage = '';
  submitMessage = '';
  submitting = false;
  formId = '';
  slug = '';

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiFormsService) {}

  ngOnInit(): void {
    this.formId = this.route.snapshot.paramMap.get('formId') ?? '';
    this.slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.errorMessage = '';
    this.api.getPublicForm(this.formId, this.slug).subscribe({
      next: (f) => (this.form = f),
      error: (err: HttpErrorResponse) => {
        if (err.status === 404) {
          this.api.getFormById(this.formId).subscribe({
            next: (f) => {
              this.errorMessage = f.status === 'draft'
                ? 'El formulario existe, pero está en borrador. Debes publicarlo para verlo en URL pública.'
                : 'Formulario no existe con ese id/slug.';
            },
            error: () => {
              this.errorMessage = 'Formulario no publicado o no existe.';
            }
          });
          return;
        }

        this.errorMessage = err.error?.detail || err.error?.message || `Error ${err.status}: no se pudo cargar formulario público.`;
      }
    });
  }

  submit(): void {
    if (!this.form) return;
    this.done = false;
    this.submitMessage = '';
    this.submitting = true;
    this.api.submitPublic(this.formId, this.slug, { answers: this.answers }).subscribe({
      next: (res) => {
        this.done = true;
        this.submitMessage = `Envío exitoso. ID de respuesta: ${res.id}`;
        this.answers = {};
        this.submitting = false;
      },
      error: (err: HttpErrorResponse) => {
        const detail = err.error?.detail || err.error?.message;
        this.submitMessage = detail || `Error ${err.status}: no se pudo enviar el formulario.`;
        this.submitting = false;
      }
    });
  }
}
