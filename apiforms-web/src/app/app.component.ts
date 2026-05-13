import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { NavigationEnd } from '@angular/router';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { filter } from 'rxjs';
import { AuthService } from './features/auth/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterOutlet, RouterLink],
  template: `
    <header class="topbar appbar" *ngIf="isAppArea">
      <a routerLink="/apiforms/forms-list">Forms</a>
      <a routerLink="/apiforms/form-builder">Builder</a>
      <a routerLink="/apiforms/api-settings">API Settings</a>
      <a routerLink="/apiforms/forms-list" class="btn btn-green">Mis proyectos</a>
      <span class="user-avatar" title="Usuario logeado">{{ userInitial }}</span>
      <button type="button" (click)="logout()">Salir</button>
    </header>

    <ng-container *ngIf="!isAppArea">
    <div class="network-bg" aria-hidden="true"><div class="nodes"></div><div class="links"></div></div>

    <header class="topbar">
      <div class="brand" routerLink="/apiforms/forms-list">APIFORMS</div>
      <nav class="menu">
        <a href="#" (click)="openSection($event, 'caracteristicas')">Características</a>
        <a href="#" (click)="openSection($event, 'documentacion')">Documentación</a>
        <a href="#" (click)="openSection($event, 'plantillas')">Plantillas</a>
        <a href="#" (click)="openSection($event, 'blog')">Blog</a>
      </nav>
      <div class="actions">
        <a routerLink="/login" class="link">Iniciar sesión</a>
        <a routerLink="/apiforms/forms-list" class="btn btn-green">Ir al panel</a>
        <span class="user-avatar" title="Usuario logeado">{{ userInitial }}</span>
        <button type="button" (click)="logout()">Salir</button>
      </div>
    </header>

    <section class="hero" [class.shifted]="!!activeSection">
      <div class="left">
        <span class="chip">FORMULARIOS -> APIS -> FLUJOS</span>
        <h1>Crea formularios.<br />Genera APIs.<br /><span>Construye flujos.</span></h1>
        <p>
          APIFORMS te permite crear formularios inteligentes, generar APIs CRUD automáticamente y
          construir flujos de transformación sin código o con SmartQL.
        </p>
        <div class="cta-row">
          <a routerLink="/apiforms/forms-list" class="btn btn-green">Ir al panel</a>
          <a routerLink="/apiforms/forms-list" class="btn btn-dark">Ver demo</a>
        </div>
      </div>
      <div class="right">
        <div class="panel large">
          <h3>Inicio de sesión</h3>
          <label>Email</label>
          <input class="line-input" type="email" [(ngModel)]="email" name="hero_email" placeholder="correo@ejemplo.com" />
          <label>Password</label>
          <input class="line-input" type="password" [(ngModel)]="password" name="hero_password" placeholder="********" />
          <div class="hero-login-actions">
            <button type="button" class="btn btn-green" (click)="loginFromHero()">Entrar</button>
            <button type="button" class="btn btn-dark" (click)="registerFromHero()">Registrarse</button>
          </div>
          <p *ngIf="heroAuthError" class="hero-auth-error">{{ heroAuthError }}</p>
        </div>
        <div class="cards">
          <div class="panel small">
            <h4>API CRUD</h4>
            <p>GET /data</p>
            <p>POST /data</p>
            <p>PUT /data/:id</p>
          </div>
          <div class="panel small">
            <h4>Flujos</h4>
            <p>SmartQL</p>
            <p>Validación + Transformación</p>
          </div>
          <div class="panel small">
            <h4>Integraciones</h4>
            <p>HTTP / DB / SFTP</p>
          </div>
        </div>
      </div>
    </section>

    <aside class="info-drawer" [class.open]="!!activeSection" *ngIf="!isAppArea">
      <div class="drawer-head">
        <h3>{{ sectionTitle }}</h3>
        <button type="button" class="close-drawer" (click)="closeSection()">x</button>
      </div>
      <div class="drawer-body">
        <p [innerHTML]="sectionContent"></p>
      </div>
    </aside>
    </ng-container>

    <main><router-outlet /></main>
  `,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  isAppArea = false;
  email = '';
  password = '';
  heroAuthError = '';
  activeSection: 'caracteristicas' | 'documentacion' | 'plantillas' | 'blog' | null = null;

  constructor(private readonly auth: AuthService, private readonly router: Router) {
    this.updateLayout(this.router.url);
    this.router.events
      .pipe(filter((e): e is NavigationEnd => e instanceof NavigationEnd))
      .subscribe((e) => this.updateLayout(e.urlAfterRedirects));
  }

  get userInitial(): string {
    const email = this.auth.getCurrentUserEmail().trim();
    if (!email) return '?';
    return email[0].toUpperCase();
  }

  logout(): void {
    this.auth.logout();
    this.router.navigateByUrl('/login');
  }

  loginFromHero(): void {
    this.heroAuthError = '';
    if (!this.email.trim() || !this.password.trim()) {
      this.heroAuthError = 'Debes completar email y password.';
      return;
    }
    this.auth.login(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigateByUrl('/apiforms/forms-list'),
      error: (err: HttpErrorResponse) => {
        this.heroAuthError = err.error?.message || `Error ${err.status}: no se pudo iniciar sesión.`;
      }
    });
  }

  registerFromHero(): void {
    this.heroAuthError = '';
    if (!this.email.trim() || !this.password.trim()) {
      this.heroAuthError = 'Debes completar email y password.';
      return;
    }
    this.auth.register(this.email.trim(), this.password).subscribe({
      next: () => this.router.navigateByUrl('/apiforms/forms-list'),
      error: (err: HttpErrorResponse) => {
        this.heroAuthError = err.error?.message || `Error ${err.status}: no se pudo registrar.`;
      }
    });
  }

  private updateLayout(url: string): void {
    this.isAppArea = url.startsWith('/apiforms');
  }

  openSection(event: Event, section: 'caracteristicas' | 'documentacion' | 'plantillas' | 'blog'): void {
    event.preventDefault();
    this.activeSection = section;
  }

  closeSection(): void {
    this.activeSection = null;
  }

  get sectionTitle(): string {
    switch (this.activeSection) {
      case 'caracteristicas': return 'Características';
      case 'documentacion': return 'Documentación';
      case 'plantillas': return 'Plantillas';
      case 'blog': return 'Blog';
      default: return '';
    }
  }

  get sectionContent(): string {
    switch (this.activeSection) {
      case 'caracteristicas':
        return 'Crea formularios visuales, publica APIs CRUD automáticas y diseña reglas de negocio con SmartQL. Incluye JWT, API Key, permisos, validaciones y búsqueda avanzada.';
      case 'documentacion':
        return 'Guías de inicio rápido, referencia de endpoints, ejemplos de payloads, políticas SmartQL, integración con Postman y buenas prácticas de seguridad para producción.';
      case 'plantillas':
        return 'Plantillas listas para contacto, soporte, onboarding, encuestas, órdenes y formularios internos. Puedes clonar, editar campos y publicar en minutos.';
      case 'blog':
        return 'Novedades del producto, tutoriales de automatización, casos de uso reales y patrones de arquitectura para escalar formularios, APIs e integraciones.';
      default:
        return '';
    }
  }
}
