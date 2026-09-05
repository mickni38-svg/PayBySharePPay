import assert from 'node:assert/strict';
import test from 'node:test';
import {
  buildDraftPayload,
  cartTotal,
  createCartLine,
  findProduct,
  validateCatalog,
  withLineQuantity
} from './order-model.js';

test('catalog uses unique stable category, product and modifier ids', () => {
  assert.deepEqual(validateCatalog(), {
    categoryIdsUnique: true,
    productIdsUnique: true,
    modifierIdsUnique: true
  });
});

test('cart line calculates base price times quantity', () => {
  const product = findProduct('roma-pizza-margherita');
  const line = createCartLine(product, [], 2, 'line-1');

  assert.equal(line.unitPrice, 89);
  assert.equal(line.lineTotal, 178);
  assert.equal(line.name, 'Margherita');
});

test('selected modifiers are included per item and in readable name', () => {
  const product = findProduct('roma-pizza-pepperoni');
  const line = createCartLine(product, ['roma-option-pepperoni-extra-cheese'], 2, 'line-1');

  assert.equal(line.unitPrice, 120);
  assert.equal(line.lineTotal, 240);
  assert.match(line.name, /Ekstra mozzarella/);
});

test('same product with different modifiers remains separate lines', () => {
  const product = findProduct('roma-pizza-margherita');
  const cart = [
    createCartLine(product, ['roma-option-margherita-chili'], 1, 'line-chili'),
    createCartLine(product, ['roma-option-margherita-gluten-free'], 1, 'line-gluten-free')
  ];

  assert.equal(cart.length, 2);
  assert.notEqual(cart[0].lineInstanceId, cart[1].lineInstanceId);
  assert.equal(cartTotal(cart), 203);
});

test('quantity changes update cart and payload totals consistently', () => {
  const product = findProduct('roma-side-fries');
  let cart = [createCartLine(product, ['roma-option-fries-aioli'], 1, 'line-fries')];
  cart = withLineQuantity(cart, 'line-fries', 3);

  const payload = buildDraftPayload({
    cart,
    orderId: 42,
    merchantId: 9,
    participantToken: 'participant-token',
    merchantDraftReference: 'ROMA-42-TEST',
    now: new Date('2026-09-05T12:00:00Z')
  });

  assert.equal(cartTotal(cart), 156);
  assert.equal(payload.totalAmount, 156);
  assert.equal(payload.lines[0].lineTotal, 156);
});

test('draft payload contains stable product and modifier ids in normalized and raw data', () => {
  const product = findProduct('roma-pizza-verdure');
  const cart = [createCartLine(product, ['roma-option-verdure-chili'], 1, 'line-verdure')];

  const payload = buildDraftPayload({
    cart,
    orderId: 42,
    merchantId: 9,
    participantToken: 'participant-token',
    merchantDraftReference: 'ROMA-42-TEST',
    testPhoneNumber: '63550321',
    now: new Date('2026-09-05T12:00:00Z')
  });
  const raw = JSON.parse(payload.rawMerchantPayloadJson);

  assert.equal(payload.lines[0].lineId, 'roma-pizza-verdure');
  assert.equal(raw.items[0].productId, 'roma-pizza-verdure');
  assert.equal(raw.items[0].modifiers[0].id, 'roma-option-verdure-chili');
  assert.equal(payload.testPhoneNumber, '63550321');
  assert.equal(payload.expiresAtUtc, '2026-09-06T12:00:00.000Z');
});

test('empty cart cannot create a payment draft', () => {
  assert.throws(() => buildDraftPayload({
    cart: [],
    orderId: 42,
    merchantId: 9,
    participantToken: 'participant-token',
    merchantDraftReference: 'ROMA-42-TEST'
  }), /Kurven er tom/);
});
