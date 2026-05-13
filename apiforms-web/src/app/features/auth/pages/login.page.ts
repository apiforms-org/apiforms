import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'apiforms-login-page',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <section class="wrap">
      <article class="panel contact-panel">
        <h2>Formulario de Contacto</h2>
        <div class="preview-form">
          <label>Nombre</label>
          <input type="text" placeholder="Escribe tu nombre" />

          <label>Correo electrónico</label>
          <input type="email" placeholder="correo@ejemplo.com" />

          <label>Mensaje</label>
          <textarea rows="4" placeholder="Escribe tu mensaje"></textarea>

          <button type="button" class="btn btn-green">Enviar</button>
        </div>
      </article>
    </section>
  `,
  styles: [`.wrap{max-width:760px;margin:38px auto;padding:0 16px;display:flex;flex-direction:column;gap:18px}.panel{background:linear-gradient(180deg,#0b1528 0%,#0a1322 100%);border:1px solid #253754;border-radius:16px;padding:18px;color:#dbe7fb;box-shadow:0 16px 40px rgba(2,8,23,.45)}.login-panel h1,.contact-panel h2{margin:0 0 12px;color:#f8fafc}.form-grid,.preview-form{display:flex;flex-direction:column;gap:10px}label{font-weight:600;color:#dbe7fb}input,textarea{padding:11px;border-radius:10px;border:1px solid #2b4367;background:#0a1a31;color:#e2e8f0;outline:none}input::placeholder,textarea::placeholder{color:#93a9c7}.actions{display:flex;gap:10px;margin-top:8px}.btn{border-radius:10px;padding:10px 14px;border:1px solid #334155;cursor:pointer}.btn-green{background:linear-gradient(180deg,#2fbe58 0%,#209346 100%);border-color:#2ea153;color:#f0fdf4;font-weight:700}.btn-dark{background:#111827;color:#e2e8f0}.error{color:#fecaca;background:#3b0a0a;padding:10px;border-radius:8px;border:1px solid #7f1d1d;margin-top:8px}`]
})
export class LoginPage {
  email = '';
  password = '';
  errorMessage = '';

  constructor(private readonly auth: AuthService, private readonly router: Router) {}

  login(): void {
    this.errorMessage = '';
    this.auth.login(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl('/apiforms/forms-list'),
      error: (err: HttpErrorResponse) => this.errorMessage = err.error?.message || `Error ${err.status}: no se pudo iniciar sesión.`
    });
  }

  register(): void {
    this.errorMessage = '';
    this.auth.register(this.email, this.password).subscribe({
      next: () => this.router.navigateByUrl('/apiforms/forms-list'),
      error: (err: HttpErrorResponse) => this.errorMessage = err.error?.message || `Error ${err.status}: no se pudo registrar.`
    });
  }
}
