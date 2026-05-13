import { Routes } from '@angular/router';
import { ApiSettingsPage } from '../pages/api-settings.page';
import { FormBuilderPage } from '../pages/form-builder.page';
import { FormResponsesPage } from '../pages/form-responses.page';
import { FlowPoliciesPage } from '../pages/flow-policies.page';
import { FormsListPage } from '../pages/forms-list.page';

export const APIFORMS_ROUTES: Routes = [
  { path: 'forms-list', component: FormsListPage },
  { path: 'form-builder', component: FormBuilderPage },
  { path: 'form-responses/:formId/:slug', component: FormResponsesPage },
  { path: 'policies/:formId', component: FlowPoliciesPage },
  { path: 'api-settings', component: ApiSettingsPage },
  { path: '', pathMatch: 'full', redirectTo: 'forms-list' }
];
