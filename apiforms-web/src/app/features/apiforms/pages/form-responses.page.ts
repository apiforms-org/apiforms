import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiSubmission } from '../models/apiform.models';
import { ApiFormsService } from '../services/apiforms.service';

@Component({
  selector: 'apiforms-form-responses-page',
  standalone: true,
  imports: [CommonModule],
  template: `
    <section class="wrap">
      <h1>Respuestas: {{ slug }}</h1>
      <div class="card" *ngFor="let item of responses">
        <pre>{{ item.answers | json }}</pre>
      </div>
    </section>
  `,
  styles: [`.wrap{max-width:900px;margin:24px auto;padding:0 16px}.card{background:#fff;border:1px solid #d6dde8;border-radius:12px;padding:12px;margin-bottom:10px}`]
})
export class FormResponsesPage implements OnInit {
  formId = '';
  slug = '';
  responses: ApiSubmission[] = [];

  constructor(private readonly route: ActivatedRoute, private readonly api: ApiFormsService) {}

  ngOnInit(): void {
    this.formId = this.route.snapshot.paramMap.get('formId') ?? '';
    this.slug = this.route.snapshot.paramMap.get('slug') ?? '';
    this.api.listResponses(this.formId, this.slug).subscribe({ next: (res) => (this.responses = res) });
  }
}
