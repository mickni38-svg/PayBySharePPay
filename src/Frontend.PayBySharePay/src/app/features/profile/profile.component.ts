import { AfterViewInit, Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { AuthService, LoginResponse, ParticipantType } from '../../core/services/auth.service';
import { ProfileService, UpdateProfileRequest, VippsTestPersonDto } from '../../core/services/profile.service';
import { ParticipantApiDto } from '../../core/models/participant.model';
import { ThemeService } from '../../core/services/theme.service';
import { DirectoryService } from '../../core/services/directory.service';
import { DevService } from '../../core/services/dev.service';
import { DirectoryEntry } from '../../core/models/directory.model';
import { environment } from '../../../environments/environment';

declare const google: {
  accounts: {
    id: {
      initialize: (config: object) => void;
      renderButton: (parent: HTMLElement, options: object) => void;
    };
  };
};

type MainTab = 'account' | 'settings' | 'vipps' | 'developer';
type AccountMode = 'profile' | 'login' | 'register';
type RegisterType = 'person' | 'merchant';
type AccordionSection = 'profile';

const NOTIF_KEY = 'sbys_notifications_enabled';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit, AfterViewInit {
  readonly isProduction = environment.production;

  mainTab = signal<MainTab>('account');
  accountMode = signal<AccountMode>('login');
  registerType = signal<RegisterType>('person');
  accordionSection = signal<AccordionSection | null>('profile');

  isLoading = signal(false);
  isSaving = signal(false);
  saveSuccess = signal(false);
  saveError = signal(false);
  profileType = signal<ParticipantType | null>(null);
  companyName = signal('');

  name = signal('');
  email = signal('');
  phone = signal('');
  address = signal('');
  postalCode = signal('');
  city = signal('');
  country = signal('Danmark');
  notificationsEnabled = signal(true);

  loginEmail = '';
  loginPassword = '';
  showLoginPassword = signal(false);
  loginError = signal<string | null>(null);
  loginLoading = signal(false);

  personName = '';
  personEmail = '';
  personPhone = '';
  personPassword = '';
  personPasswordConfirm = '';

  merchantName = '';
  merchantCompany = '';
  merchantEmail = '';
  merchantPassword = '';
  merchantPasswordConfirm = '';
  merchantMsn = '';
  merchantCvr = '';
  merchantContact = '';
  merchantContactEmail = '';
  merchantPhone = '';
  merchantAddress = '';

  registerError = signal<string | null>(null);
  registerLoading = signal(false);

  vippsTestPersons = signal<VippsTestPersonDto[]>([]);
  selectedVippsTestUserId = signal<number | null>(null);
  vippsLoading = signal(false);
  vippsLoaded = signal(false);
  vippsSaving = signal(false);
  vippsSaveSuccess = signal(false);
  vippsError = signal<string | null>(null);

  persons = signal<DirectoryEntry[]>([]);
  selectedEmail = '';
  developerLoaded = signal(false);
  developerLoading = signal(false);
  developerLoginError = signal<string | null>(null);
  developerLoginLoading = signal(false);
  resetLoading = signal(false);
  resetMessage = signal<string | null>(null);

  private googleRendered = false;

  constructor(
    readonly auth: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute,
    private readonly profileService: ProfileService,
    protected readonly themeService: ThemeService,
    private readonly directory: DirectoryService,
    private readonly devService: DevService
  ) {}

  ngOnInit(): void {
    this.notificationsEnabled.set(localStorage.getItem(NOTIF_KEY) !== 'false');

    const isLoggedIn = this.auth.isLoggedIn();
    const requestedMode = this.route.snapshot.queryParamMap.get('mode');
    if (isLoggedIn) {
      this.accountMode.set('profile');
      this.accordionSection.set('profile');
    } else if (requestedMode === 'register') {
      this.accountMode.set('register');
    } else {
      this.accountMode.set('login');
    }

    if (isLoggedIn) this.loadProfile();
  }

  ngAfterViewInit(): void {
    this.renderGoogleButton();
  }

  effectiveParticipantType(): ParticipantType | null {
    return this.auth.currentUserType() ?? this.profileType();
  }

  canUseVipps(): boolean {
    return this.auth.isLoggedIn() && this.effectiveParticipantType() === 'Person';
  }

  toggleAccordion(section: AccordionSection): void {
    this.accordionSection.update(current => current === section ? null : section);
  }

  selectMainTab(tab: MainTab): void {
    if (tab === 'settings' && !this.auth.isLoggedIn()) return;
    if (tab === 'vipps' && !this.canUseVipps()) return;
    if (tab === 'developer' && this.isProduction) return;

    this.mainTab.set(tab);
    if (tab === 'vipps') this.loadVippsMapping();
    if (tab === 'developer') this.loadDeveloperAccounts();
  }

  setAccountMode(mode: AccountMode): void {
    if (this.auth.isLoggedIn() ? mode !== 'profile' : mode === 'profile') return;
    this.accountMode.set(mode);
    this.loginError.set(null);
    this.registerError.set(null);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { mode },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });

    if (mode === 'login') {
      this.googleRendered = false;
      setTimeout(() => this.renderGoogleButton());
    }
  }

  setRegisterType(type: RegisterType): void {
    this.registerType.set(type);
    this.registerError.set(null);
  }

  onTabKeydown(event: KeyboardEvent, current: MainTab): void {
    const tabs: MainTab[] = ['account'];
    if (this.auth.isLoggedIn()) tabs.push('settings');
    if (this.canUseVipps()) tabs.push('vipps');
    if (!this.isProduction) tabs.push('developer');

    let index = tabs.indexOf(current);
    if (event.key === 'ArrowRight') index = (index + 1) % tabs.length;
    else if (event.key === 'ArrowLeft') index = (index - 1 + tabs.length) % tabs.length;
    else if (event.key === 'Home') index = 0;
    else if (event.key === 'End') index = tabs.length - 1;
    else return;

    event.preventDefault();
    const next = tabs[index];
    this.selectMainTab(next);
    document.getElementById(`profile-tab-${next}`)?.focus();
  }

  private loadProfile(): void {
    const userId = this.auth.currentUserId();
    if (!userId) return;

    this.isLoading.set(true);
    this.profileService.getProfile(userId).subscribe({
      next: (profile: ParticipantApiDto) => {
        this.name.set(profile.name);
        this.email.set(profile.email ?? '');
        this.phone.set(profile.phone ?? '');
        this.address.set(profile.address ?? '');
        this.postalCode.set(profile.postalCode ?? '');
        this.city.set(profile.city ?? '');
        this.country.set(profile.country ?? 'Danmark');
        this.companyName.set(profile.companyName ?? '');
        this.profileType.set(profile.type === 'Merchant' ? 'Merchant' : 'Person');
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.saveError.set(true);
      }
    });
  }

  login(): void {
    const email = this.loginEmail.trim();
    if (!email || !this.loginPassword) return;

    this.loginLoading.set(true);
    this.loginError.set(null);
    this.auth.login(email, this.loginPassword).subscribe({
      next: (response) => this.finishAuthentication(response),
      error: () => {
        this.loginLoading.set(false);
        this.loginError.set('Email eller adgangskode er forkert.');
      }
    });
  }

  toggleLoginPasswordVisibility(): void {
    this.showLoginPassword.update(value => !value);
  }

  private finishAuthentication(response: LoginResponse): void {
    this.loginLoading.set(false);
    this.registerLoading.set(false);
    if (response.participantType === 'Merchant') {
      this.profileType.set('Merchant');
      this.mainTab.set('account');
      this.accountMode.set('profile');
      this.accordionSection.set('profile');
      this.loadProfile();
      this.router.navigate(['/profile'], {
        queryParams: { mode: 'profile' },
        replaceUrl: true
      });
    } else {
      this.router.navigate(['/home']);
    }
  }

  private renderGoogleButton(): void {
    if (this.googleRendered || this.accountMode() !== 'login' || typeof google === 'undefined') return;
    const button = document.getElementById('profile-google-signin-btn');
    if (!button) return;

    google.accounts.id.initialize({
      client_id: environment.googleClientId,
      callback: (response: { credential: string }) => this.handleGoogleResponse(response)
    });
    google.accounts.id.renderButton(button, {
      theme: 'outline',
      size: 'large',
      width: 312,
      text: 'continue_with',
      locale: 'da'
    });
    this.googleRendered = true;
  }

  private handleGoogleResponse(response: { credential: string }): void {
    this.loginLoading.set(true);
    this.loginError.set(null);
    this.auth.googleLogin(response.credential).subscribe({
      next: () => this.router.navigate(['/home']),
      error: (error) => {
        this.loginLoading.set(false);
        this.loginError.set(error?.error?.error ?? 'Google-login mislykkedes. Prøv igen.');
      }
    });
  }

  canRegisterPerson(): boolean {
    return !!this.personName.trim() &&
      !!this.personEmail.trim() &&
      this.personPassword.length >= 6 &&
      this.personPassword === this.personPasswordConfirm;
  }

  canRegisterMerchant(): boolean {
    return !!this.merchantName.trim() &&
      !!this.merchantCompany.trim() &&
      !!this.merchantEmail.trim() &&
      !!this.merchantMsn.trim() &&
      this.merchantPassword.length >= 6 &&
      this.merchantPassword === this.merchantPasswordConfirm;
  }

  registerPerson(): void {
    if (!this.canRegisterPerson()) {
      this.registerError.set('Udfyld de obligatoriske felter, og kontrollér adgangskoderne.');
      return;
    }

    this.registerLoading.set(true);
    this.registerError.set(null);
    this.auth.register({
      name: this.personName.trim(),
      email: this.personEmail.trim(),
      phone: this.personPhone.trim() || undefined,
      password: this.personPassword
    }).subscribe({
      next: (response) => this.finishAuthentication(response),
      error: (error) => {
        this.registerLoading.set(false);
        this.registerError.set(error.status === 409
          ? 'Der findes allerede en konto med denne email.'
          : 'Kontoen kunne ikke oprettes. Prøv igen.');
      }
    });
  }

  registerMerchant(): void {
    if (!this.canRegisterMerchant()) {
      this.registerError.set('Udfyld firmanavn, konto-email, adgangskode og Vipps MSN.');
      return;
    }

    this.registerLoading.set(true);
    this.registerError.set(null);
    this.auth.registerMerchant({
      name: this.merchantName.trim(),
      companyName: this.merchantCompany.trim(),
      email: this.merchantEmail.trim(),
      password: this.merchantPassword,
      vippsMerchantSerialNumber: this.merchantMsn.trim(),
      cvrNumber: this.merchantCvr.trim() || undefined,
      contactPerson: this.merchantContact.trim() || undefined,
      contactEmail: this.merchantContactEmail.trim() || undefined,
      contactPhone: this.merchantPhone.trim() || undefined,
      companyAddress: this.merchantAddress.trim() || undefined
    }).subscribe({
      next: (response) => this.finishAuthentication(response),
      error: (error) => {
        this.registerLoading.set(false);
        this.registerError.set(error.status === 409
          ? 'Der findes allerede en konto med denne email.'
          : error?.error?.error ?? 'Merchantkontoen kunne ikke oprettes. Prøv igen.');
      }
    });
  }

  saveProfile(): void {
    if (!this.name().trim()) return;
    const userId = this.auth.currentUserId();
    if (!userId) return;

    this.isSaving.set(true);
    this.saveSuccess.set(false);
    this.saveError.set(false);

    const request: UpdateProfileRequest = {
      name: this.name().trim(),
      email: this.email().trim() || undefined,
      phone: this.phone().trim() || undefined,
      address: this.address().trim() || undefined,
      postalCode: this.postalCode().trim() || undefined,
      city: this.city().trim() || undefined,
      country: this.country().trim() || undefined
    };

    this.profileService.updateProfile(userId, request).subscribe({
      next: () => {
        this.auth.updateStoredName(request.name);
        this.isSaving.set(false);
        this.saveSuccess.set(true);
        setTimeout(() => this.saveSuccess.set(false), 3000);
      },
      error: () => {
        this.isSaving.set(false);
        this.saveError.set(true);
      }
    });
  }

  toggleNotifications(): void {
    const enabled = !this.notificationsEnabled();
    this.notificationsEnabled.set(enabled);
    localStorage.setItem(NOTIF_KEY, enabled ? 'true' : 'false');
  }

  logout(): void {
    this.auth.logout();
    this.profileType.set(null);
    this.mainTab.set('account');
    this.accountMode.set('login');
    this.accordionSection.set('profile');
    this.router.navigate(['/profile'], {
      queryParams: { mode: 'login' },
      replaceUrl: true
    });
    this.googleRendered = false;
    setTimeout(() => this.renderGoogleButton());
  }

  private loadVippsMapping(): void {
    if (this.vippsLoaded() || this.vippsLoading() || !this.canUseVipps()) return;
    this.vippsLoading.set(true);
    this.vippsError.set(null);

    const userId = this.auth.currentUserId();
    this.profileService.getVippsTestPersons().subscribe({
      next: (persons) => {
        this.vippsTestPersons.set(persons);
        const mapping = persons.find(person => person.mappedByParticipantId === userId);
        this.selectedVippsTestUserId.set(mapping?.id ?? null);
        this.vippsLoaded.set(true);
        this.vippsLoading.set(false);
      },
      error: () => {
        this.vippsLoading.set(false);
        this.vippsError.set('Vipps-testpersoner kunne ikke hentes.');
      }
    });
  }

  isVippsPersonDisabled(person: VippsTestPersonDto): boolean {
    const userId = this.auth.currentUserId();
    return person.mappedByParticipantId != null && person.mappedByParticipantId !== userId;
  }

  saveVippsMapping(): void {
    const userId = this.auth.currentUserId();
    if (!userId || !this.canUseVipps()) return;

    this.vippsSaving.set(true);
    this.vippsSaveSuccess.set(false);
    this.vippsError.set(null);
    this.profileService.setVippsTestUser(userId, this.selectedVippsTestUserId()).subscribe({
      next: () => {
        this.vippsSaving.set(false);
        this.vippsSaveSuccess.set(true);
        this.vippsLoaded.set(false);
        this.loadVippsMapping();
        setTimeout(() => this.vippsSaveSuccess.set(false), 3000);
      },
      error: () => {
        this.vippsSaving.set(false);
        this.vippsError.set('Mappingen kunne ikke gemmes.');
      }
    });
  }

  private loadDeveloperAccounts(): void {
    if (this.isProduction || this.developerLoaded() || this.developerLoading()) return;
    this.developerLoading.set(true);
    this.directory.search('').subscribe({
      next: (list) => {
        this.persons.set(list.filter(entry => entry.type === 'Person'));
        this.developerLoaded.set(true);
        this.developerLoading.set(false);
      },
      error: () => {
        this.persons.set([]);
        this.developerLoaded.set(true);
        this.developerLoading.set(false);
        this.developerLoginError.set('Testkonti kunne ikke hentes.');
      }
    });
  }

  developerLogin(): void {
    if (!this.selectedEmail || this.isProduction) return;

    this.developerLoginLoading.set(true);
    this.developerLoginError.set(null);
    this.auth.login(this.selectedEmail, undefined).subscribe({
      next: () => {
        this.developerLoginLoading.set(false);
        this.router.navigate(['/home']);
      },
      error: (error) => {
        this.developerLoginLoading.set(false);
        this.developerLoginError.set(
          error.status === 404
            ? 'Udviklerlogin findes kun, når backend kører i Development.'
            : 'Testlogin fejlede.'
        );
      }
    });
  }

  developerReset(): void {
    if (this.isProduction) return;
    if (!confirm('Dette sletter alle ordrer, betalinger og beskeder i udviklingsdatabasen. Fortsæt?')) return;

    this.resetLoading.set(true);
    this.resetMessage.set(null);
    this.devService.resetData().subscribe({
      next: () => {
        this.resetLoading.set(false);
        this.resetMessage.set('Udviklingsdata er nulstillet.');
      },
      error: (error) => {
        this.resetLoading.set(false);
        this.resetMessage.set(error.status === 404
          ? 'Reset findes kun, når backend kører i Development.'
          : 'Data kunne ikke nulstilles.');
      }
    });
  }
}
