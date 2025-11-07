# Provider API Navigation Guide

This document serves as a quick reference guide to the key API endpoints for each of our FX providers. It is a living document that should be updated as we explore new parts of their APIs.

---

## Wise

*   **Official Docs:** [docs.wise.com/api-docs](https://docs.wise.com/api-docs)

### Key Processes

1.  **Quoting:**
    *   **Endpoint:** `POST /v3/quotes`
    *   **Documentation:** [docs.wise.com/api-docs/api-reference/quote](https://docs.wise.com/api-docs/api-reference/quote)
    *   **Notes:** Provides a bundled quote with a fixed 30-minute expiration.

2.  **Payment Execution (Transfer):**
    *   **Endpoint:** `POST /v1/transfers`
    *   **Documentation:** [docs.wise.com/api-docs/api-reference/transfer](https://docs.wise.com/api-docs/api-reference/transfer)
    *   **Notes:** Requires a `quoteUuid` from the quoting step. Uses a `customerTransactionId` for idempotency. Has a multi-step lifecycle including a separate funding step.

---

## Nium

*   **Official Docs:** [docs.nium.com](https://docs.nium.com)

### Key Processes

1.  **Quoting (FX):**
    *   **Endpoint:** `POST /api/v1/client/{clientHashId}/exchangeRate/lock` (This is an assumption based on the "Exchange Rate Lock And Hold" language. We need to find the exact endpoint.)
    *   **Documentation:** [docs.nium.com/docs/fx](https://docs.nium.com/docs/fx)
    *   **Notes:** Supports variable lock periods (5m to 24h). Introduces the concept of `isOffMarket` rates.

2.  **Payment Execution (Payout):**
    *   **Endpoint:** (Likely a "Transfer Money" or similar endpoint, to be confirmed)
    *   **Documentation:** [docs.nium.com/docs/payouts](https://docs.nium.com/docs/payouts)
    *   **Notes:** A real-time process. Supports multiple `payoutMethod` types (`CARD`, `LOCAL`, etc.). The high-level documentation does not emphasize a client-side idempotency key in the same way as Wise.

---

## CurrencyCloud

*   **Official Docs:** [developer.currencycloud.com](https://developer.currencycloud.com)

### Key Processes

1.  **Quoting (Two-Step Process):**
    *   **Step 1: Get FX Rate:** `GET /v2/rates/detailed`
    *   **Step 2: Get Payment Fee:** `GET /v2/payments/quote/fee`
    *   **Documentation:** [developer.currencycloud.com/api-reference](https://developer.currencycloud.com/api-reference)
    *   **Notes:** The FX quote and the payment fee are retrieved in two separate API calls. Our adapter is responsible for combining these into a single `PricedRoute`.

2.  **Payment Execution (Two-Step Process):**
    *   **Step 1: Convert Funds:** `POST /v2/conversions/create`
    *   **Step 2: Create Payment:** `POST /v2/payments/create`
    *   **Documentation:** [developer.currencycloud.com/api-reference](https://developer.currencycloud.com/api-reference)
    *   **Notes:** The `Create Payment` call requires a `conversion_id` from the first step. This reinforces the need for our `PricedRoute` to hold provider-specific data.
