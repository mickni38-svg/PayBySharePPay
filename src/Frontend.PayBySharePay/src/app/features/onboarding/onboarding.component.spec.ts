import { of } from 'rxjs';
import { OnboardingComponent } from './onboarding.component';

describe('OnboardingComponent', () => {
  function createComponent() {
    let loggedIn = false;
    let userId: number | null = null;
    const auth = {
      isLoggedIn: jasmine.createSpy('isLoggedIn').and.callFake(() => loggedIn),
      currentUserId: jasmine.createSpy('currentUserId').and.callFake(() => userId),
      getRegistrationPhoneOptions: jasmine.createSpy('getRegistrationPhoneOptions').and.returnValue(of({
        enabled: true,
        phoneNumbers: ['231 27 779', '635 50 321']
      })),
      register: jasmine.createSpy('register').and.callFake(() => {
        loggedIn = true;
        userId = 7;
        return of({
          token: 'token',
          participantId: 7,
          name: 'Michael Nielsen',
          participantType: 'Person',
          expiresAt: '2099-01-01T00:00:00Z'
        });
      })
    };
    const directory = {
      search: jasmine.createSpy('search').and.returnValue(of([
        { id: 8, type: 'Person', displayName: 'Dayna Runolfsdottir', handle: 'dayna@example.com' },
        { id: 9, type: 'Merchant', displayName: 'Bella Napoli', subtitle: 'Spisested' }
      ]))
    };
    const friends = {
      addFriend: jasmine.createSpy('addFriend').and.returnValue(of(void 0))
    };
    const router = { navigate: jasmine.createSpy('navigate') };

    const component = new OnboardingComponent(auth as any, directory as any, friends as any, router as any);
    component.personName = 'Michael Nielsen';
    component.personEmail = 'michael@example.com';
    component.personPhone = '231 27 779';
    component.personPassword = 'hemmelig';
    component.personPasswordConfirm = 'hemmelig';

    return { component, auth, directory, friends, router };
  }

  it('starts on profile step and validates required profile fields', () => {
    const { component } = createComponent();
    expect(component.step()).toBe(1);
    expect(component.canContinueProfile()).toBeTrue();
    component.personPasswordConfirm = 'forkert';
    expect(component.canContinueProfile()).toBeFalse();
  });

  it('requires a phone number from the available Vipps test pool', () => {
    const { component } = createComponent();
    expect(component.availablePhoneNumbers()).toEqual(['231 27 779', '635 50 321']);
    component.personPhone = '99999999';
    expect(component.canContinueProfile()).toBeFalse();
    component.personPhone = '635 50 321';
    expect(component.canContinueProfile()).toBeTrue();
  });

  it('creates and authenticates the account before loading protected directory data', () => {
    const { component, auth, directory } = createComponent();
    component.next();
    expect(auth.register).toHaveBeenCalledWith(jasmine.objectContaining({ phone: '231 27 779' }));
    expect(component.accountCreated()).toBeTrue();
    expect(component.step()).toBe(2);
    expect(directory.search).toHaveBeenCalledWith('', 7);
  });

  it('keeps friend and merchant selections while navigating between steps', () => {
    const { component } = createComponent();
    component.next();
    component.toggleFriend(8);
    component.next();
    component.toggleMerchant(9);
    component.back();
    expect(component.step()).toBe(2);
    expect(component.isFriendSelected(8)).toBeTrue();
    expect(component.isMerchantSelected(9)).toBeTrue();
  });

  it('saves selected people and merchants through the existing friend relation endpoint', () => {
    const { component, friends, router } = createComponent();
    component.next();
    component.toggleFriend(8);
    component.next();
    component.toggleMerchant(9);
    component.next();
    component.complete();

    expect(friends.addFriend).toHaveBeenCalledWith({ initiatorId: 7, receiverId: 8 });
    expect(friends.addFriend).toHaveBeenCalledWith({ initiatorId: 7, receiverId: 9 });
    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });

  it('allows onboarding to finish without selecting friends or merchants', () => {
    const { component, friends, router } = createComponent();
    component.next();
    component.next();
    component.next();
    component.complete();
    expect(friends.addFriend).not.toHaveBeenCalled();
    expect(router.navigate).toHaveBeenCalledWith(['/home']);
  });
});
