import {
  buildDraftPayload,
  cartTotal,
  catalog,
  createCartLine,
  findProduct,
  withLineQuantity
} from './order-model.js';

const params = new URLSearchParams(window.location.search);
const orderId = parsePositiveInt(params.get('orderId'));
const merchantId = parsePositiveInt(params.get('merchantId'));
const participantToken = params.get('participantToken')?.trim() || '';
const testPhoneNumber = params.get('testPhoneNumber')?.trim() || '';
const isLocal = ['localhost', '127.0.0.1'].includes(window.location.hostname);
const API_BASE = isLocal ? 'https://localhost:7007' : 'https://api.paynsync.dk';
const money = new Intl.NumberFormat('da-DK', {
  style: 'currency',
  currency: 'DKK',
  minimumFractionDigits: 0
});

let cart = [];
let activeProduct = null;
let modalQuantity = 1;
let isSubmitting = false;

const elements = {
  categoryNav: document.querySelector('#categoryNav'),
  menuContent: document.querySelector('#menuContent'),
  groupLabel: document.querySelector('#groupLabel'),
  configError: document.querySelector('#configError'),
  cartPanel: document.querySelector('#cartPanel'),
  cartItems: document.querySelector('#cartItems'),
  cartEmpty: document.querySelector('#cartEmpty'),
  cartCount: document.querySelector('#cartCount'),
  cartTotal: document.querySelector('#cartTotal'),
  checkoutButton: document.querySelector('#checkoutButton'),
  mobileCartButton: document.querySelector('#mobileCartButton'),
  mobileCartCount: document.querySelector('#mobileCartCount'),
  mobileCartTotal: document.querySelector('#mobileCartTotal'),
  errorMessage: document.querySelector('#errorMessage'),
  successPanel: document.querySelector('#successPanel'),
  shopLayout: document.querySelector('#shopLayout'),
  productDialog: document.querySelector('#productDialog'),
  productForm: document.querySelector('#productForm'),
  productEmoji: document.querySelector('#productEmoji'),
  productTitle: document.querySelector('#productTitle'),
  productDescription: document.querySelector('#productDescription'),
  productBasePrice: document.querySelector('#productBasePrice'),
  modifierList: document.querySelector('#modifierList'),
  modalQuantity: document.querySelector('#modalQuantity'),
  addButton: document.querySelector('#addButton'),
  toast: document.querySelector('#toast'),
  merchantLogo: document.querySelector('#merchantLogo'),
  merchantLogoFallback: document.querySelector('#merchantLogoFallback')
};

initialize();

function initialize() {
  renderCategoryNavigation();
  renderMenu();
  renderCart();
  renderGroupContext();
  configureMerchantLogo();
  bindEvents();
}

function bindEvents() {
  elements.menuContent.addEventListener('click', event => {
    const button = event.target.closest('[data-product-id]');
    if (button)
      openProduct(button.dataset.productId);
  });

  elements.productForm.addEventListener('submit', event => {
    event.preventDefault();
    addActiveProductToCart();
  });

  elements.productDialog.querySelector('[data-close-dialog]').addEventListener('click', () => {
    elements.productDialog.close();
  });

  elements.productDialog.addEventListener('click', event => {
    if (event.target === elements.productDialog)
      elements.productDialog.close();
  });

  elements.productForm.addEventListener('change', updateModalPrice);
  elements.productForm.querySelector('[data-modal-action="decrease"]').addEventListener('click', () => changeModalQuantity(-1));
  elements.productForm.querySelector('[data-modal-action="increase"]').addEventListener('click', () => changeModalQuantity(1));

  elements.cartItems.addEventListener('click', event => {
    const button = event.target.closest('[data-cart-action]');
    if (!button)
      return;

    const line = cart.find(item => item.lineInstanceId === button.dataset.lineId);
    if (!line)
      return;

    const action = button.dataset.cartAction;
    if (action === 'remove')
      cart = withLineQuantity(cart, line.lineInstanceId, 0);
    if (action === 'decrease')
      cart = withLineQuantity(cart, line.lineInstanceId, line.quantity - 1);
    if (action === 'increase')
      cart = withLineQuantity(cart, line.lineInstanceId, line.quantity + 1);

    renderCart();
  });

  elements.checkoutButton.addEventListener('click', submitOrder);
  elements.mobileCartButton.addEventListener('click', () => {
    elements.cartPanel.scrollIntoView({ behavior: 'smooth', block: 'start' });
    elements.cartPanel.focus({ preventScroll: true });
  });
}

function renderGroupContext() {
  if (orderId && participantToken) {
    elements.groupLabel.textContent = `Du bestiller til PayNSync-gruppeordre #${orderId}`;
    elements.configError.hidden = true;
    return;
  }

  const missing = [!orderId && 'orderId', !participantToken && 'participantToken'].filter(Boolean).join(' og ');
  elements.groupLabel.textContent = 'Demoen mangler oplysninger om gruppeordren';
  elements.configError.textContent = `Linket mangler ${missing}. Åbn restauranten fra din PayNSync-gruppeordre.`;
  elements.configError.hidden = false;
}

function configureMerchantLogo() {
  if (!merchantId)
    return;

  elements.merchantLogo.src = `${API_BASE}/api/participants/${merchantId}/logo`;
  elements.merchantLogo.addEventListener('load', () => {
    elements.merchantLogo.hidden = false;
    elements.merchantLogoFallback.hidden = true;
  });
  elements.merchantLogo.addEventListener('error', () => {
    elements.merchantLogo.hidden = true;
    elements.merchantLogoFallback.hidden = false;
  });
}

function renderCategoryNavigation() {
  elements.categoryNav.innerHTML = catalog.map((category, index) => `
    <a class="category-link${index === 0 ? ' category-link--active' : ''}" href="#category-${category.id}">
      ${escapeHtml(category.name)}
    </a>`).join('');
}

function renderMenu() {
  elements.menuContent.innerHTML = catalog.map(category => `
    <section class="menu-section" id="category-${category.id}" aria-labelledby="heading-${category.id}">
      <div class="section-heading">
        <div>
          <h2 id="heading-${category.id}">${escapeHtml(category.name)}</h2>
          <p>${escapeHtml(category.description)}</p>
        </div>
        <span>${category.products.length} valg</span>
      </div>
      <div class="product-grid">
        ${category.products.map(renderProduct).join('')}
      </div>
    </section>`).join('');
}

function renderProduct(product) {
  const badges = product.badges.map(badge => `<span class="product-badge">${escapeHtml(badge)}</span>`).join('');
  const modifierHint = product.modifiers.length > 0 ? 'Kan tilpasses' : 'Klar som den er';

  return `
    <article class="product-card">
      <button class="product-card__button" type="button" data-product-id="${product.id}" aria-label="Tilføj ${escapeHtml(product.name)} til kurven">
        <span class="product-card__copy">
          <span class="product-card__badges">${badges}</span>
          <strong>${escapeHtml(product.name)}</strong>
          <span class="product-card__description">${escapeHtml(product.description)}</span>
          <span class="product-card__meta">
            <b>${money.format(product.price)}</b>
            <span>${modifierHint}</span>
          </span>
        </span>
        <span class="product-card__visual" aria-hidden="true">
          <span>${product.emoji}</span>
          <i>+</i>
        </span>
      </button>
    </article>`;
}

function openProduct(productId) {
  activeProduct = findProduct(productId);
  if (!activeProduct)
    return;

  modalQuantity = 1;
  elements.productEmoji.textContent = activeProduct.emoji;
  elements.productTitle.textContent = activeProduct.name;
  elements.productDescription.textContent = activeProduct.description;
  elements.productBasePrice.textContent = money.format(activeProduct.price);
  elements.modalQuantity.textContent = modalQuantity;
  elements.modifierList.innerHTML = activeProduct.modifiers.length === 0
    ? '<p class="no-modifiers">Denne ret behøver ingen tilvalg.</p>'
    : activeProduct.modifiers.map(modifier => `
      <label class="modifier-option" for="${modifier.id}">
        <span>
          <input id="${modifier.id}" name="modifier" type="checkbox" value="${modifier.id}">
          <span>${escapeHtml(modifier.name)}</span>
        </span>
        <b>${modifier.price === 0 ? '0 kr.' : `+${money.format(modifier.price)}`}</b>
      </label>`).join('');

  updateModalPrice();
  elements.productDialog.showModal();
}

function changeModalQuantity(change) {
  modalQuantity = Math.min(20, Math.max(1, modalQuantity + change));
  elements.modalQuantity.textContent = modalQuantity;
  updateModalPrice();
}

function updateModalPrice() {
  if (!activeProduct)
    return;

  const modifierIds = [...elements.productForm.querySelectorAll('input[name="modifier"]:checked')]
    .map(input => input.value);
  const preview = createCartLine(activeProduct, modifierIds, modalQuantity, 'preview');
  elements.addButton.textContent = `Tilføj · ${money.format(preview.lineTotal)}`;
}

function addActiveProductToCart() {
  const modifierIds = [...elements.productForm.querySelectorAll('input[name="modifier"]:checked')]
    .map(input => input.value);
  const line = createCartLine(activeProduct, modifierIds, modalQuantity);

  cart = [...cart, line];
  elements.productDialog.close();
  renderCart();
  showToast(`${activeProduct.name} er tilføjet`);
}

function renderCart() {
  const count = cart.reduce((sum, line) => sum + line.quantity, 0);
  const total = cartTotal(cart);
  const canCheckout = count > 0 && orderId && participantToken && !isSubmitting;

  elements.cartCount.textContent = `${count} ${count === 1 ? 'vare' : 'varer'}`;
  elements.cartTotal.textContent = money.format(total);
  elements.mobileCartCount.textContent = count;
  elements.mobileCartTotal.textContent = money.format(total);
  elements.checkoutButton.disabled = !canCheckout;
  elements.mobileCartButton.hidden = count === 0;
  elements.cartEmpty.hidden = count > 0;

  elements.cartItems.innerHTML = cart.map(line => `
    <article class="cart-line">
      <div class="cart-line__visual" aria-hidden="true">${line.emoji}</div>
      <div class="cart-line__content">
        <div class="cart-line__title">
          <strong>${escapeHtml(line.productName)}</strong>
          <button type="button" data-cart-action="remove" data-line-id="${line.lineInstanceId}" aria-label="Fjern ${escapeHtml(line.productName)}">Fjern</button>
        </div>
        ${line.modifiers.length > 0 ? `<p>${line.modifiers.map(item => escapeHtml(item.name)).join(', ')}</p>` : ''}
        <div class="cart-line__footer">
          <div class="quantity-control quantity-control--small" aria-label="Antal ${escapeHtml(line.productName)}">
            <button type="button" data-cart-action="decrease" data-line-id="${line.lineInstanceId}" aria-label="Fjern én">−</button>
            <span>${line.quantity}</span>
            <button type="button" data-cart-action="increase" data-line-id="${line.lineInstanceId}" aria-label="Tilføj én">+</button>
          </div>
          <b>${money.format(line.lineTotal)}</b>
        </div>
      </div>
    </article>`).join('');
}

async function submitOrder() {
  if (isSubmitting)
    return;

  isSubmitting = true;
  elements.errorMessage.hidden = true;
  elements.checkoutButton.textContent = 'Sender til PayNSync…';
  renderCart();

  try {
    const payload = buildDraftPayload({
      cart,
      orderId,
      merchantId,
      participantToken,
      merchantDraftReference: getMerchantDraftReference(),
      testPhoneNumber
    });

    const response = await fetch(`${API_BASE}/api/merchant-orders`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });

    if (!response.ok)
      throw new Error(await readApiError(response));

    const result = await response.json();
    if (result.paymentRedirectUrl) {
      window.location.assign(result.paymentRedirectUrl);
      return;
    }

    showSuccess(result.message || 'Din bestilling er gemt, og betalingen er reserveret.');
  } catch (error) {
    elements.errorMessage.textContent = friendlyError(error);
    elements.errorMessage.hidden = false;
    elements.errorMessage.scrollIntoView({ behavior: 'smooth', block: 'center' });
  } finally {
    isSubmitting = false;
    elements.checkoutButton.textContent = 'Fortsæt med PayNSync';
    renderCart();
  }
}

function showSuccess(message) {
  elements.shopLayout.hidden = true;
  elements.mobileCartButton.hidden = true;
  elements.successPanel.hidden = false;
  elements.successPanel.querySelector('p').textContent = message;
  elements.successPanel.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

function showToast(message) {
  elements.toast.textContent = message;
  elements.toast.classList.add('toast--visible');
  window.setTimeout(() => elements.toast.classList.remove('toast--visible'), 1800);
}

function getMerchantDraftReference() {
  const tokenSuffix = participantToken.slice(-12) || 'guest';
  const storageKey = `roma-draft-${orderId}-${tokenSuffix}`;

  try {
    const existing = sessionStorage.getItem(storageKey);
    if (existing)
      return existing;

    const id = globalThis.crypto?.randomUUID?.() ?? `${Date.now()}-${Math.random().toString(16).slice(2)}`;
    const reference = `ROMA-${orderId}-${id}`;
    sessionStorage.setItem(storageKey, reference);
    return reference;
  } catch {
    return `ROMA-${orderId}-${Date.now()}`;
  }
}

async function readApiError(response) {
  const text = await response.text();
  if (!text)
    return `PayNSync svarede med status ${response.status}.`;

  try {
    const body = JSON.parse(text);
    return body.message || body.title || `PayNSync svarede med status ${response.status}.`;
  } catch {
    return `PayNSync svarede med status ${response.status}.`;
  }
}

function friendlyError(error) {
  return error instanceof Error
    ? `Bestillingen kunne ikke sendes. ${error.message}`
    : 'Bestillingen kunne ikke sendes. Prøv igen.';
}

function parsePositiveInt(value) {
  const parsed = Number.parseInt(value ?? '', 10);
  return Number.isInteger(parsed) && parsed > 0 ? parsed : null;
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}
