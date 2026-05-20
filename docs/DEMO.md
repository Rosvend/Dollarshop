### Run the end-to-end checkout demo

Seeded demo data you'll need to reference:

| What | GUID |
| :-- | :-- |
| Demo customer | `22222222-2222-2222-2222-222222222201` |
| Wireless Mouse · $12.99 | `11111111-1111-1111-1111-111111111101` |
| USB-C Cable · $8.50 | `11111111-1111-1111-1111-111111111102` |
| Notebook A5 · $3.99 | `11111111-1111-1111-1111-111111111103` |

Use these GUIDs exactly — random product IDs are rejected with `404 ProductNotFoundException`.

```bash
# 1. Create a cart
CART=$(curl -s -X POST http://localhost:5003/carts \
  -H 'content-type: application/json' \
  -d '{ "customerId": "22222222-2222-2222-2222-222222222201" }' \
  | jq -r .cartId)
echo "Cart: $CART"

# 2. Add items
curl -s -X POST "http://localhost:5003/carts/$CART/items" \
  -H 'content-type: application/json' \
  -d '{
    "productId":   "11111111-1111-1111-1111-111111111101",
    "productName": "Wireless Mouse",
    "unitPrice":   12.99,
    "currency":    "USD",
    "quantity":    2
  }'

# 3. Inspect the cart (status: "Active", subtotal $25.98)
curl -s "http://localhost:5003/carts/$CART" | jq

# 4. Checkout — returns 202 Accepted, the Saga continues asynchronously
curl -i -X POST "http://localhost:5003/carts/$CART/checkout" \
  -H 'content-type: application/json' \
  -d '{ "paymentMethod": "card" }'

# 5. After ~2s the cart status flips to "Closed"
sleep 3
curl -s "http://localhost:5003/carts/$CART" | jq .status   # → "Closed"
```

### Observe what happened

```bash
# Follow the Saga across services
docker compose logs -f sales-service catalog-service finance-service

# Inspect cart state and the Outbox
docker compose exec postgres psql -U sales -d sales_db -c \
  "select status, customer_id from carts;"

docker compose exec postgres psql -U sales -d sales_db -c \
  "select message_type, processed_on is not null as sent
     from outbox_messages order by occurred_on;"

# Inspect seeded products
docker compose exec postgres psql -U sales -d catalog_db -c \
  "select sku, name, price_amount, stock_level from products;"
```

In the RabbitMQ UI (`:15672`) you'll see exchange `dollarshop.events` (topic, durable) bound to queues `finance.payment-requests` and `sales.payment-results`.


### Optional: rehearse the compensation path

The `finance-service` stub always approves. To see the compensating Saga (cart reverts, stock released):

In `src/finance-service/PaymentProcessor.cs`, change `Outcome: "AUTHORIZED"` to `Outcome: "DECLINED"`, then:

```bash
docker compose up -d --build finance-service
# run the demo flow again — cart status will become "Reverted"
```
