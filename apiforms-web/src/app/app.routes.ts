import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { LoginPage } from './features/auth/pages/login.page';
import { FormPublicPage } from './features/apiforms/pages/form-public.page';
import { APIFORMS_ROUTES } from './features/apiforms/routes/apiforms.routes';

export const routes: Routes = [
  { path: 'login', component: LoginPage },
  { path: 'f/:formId/:slug', component: FormPublicPage },
  { path: 'apiforms', canActivate: [authGuard], children: APIFORMS_ROUTES },
  { path: '', pathMatch: 'full', redirectTo: 'login' }
];
