import { of } from 'rxjs';

import { AuthService, LoginResponse, RegisterMerchantRequest } from './auth.service';

describe('AuthService', () => {
  const merchantResponse: LoginResponse = {
    token: 'merchant-token',
    participantId: 42,
    name: 'Test Merchant',
    participantType: 'Merchant',
    expiresAt: '2099-01-01T00:00:00Z'
  };

  beforeEach(() => localStorage.clear());

  it('sends all required merchant account fields and stores the participant type', () => {
    const http = {
      post: jasmine.createSpy('post').and.returnValue(of(merchantResponse))
    };
    const service = new AuthService(http as any);
    const request: RegisterMerchantRequest = {
      name: 'Merchant Owner',
      companyName: 'Example ApS',
      email: 'merchant@example.com',
      password: 'hemmelig',
      vippsMerchantSerialNumber: '123456'
    };

    service.registerMerchant(request).subscribe();

    expect(http.post).toHaveBeenCalledWith(
      jasmine.stringMatching(/\/api\/auth\/register-merchant$/),
      request
    );
    expect(service.isLoggedIn()).toBeTrue();
    expect(service.currentUserType()).toBe('Merchant');
    expect(JSON.parse(localStorage.getItem('sbys_user') ?? '{}')).toEqual({
      participantId: 42,
      name: 'Test Merchant',
      participantType: 'Merchant'
    });
  });

  it('includes a password in normal login and stores person identity', () => {
    const response: LoginResponse = {
      ...merchantResponse,
      token: 'person-token',
      participantId: 7,
      name: 'Test Person',
      participantType: 'Person'
    };
    const http = {
      post: jasmine.createSpy('post').and.returnValue(of(response))
    };
    const service = new AuthService(http as any);

    service.login('person@example.com', 'hemmelig').subscribe();

    expect(http.post).toHaveBeenCalledWith(
      jasmine.stringMatching(/\/api\/auth\/login$/),
      { email: 'person@example.com', password: 'hemmelig' }
    );
    expect(service.currentUserType()).toBe('Person');
  });

  it('omits password only for the explicit developer-login call', () => {
    const http = {
      post: jasmine.createSpy('post').and.returnValue(of({
        ...merchantResponse,
        participantType: 'Person'
      }))
    };
    const service = new AuthService(http as any);

    service.login('seed@example.com', undefined).subscribe();

    expect(http.post).toHaveBeenCalledWith(
      jasmine.stringMatching(/\/api\/auth\/login$/),
      { email: 'seed@example.com' }
    );
  });
});
