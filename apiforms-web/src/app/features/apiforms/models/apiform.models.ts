export type FormStatus = 'draft' | 'published';

export interface ApiFormField {
  id: string;
  type: string;
  label: string;
  placeholder?: string;
  required: boolean;
  defaultValue?: unknown;
  regex?: string;
  min?: number;
  max?: number;
  readonly: boolean;
  hidden: boolean;
  options: string[];
  optionsText?: string;
}

export interface ApiFormSettings {
  apiKeyRequired: boolean;
  jwtRequired: boolean;
  publicRead: boolean;
  publicWrite: boolean;
  rateLimit: number;
  cors: boolean;
  allowedOrigins: string[];
}

export interface ApiForm {
  id: string;
  tenantId: string;
  name: string;
  slug: string;
  status: FormStatus;
  fields: ApiFormField[];
  settings: ApiFormSettings;
  version: number;
  createdAt: string;
  updatedAt: string;
}

export interface CreateFormRequest {
  name: string;
  slug: string;
  fields: ApiFormField[];
}

export interface PublicSubmitRequest {
  answers: Record<string, unknown>;
}

export interface ApiSubmission {
  id: string;
  tenantId: string;
  formId: string;
  answers: Record<string, unknown>;
  metadata: Record<string, unknown>;
  submittedAt: string;
}

export interface FormPermission {
  create: boolean;
  read: boolean;
  update: boolean;
  delete: boolean;
  publicSubmit: boolean;
}

export interface FormAuthSettings {
  requireJwt: boolean;
  requireSubscriptionKey: boolean;
  hasActiveKey: boolean;
  keyPreview?: string;
}

export interface CreateSubscriptionKeyResponse {
  id: string;
  name: string;
  key: string;
  keyPreview: string;
}
