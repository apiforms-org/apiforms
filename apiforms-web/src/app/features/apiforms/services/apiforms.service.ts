import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { env } from '../../../core/config/env';
import { ApiForm, ApiSubmission, CreateFormRequest, CreateSubscriptionKeyResponse, FormAuthSettings, FormPermission, PublicSubmitRequest } from '../models/apiform.models';

@Injectable({ providedIn: 'root' })
export class ApiFormsService {
  private readonly base = `${env.apiBaseUrl}/api/forms`;

  constructor(private readonly http: HttpClient) {}

  listForms(): Observable<ApiForm[]> {
    return this.http.get<ApiForm[]>(this.base);
  }

  createForm(payload: CreateFormRequest): Observable<ApiForm> {
    return this.http.post<ApiForm>(this.base, payload);
  }

  getFormById(id: string): Observable<ApiForm> {
    return this.http.get<ApiForm>(`${this.base}/${id}`);
  }

  publishForm(id: string): Observable<ApiForm> {
    return this.http.post<ApiForm>(`${this.base}/${id}/publish`, {});
  }

  getPublicForm(formId: string, slug: string): Observable<ApiForm> {
    return this.http.get<ApiForm>(`${this.base}/public/${formId}/${slug}`);
  }

  submitPublic(formId: string, slug: string, payload: PublicSubmitRequest): Observable<ApiSubmission> {
    return this.http.post<ApiSubmission>(`${this.base}/public/${formId}/${slug}/submit`, payload);
  }

  listResponses(formId: string, slug: string): Observable<ApiSubmission[]> {
    return this.http.get<ApiSubmission[]>(`${this.base}/${formId}/${slug}/data`);
  }

  getPermissions(formId: string): Observable<FormPermission> {
    return this.http.get<FormPermission>(`${env.apiBaseUrl}/api/form-permissions/${formId}`);
  }

  updatePermissions(formId: string, permissions: FormPermission): Observable<FormPermission> {
    return this.http.put<FormPermission>(`${env.apiBaseUrl}/api/form-permissions/${formId}`, permissions);
  }

  getFormAuthSettings(formId: string): Observable<FormAuthSettings> {
    return this.http.get<FormAuthSettings>(`${env.apiBaseUrl}/api/form-auth/${formId}`);
  }

  updateFormAuthSettings(formId: string, payload: { requireJwt: boolean; requireSubscriptionKey: boolean }): Observable<FormAuthSettings> {
    return this.http.put<FormAuthSettings>(`${env.apiBaseUrl}/api/form-auth/${formId}`, payload);
  }

  createSubscriptionKey(formId: string, name: string): Observable<CreateSubscriptionKeyResponse> {
    return this.http.post<CreateSubscriptionKeyResponse>(`${env.apiBaseUrl}/api/form-auth/${formId}/keys`, { name });
  }

  getSmartQlPolicy(formId: string, policyId: string): Observable<{ formId: string; policyId: string; event: string; smartQl: string; enabled: boolean; priority: number; updatedAt: string }> {
    return this.http.get<{ formId: string; policyId: string; event: string; smartQl: string; enabled: boolean; priority: number; updatedAt: string }>(
      `${env.apiBaseUrl}/api/smartql-policies/${formId}/${policyId}`
    );
  }

  upsertSmartQlPolicy(formId: string, payload: { policyId: string; event: string; smartQl: string; enabled?: boolean; priority?: number }): Observable<{ formId: string; policyId: string; event: string; smartQl: string; enabled: boolean; priority: number; updatedAt: string }> {
    return this.http.put<{ formId: string; policyId: string; event: string; smartQl: string; enabled: boolean; priority: number; updatedAt: string }>(
      `${env.apiBaseUrl}/api/smartql-policies/${formId}`,
      payload
    );
  }

}
