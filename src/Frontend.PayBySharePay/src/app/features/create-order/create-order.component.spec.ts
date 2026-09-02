import { of, throwError } from 'rxjs';

import { DirectoryEntry } from '../../core/models/directory.model';
import { CreateOrderComponent } from './create-order.component';

describe('CreateOrderComponent UC-03/UC-04 wizard', () => {
  const host: DirectoryEntry = {
    id: 7,
    type: 'Person',
    displayName: 'Vært Hansen',
    handle: 'vaert@paynsync.dk'
  };
  const merchant: DirectoryEntry = {
    id: 42,
    type: 'Merchant',
    displayName: 'Test Bistro',
    handle: 'test-bistro',
    logoUrl: '/api/participants/42/logo'
  };
  const anna: DirectoryEntry = {
    id: 8,
    type: 'Person',
    displayName: 'Anna Jensen',
    handle: 'anna@paynsync.dk'
  };
  const bo: DirectoryEntry = {
    id: 9,
    type: 'Person',
    displayName: 'Bo Møller',
    handle: 'bo@paynsync.dk'
  };

  afterEach(() => {
    history.replaceState({}, '', window.location.href);
  });

  function setMerchantState(entry: DirectoryEntry = merchant): void {
    history.replaceState({
      merchant: {
        id: entry.id,
        displayName: entry.displayName,
        handle: entry.handle,
        logoUrl: entry.logoUrl ?? null,
        fallbackLogoUrl: null
      }
    }, '', window.location.href);
  }

  function createComponent(
    friends: DirectoryEntry[] = [host, merchant, anna, bo],
    userId: number | null = host.id,
    loadFails = false
  ): {
    component: CreateOrderComponent;
    router: { navigate: jasmine.Spy };
  } {
    const router = {
      navigate: jasmine.createSpy('navigate')
    };
    const directoryService = {
      getFriends: jasmine.createSpy('getFriends').and.returnValue(
        loadFails
          ? throwError(() => new Error('network error'))
          : of(friends)
      )
    };

    return {
      component: new CreateOrderComponent(
        {} as any,
        directoryService as any,
        router as any,
        { currentUserId: () => userId } as any
      ),
      router
    };
  }

  function openStep2(): CreateOrderComponent {
    setMerchantState();
    const { component } = createComponent();
    component.ngOnInit();
    component.togglePerson(component.persons()[0]);
    component.goNext();
    return component;
  }

  it('redirects home when opened without merchant state', () => {
    history.replaceState({}, '', window.location.href);
    const { component, router } = createComponent();

    component.ngOnInit();

    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('redirects home when the requested merchant is not a current merchant friend', () => {
    setMerchantState();
    const { component, router } = createComponent([host, anna, bo]);

    component.ngOnInit();

    expect(router.navigate).toHaveBeenCalledWith(['/home']);
    expect(component.selectedMerchant()).toBeNull();
  });

  it('uses the validated merchant data from the existing friends service', () => {
    setMerchantState({
      ...merchant,
      displayName: 'Forældet navn',
      logoUrl: '/old-logo.png'
    });
    const { component, router } = createComponent();

    component.ngOnInit();

    expect(component.selectedMerchant()?.id).toBe(merchant.id);
    expect(component.selectedMerchant()?.displayName).toBe('Test Bistro');
    expect(component.selectedMerchant()?.logoUrl).toContain('/api/participants/42/logo');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('filters out the host and all merchants and removes duplicate participants', () => {
    setMerchantState();
    const otherMerchant: DirectoryEntry = {
      id: 43,
      type: 'Merchant',
      displayName: 'Anden Merchant'
    };
    const { component } = createComponent([
      host,
      merchant,
      otherMerchant,
      anna,
      { ...anna, displayName: 'Dublet Anna' },
      bo
    ]);

    component.ngOnInit();

    expect(component.persons().map(person => person.id)).toEqual([anna.id, bo.id]);
    expect(component.persons().every(person => person.type === 'Person')).toBeTrue();
  });

  it('searches participants reactively by name and handle', () => {
    setMerchantState();
    const { component } = createComponent();

    component.ngOnInit();
    component.searchTerm.set('møL');
    expect(component.filtered().map(person => person.id)).toEqual([bo.id]);

    component.searchTerm.set('ANNA@');
    expect(component.filtered().map(person => person.id)).toEqual([anna.id]);

    component.searchTerm.set('findes-ikke');
    expect(component.filtered()).toEqual([]);
  });

  it('requires one participant before continuing and exposes the required wizard state', () => {
    setMerchantState();
    const { component } = createComponent();

    component.ngOnInit();

    expect(component.canContinue()).toBeFalse();
    component.goNext();
    expect(component.currentStep()).toBe(1);
    expect(component.stepError()).toBe('Vælg mindst én deltager');

    component.togglePerson(component.persons()[0]);
    expect(component.canContinue()).toBeTrue();
    expect(component.wizardState().hostUserId).toBe(host.id);
    expect(component.wizardState().merchantId).toBe(merchant.id);
    expect(component.wizardState().participantIds).toEqual([anna.id]);

    component.goNext();
    expect(component.currentStep()).toBe(2);
  });

  it('selects and deselects without duplicates', () => {
    setMerchantState();
    const { component } = createComponent();

    component.ngOnInit();
    const participant = component.persons()[0];

    component.togglePerson(participant);
    expect(component.selectedParticipants().map(person => person.id)).toEqual([anna.id]);

    component.togglePerson(component.persons()[0]);
    expect(component.selectedParticipants()).toEqual([]);

    component.togglePerson(component.persons()[0]);
    expect(component.wizardState().participantIds).toEqual([anna.id]);
  });

  it('preserves selected participants when navigating forward and back', () => {
    setMerchantState();
    const { component } = createComponent();

    component.ngOnInit();
    component.togglePerson(component.persons()[1]);
    component.goNext();
    component.goBack();

    expect(component.currentStep()).toBe(1);
    expect(component.selectedParticipants().map(person => person.id)).toEqual([bo.id]);
  });

  it('does not allow the host or merchant to be selected defensively', () => {
    setMerchantState();
    const { component } = createComponent();

    component.ngOnInit();
    const candidate = component.persons()[0];

    component.togglePerson({ ...candidate, id: host.id });
    component.togglePerson({ ...candidate, id: merchant.id });

    expect(component.selectedParticipants()).toEqual([]);
  });

  it('shows a stable error state when participants cannot be loaded', () => {
    setMerchantState();
    const { component, router } = createComponent([], host.id, true);

    component.ngOnInit();

    expect(component.isLoading()).toBeFalse();
    expect(component.loadError()).toBe('Kunne ikke hente deltagere. Prøv igen.');
    expect(component.persons()).toEqual([]);
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('UC-04 rejects empty and whitespace-only titles', () => {
    const component = openStep2();

    component.title.set('   ');
    expect(component.canContinue()).toBeFalse();
    component.goNext();

    expect(component.currentStep()).toBe(2);
    expect(component.stepError()).toBe('Titel skal udfyldes');
  });

  it('UC-04 accepts 80 title characters, rejects 81, and trims before storing', () => {
    const component = openStep2();

    component.title.set('x'.repeat(80));
    expect(component.canContinue()).toBeTrue();

    component.title.set('x'.repeat(81));
    expect(component.canContinue()).toBeFalse();

    component.title.set('  Pizzaaften  ');
    component.goNext();

    expect(component.currentStep()).toBe(3);
    expect(component.title()).toBe('Pizzaaften');
    expect(component.wizardState().title).toBe('Pizzaaften');
  });

  it('UC-04 accepts an empty or 500-character message and rejects 501 characters', () => {
    const component = openStep2();
    component.title.set('Pizzaaften');

    component.message.set('');
    expect(component.canContinue()).toBeTrue();

    component.message.set('a'.repeat(500));
    expect(component.canContinue()).toBeTrue();

    component.message.set('a'.repeat(501));
    expect(component.canContinue()).toBeFalse();
  });

  it('UC-04 preserves multiline Danish text and emoji exactly in wizard state', () => {
    const component = openStep2();
    const message = 'Hej alle 👋\nVi mødes kl. 18.30\nGlæder mig til pizza 🍕';

    component.title.set('  Fredagspizza  ');
    component.message.set(message);
    component.goNext();

    expect(component.currentStep()).toBe(3);
    expect(component.wizardState().title).toBe('Fredagspizza');
    expect(component.wizardState().message).toBe(message);
  });

  it('UC-04 exposes the dynamic merchant and participant count from step 1 state', () => {
    const component = openStep2();

    expect(component.wizardState().merchant?.displayName).toBe('Test Bistro');
    expect(component.wizardState().participantIds).toEqual([anna.id]);
    expect(component.selectedParticipants().length).toBe(1);
  });

  it('UC-04 preserves title, message and participants when navigating back and forward', () => {
    const component = openStep2();
    const message = 'Linje 1\nLinje 2 😊';

    component.title.set('Pizzaaften');
    component.message.set(message);
    component.goBack();

    expect(component.currentStep()).toBe(1);
    expect(component.selectedParticipants().map(person => person.id)).toEqual([anna.id]);

    component.goNext();
    expect(component.currentStep()).toBe(2);
    expect(component.title()).toBe('Pizzaaften');
    expect(component.message()).toBe(message);
  });

  it('UC-04 returns to participant step if participants become invalid before continuing', () => {
    const component = openStep2();
    component.goBack();
    component.togglePerson(component.persons()[0]);
    component.currentStep.set(2);
    component.title.set('Pizzaaften');

    component.goNext();

    expect(component.currentStep()).toBe(1);
    expect(component.stepError()).toBe('Vælg mindst én deltager');
  });
});
