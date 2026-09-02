import { of, throwError } from 'rxjs';

import { DirectoryEntry } from '../../core/models/directory.model';
import { CreateOrderComponent } from './create-order.component';

describe('CreateOrderComponent UC-03/UC-04/UC-05 wizard', () => {
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
    loadFails = false,
    createFails = false
  ): {
    component: CreateOrderComponent;
    router: { navigate: jasmine.Spy };
    orderService: { createOrder: jasmine.Spy };
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
    const orderService = {
      createOrder: jasmine.createSpy('createOrder').and.returnValue(
        createFails
          ? throwError(() => ({ error: { message: 'Serverfejl' } }))
          : of({ id: 123 })
      )
    };

    return {
      component: new CreateOrderComponent(
        orderService as any,
        directoryService as any,
        router as any,
        { currentUserId: () => userId } as any
      ),
      router,
      orderService
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

  function openStep3(createFails = false) {
    setMerchantState();
    const result = createComponent([host, merchant, anna, bo], host.id, false, createFails);
    result.component.ngOnInit();
    result.component.togglePerson(result.component.persons()[0]);
    result.component.title.set('  Pizzaaften  ');
    result.component.message.set('Hej alle 👋');
    result.component.goNext();
    result.component.goNext();
    return result;
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
    setMerchantState({ ...merchant, displayName: 'Forældet navn', logoUrl: '/old-logo.png' });
    const { component, router } = createComponent();
    component.ngOnInit();
    expect(component.selectedMerchant()?.id).toBe(merchant.id);
    expect(component.selectedMerchant()?.displayName).toBe('Test Bistro');
    expect(component.selectedMerchant()?.logoUrl).toContain('/api/participants/42/logo');
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('filters out the host and all merchants and removes duplicate participants', () => {
    setMerchantState();
    const otherMerchant: DirectoryEntry = { id: 43, type: 'Merchant', displayName: 'Anden Merchant' };
    const { component } = createComponent([host, merchant, otherMerchant, anna, { ...anna, displayName: 'Dublet Anna' }, bo]);
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
  });

  it('requires one participant before continuing and exposes the required wizard state', () => {
    setMerchantState();
    const { component } = createComponent();
    component.ngOnInit();
    expect(component.canContinue()).toBeFalse();
    component.goNext();
    expect(component.stepError()).toBe('Vælg mindst én deltager');
    component.togglePerson(component.persons()[0]);
    expect(component.wizardState().participantIds).toEqual([anna.id]);
    component.goNext();
    expect(component.currentStep()).toBe(2);
  });

  it('UC-04 rejects whitespace-only titles', () => {
    const component = openStep2();
    component.title.set('   ');
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
    expect(component.wizardState().title).toBe('Fredagspizza');
    expect(component.wizardState().message).toBe(message);
  });

  it('UC-05 exposes dynamic merchant, details and selected participants on review state', () => {
    const { component } = openStep3();
    expect(component.currentStep()).toBe(3);
    expect(component.wizardState().merchant?.displayName).toBe('Test Bistro');
    expect(component.wizardState().title).toBe('Pizzaaften');
    expect(component.wizardState().message).toBe('Hej alle 👋');
    expect(component.wizardState().participants.map(person => person.displayName)).toEqual(['Anna Jensen']);
  });

  it('UC-05 edit actions preserve data while navigating to the relevant step', () => {
    const { component } = openStep3();
    component.editDetails();
    expect(component.currentStep()).toBe(2);
    expect(component.title()).toBe('Pizzaaften');
    expect(component.message()).toBe('Hej alle 👋');
    component.currentStep.set(3);
    component.editParticipants();
    expect(component.currentStep()).toBe(1);
    expect(component.selectedParticipants().map(person => person.id)).toEqual([anna.id]);
  });

  it('UC-05 submits once, sends idempotency key and navigates to the created order', () => {
    const { component, router, orderService } = openStep3();
    component.submit();
    component.submit();

    expect(orderService.createOrder).toHaveBeenCalledTimes(1);
    const request = orderService.createOrder.calls.mostRecent().args[0];
    expect(request.createdByParticipantId).toBe(host.id);
    expect(request.merchantParticipantId).toBe(merchant.id);
    expect(request.participantIds).toEqual([anna.id]);
    expect(request.title).toBe('Pizzaaften');
    expect(request.message).toBe('Hej alle 👋');
    expect(request.idempotencyKey.length).toBeGreaterThanOrEqual(8);
    expect(router.navigate).toHaveBeenCalledWith(['/orders', 123]);
  });

  it('UC-05 keeps wizard data and re-enables submit after a create error', () => {
    const { component, router } = openStep3(true);
    component.submit();

    expect(component.currentStep()).toBe(3);
    expect(component.title()).toBe('Pizzaaften');
    expect(component.selectedParticipants().map(person => person.id)).toEqual([anna.id]);
    expect(component.isSubmitting()).toBeFalse();
    expect(component.errorMessage()).toBe('Serverfejl');
    expect(router.navigate).not.toHaveBeenCalledWith(['/orders', 123]);
  });
});
