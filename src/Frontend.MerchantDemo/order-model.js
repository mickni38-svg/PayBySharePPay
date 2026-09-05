export const catalog = [
  {
    id: 'pizza',
    name: 'Pizza',
    description: 'Håndstrakt bund, San Marzano-tomat og mozzarella',
    products: [
      product('roma-pizza-margherita', 'Margherita', 'Tomat, fior di latte og frisk basilikum', 89, '🍕', ['Vegetarisk'], pizzaModifiers('margherita')),
      product('roma-pizza-pepperoni', 'Pepperoni', 'Tomat, mozzarella og italiensk pepperoni', 105, '🍕', ['Populær'], pizzaModifiers('pepperoni')),
      product('roma-pizza-prosciutto', 'Prosciutto', 'Tomat, mozzarella, skinke og champignon', 109, '🍄', [], pizzaModifiers('prosciutto')),
      product('roma-pizza-verdure', 'Verdure', 'Grillede grøntsager, pesto og mozzarella', 105, '🥬', ['Vegetarisk'], pizzaModifiers('verdure')),
      product('roma-pizza-diavola', 'Diavola', 'Stærk salami, chili, rødløg og mozzarella', 115, '🌶️', ['Stærk'], pizzaModifiers('diavola'))
    ]
  },
  {
    id: 'pasta',
    name: 'Pasta & salater',
    description: 'Frisklavet i køkkenet',
    products: [
      product('roma-pasta-carbonara', 'Spaghetti Carbonara', 'Guanciale, æg, pecorino og sort peber', 119, '🍝', ['Populær'], standardModifiers('carbonara')),
      product('roma-pasta-arrabbiata', 'Penne Arrabbiata', 'Tomat, hvidløg, chili og persille', 105, '🍝', ['Vegetarisk', 'Stærk'], standardModifiers('arrabbiata')),
      product('roma-salad-caesar', 'Caesar salat', 'Kylling, romaine, parmesan, croutoner og dressing', 109, '🥗', [], [
        modifier('caesar-extra-chicken', 'Ekstra kylling', 25),
        modifier('caesar-no-croutons', 'Uden croutoner', 0),
        modifier('caesar-dressing-side', 'Dressing ved siden af', 0)
      ])
    ]
  },
  {
    id: 'sides',
    name: 'Tilbehør',
    description: 'Lidt ekstra til bordet',
    products: [
      product('roma-side-garlic-bread', 'Hvidløgsbrød', 'Sprødt brød med hvidløgssmør og urter', 45, '🥖', [], [modifier('garlic-bread-cheese', 'Mozzarella', 12)]),
      product('roma-side-fries', 'Rosmarinfritter', 'Sprøde fritter med rosmarin og havsalt', 42, '🍟', [], [modifier('fries-aioli', 'Trøffel-aioli', 10)])
    ]
  },
  {
    id: 'drinks',
    name: 'Drikkevarer',
    description: 'Kolde drikke',
    products: [
      product('roma-drink-cola', 'Cola 50 cl', 'Vælg almindelig eller uden sukker', 29, '🥤', [], [modifier('cola-zero', 'Uden sukker', 0)]),
      product('roma-drink-water', 'San Pellegrino 50 cl', 'Italiensk mineralvand med brus', 32, '💧'),
      product('roma-drink-beer', 'Peroni 33 cl', 'Italiensk pilsner, 4,7 %', 45, '🍺')
    ]
  },
  {
    id: 'dessert',
    name: 'Dessert',
    description: 'En sød afslutning',
    products: [
      product('roma-dessert-tiramisu', 'Tiramisu', 'Mascarpone, espresso og kakao', 59, '🍰', ['Husets']),
      product('roma-dessert-panna-cotta', 'Panna cotta', 'Vanilje, citron og bærkompot', 55, '🍮')
    ]
  }
];

function product(id, name, description, price, emoji, badges = [], modifiers = []) {
  return { id, name, description, price, emoji, badges, modifiers };
}

function modifier(id, name, price) {
  return { id: `roma-option-${id}`, name, price };
}

function pizzaModifiers(prefix) {
  return [
    modifier(`${prefix}-extra-cheese`, 'Ekstra mozzarella', 15),
    modifier(`${prefix}-chili`, 'Frisk chili', 5),
    modifier(`${prefix}-gluten-free`, 'Glutenfri bund', 20)
  ];
}

function standardModifiers(prefix) {
  return [
    modifier(`${prefix}-parmesan`, 'Ekstra parmesan', 12),
    modifier(`${prefix}-chili`, 'Frisk chili', 5)
  ];
}

export function getAllProducts() {
  return catalog.flatMap(category => category.products);
}

export function findProduct(productId) {
  return getAllProducts().find(productItem => productItem.id === productId) ?? null;
}

export function createCartLine(productItem, selectedModifierIds = [], quantity = 1, lineInstanceId = createLineInstanceId()) {
  if (!productItem)
    throw new Error('Produktet findes ikke.');

  if (!Number.isInteger(quantity) || quantity < 1 || quantity > 20)
    throw new Error('Antal skal være mellem 1 og 20.');

  const selectedIds = new Set(selectedModifierIds);
  const selectedModifiers = productItem.modifiers.filter(item => selectedIds.has(item.id));

  if (selectedModifiers.length !== selectedIds.size)
    throw new Error('Et valgt tilvalg findes ikke på produktet.');

  const unitPrice = roundMoney(productItem.price + selectedModifiers.reduce((sum, item) => sum + item.price, 0));
  const name = selectedModifiers.length === 0
    ? productItem.name
    : `${productItem.name} · ${selectedModifiers.map(item => item.name).join(', ')}`;

  return {
    lineInstanceId,
    productId: productItem.id,
    productName: productItem.name,
    name,
    emoji: productItem.emoji,
    quantity,
    baseUnitPrice: productItem.price,
    modifiers: selectedModifiers.map(item => ({ ...item })),
    unitPrice,
    lineTotal: roundMoney(unitPrice * quantity)
  };
}

export function withLineQuantity(cart, lineInstanceId, quantity) {
  if (quantity <= 0)
    return cart.filter(line => line.lineInstanceId !== lineInstanceId);

  if (!Number.isInteger(quantity) || quantity > 20)
    throw new Error('Antal skal være mellem 0 og 20.');

  return cart.map(line => line.lineInstanceId === lineInstanceId
    ? { ...line, quantity, lineTotal: roundMoney(line.unitPrice * quantity) }
    : line);
}

export function cartTotal(cart) {
  return roundMoney(cart.reduce((sum, line) => sum + line.lineTotal, 0));
}

export function buildDraftPayload({
  cart,
  orderId,
  merchantId,
  participantToken,
  merchantDraftReference,
  testPhoneNumber,
  now = new Date()
}) {
  if (!Number.isInteger(orderId) || orderId <= 0)
    throw new Error('Der mangler et gyldigt orderId.');
  if (!participantToken)
    throw new Error('Der mangler et participantToken.');
  if (!merchantDraftReference)
    throw new Error('Der mangler en merchant draft-reference.');
  if (!Array.isArray(cart) || cart.length === 0)
    throw new Error('Kurven er tom.');

  const total = cartTotal(cart);
  const expiresAtUtc = new Date(now.getTime() + 24 * 60 * 60 * 1000).toISOString();
  const rawOrder = {
    schemaVersion: 'roma-demo-v1',
    merchantDraftReference,
    items: cart.map(line => ({
      lineInstanceId: line.lineInstanceId,
      productId: line.productId,
      productName: line.productName,
      quantity: line.quantity,
      baseUnitPrice: line.baseUnitPrice,
      modifiers: line.modifiers.map(item => ({ id: item.id, name: item.name, price: item.price })),
      unitPrice: line.unitPrice,
      lineTotal: line.lineTotal
    }))
  };

  const payload = {
    orderId,
    merchantParticipantId: Number.isInteger(merchantId) && merchantId > 0 ? merchantId : null,
    participantToken,
    merchantDraftReference,
    subtotalAmount: total,
    totalAmount: total,
    currency: 'DKK',
    paymentMode: 'AuthorizeThenCapture',
    expiresAtUtc,
    lines: cart.map(line => ({
      lineId: line.productId,
      name: line.name,
      quantity: line.quantity,
      unitPrice: line.unitPrice,
      lineTotal: line.lineTotal
    })),
    rawMerchantPayloadJson: JSON.stringify(rawOrder)
  };

  if (testPhoneNumber)
    payload.testPhoneNumber = testPhoneNumber;

  return payload;
}

export function validateCatalog() {
  const categoryIds = catalog.map(category => category.id);
  const products = getAllProducts();
  const productIds = products.map(productItem => productItem.id);
  const modifierIds = products.flatMap(productItem => productItem.modifiers.map(item => item.id));

  return {
    categoryIdsUnique: uniqueAndFilled(categoryIds),
    productIdsUnique: uniqueAndFilled(productIds),
    modifierIdsUnique: uniqueAndFilled(modifierIds)
  };
}

function uniqueAndFilled(values) {
  return values.every(Boolean) && new Set(values).size === values.length;
}

function roundMoney(value) {
  return Math.round((value + Number.EPSILON) * 100) / 100;
}

function createLineInstanceId() {
  if (globalThis.crypto?.randomUUID)
    return globalThis.crypto.randomUUID();

  return `line-${Date.now()}-${Math.random().toString(16).slice(2)}`;
}
