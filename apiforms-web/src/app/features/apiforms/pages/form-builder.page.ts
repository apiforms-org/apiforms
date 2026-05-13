import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CreateFormRequest } from '../models/apiform.models';
import { ApiFormsService } from '../services/apiforms.service';

@Component({
  selector: 'apiforms-form-builder-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="wrap">
      <h1>Form Builder</h1>
      <form (ngSubmit)="save()" class="panel">
        <label>Nombre</label>
        <input [(ngModel)]="name" name="name" required />

        <label>Slug</label>
        <input [(ngModel)]="slug" name="slug" required />

        <h3>Campos</h3>
        <div *ngFor="let f of fields; let i = index" class="row">
          <input [(ngModel)]="f.id" [name]="'id'+i" placeholder="id automático" readonly />
          <input [(ngModel)]="f.label" [name]="'label'+i" placeholder="label" required (ngModelChange)="onLabelChange(i)" />
          <select [(ngModel)]="f.type" [name]="'type'+i" (ngModelChange)="onTypeChange(i)">
            <option>text</option>
            <option>textarea</option>
            <option>email</option>
            <option>number</option>
            <option>decimal</option>
            <option>date</option>
            <option>datetime</option>
            <option>boolean</option>
            <option>select</option>
          </select>
          <label><input type="checkbox" [(ngModel)]="f.required" [name]="'req'+i" /> req</label>
          <button type="button" (click)="removeField(i)">x</button>
          <input *ngIf="f.type === 'select'" [(ngModel)]="f.optionsText" [name]="'options'+i" placeholder="opciones separadas por coma" class="full" />
        </div>

        <button type="button" (click)="addField()">Agregar campo</button>
        <button type="submit">Guardar formulario</button>
        <p *ngIf="errorMessage" class="error">{{ errorMessage }}</p>
      </form>
    </section>
  `,
  styles: [`.wrap{max-width:920px;margin:24px auto;padding:0 16px;color:#dbe7fb}.wrap h1{color:#f8fafc}.panel{display:flex;flex-direction:column;gap:10px;background:linear-gradient(180deg,#0b1528 0%,#0a1322 100%);padding:16px;border:1px solid #253754;border-radius:14px;box-shadow:0 16px 40px rgba(2,8,23,.45)}.panel label,.panel h3{color:#dbe7fb}.row{display:grid;grid-template-columns:1fr 1fr 140px 100px 40px;gap:8px;align-items:center}.full{grid-column:1/-1}@media(max-width:800px){.row{grid-template-columns:1fr 1fr}}input,select,button{padding:8px;border:1px solid #2b4367;border-radius:8px;background:#0a1a31;color:#e2e8f0}input::placeholder{color:#93a9c7}button{background:#0f213f;cursor:pointer}button[type='submit']{background:#0e7490;color:#fff;border-color:#0e7490}button[type='button']{background:#111f3b}.error{color:#fecaca;background:#3b0a0a;padding:10px;border-radius:8px;border:1px solid #7f1d1d}`]
})
export class FormBuilderPage {
  name = '';
  slug = '';
  fields = [this.newField()];
  errorMessage = '';

  constructor(private readonly api: ApiFormsService, private readonly router: Router) {}

  addField(): void { this.fields.push(this.newField()); }
  removeField(i: number): void { this.fields.splice(i, 1); }
  onLabelChange(i: number): void {
    const label = this.fields[i]?.label ?? '';
    this.fields[i].id = this.slugify(label);
  }

  onTypeChange(i: number): void {
    const field = this.fields[i];
    if (field.type !== 'select') {
      field.optionsText = '';
      field.options = [];
    }
  }

  save(): void {
    this.errorMessage = '';
    const normalizedFields = this.fields.map((f) => ({
      ...f,
      options: f.type === 'select'
        ? (f.optionsText ?? '')
            .split(',')
            .map((x: string) => x.trim())
            .filter((x: string) => x.length > 0)
        : []
    }));
    const payload: CreateFormRequest = { name: this.name, slug: this.slug, fields: normalizedFields };
    this.api.createForm(payload).subscribe({
      next: () => this.router.navigateByUrl('/apiforms/forms-list'),
      error: (err: HttpErrorResponse) => {
        if (err.status === 0) {
          this.errorMessage = 'No se pudo conectar con la API (CORS, URL incorrecta o API apagada).';
          return;
        }

        const apiMessage = err.error?.detail || err.error?.message;
        this.errorMessage = apiMessage || `Error ${err.status}: no se pudo guardar el formulario.`;
      }
    });
  }

  private newField() {
    return {
      id: '', type: 'text', label: '', placeholder: '', required: false,
      defaultValue: null, regex: '', min: undefined, max: undefined,
      readonly: false, hidden: false, options: [] as string[], optionsText: ''
    };
  }

  private slugify(value: string): string {
    return value
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, '_')
      .replace(/^_+|_+$/g, '');
  }
}
