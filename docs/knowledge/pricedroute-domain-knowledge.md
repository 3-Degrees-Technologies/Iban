# PricedRoute Domain Knowledge

## Overview

**PricedRoute** is a core value object in Centro's Quote domain that represents a **specific quote from a single FX provider** for a given **corridor**. It's the normalized representation of what any provider (Wise, Nium, CurrencyCloud, etc.) offers for a particular country-to-country currency exchange.

## What It Represents

Think of PricedRoute as a **"quote slip"** from an FX provider - it contains everything you need to know about:
- **The Corridor**: Source country/currency to target country/currency
- **The Deal**: Exchange rate, amounts, fees
- **The Service**: Delivery time, expiration
- **The Provider**: Who's offering it and their unique data

```csharp
var wiseRoute = new PricedRoute(
    providerId: "wise",
    providerName: "Wise", 
    exchangeRate: 1.2750m,        // 1 USD = 1.2750 EUR
    sourceAmount: 1000m,          // Send $1,000 USD
    targetAmount: 1275m,          // Receive €1,275 EUR  
    sourceCountry: "US",          // From United States
    sourceCurrency: "USD",
    targetCountry: "DE",          // To Germany
    targetCurrency: "EUR",
    fee: 8.50m,                   // $8.50 fee
    feeCurrency: "USD",
    estimatedDeliveryTime: TimeSpan.FromHours(2),
    expiresAt: DateTime.UtcNow.AddMinutes(30)
);
```

## Corridors: Country + Currency Pairs

**CRITICAL**: Centro models corridors as **country-to-country + currency-to-currency** combinations, not just currency pairs. This enables intelligent provider routing based on the complete corridor context.

### Why Countries Matter
- **Different costs**: `US-USD/DE-EUR` vs `CA-USD/DE-EUR` have different pricing
- **Different rails**: Payment networks vary by country (SEPA vs SWIFT vs local rails)
- **Different regulations**: Each country pair has unique compliance requirements
- **Different speeds**: Cross-border rails affect delivery times

### Corridor Examples
```csharp
// Same currency pair, different corridors
"US-USD/DE-EUR"    // United States to Germany (domestic USD, SEPA EUR)
"CA-USD/DE-EUR"    // Canada to Germany (international USD, SEPA EUR)  
"GB-USD/DE-EUR"    // UK to Germany (international USD, SEPA EUR)

// Different currency pairs, same countries
"US-USD/GB-GBP"    // United States to United Kingdom
"US-EUR/GB-GBP"    // United States (EUR account) to United Kingdom

// Complex routing scenarios
"SG-SGD/US-USD"    // Singapore to United States (SWIFT international)
"HK-HKD/CN-CNY"    // Hong Kong to China (special corridor, unique regulations)
```

## When You Use It

### 1. Quote Collection Phase
When Centro requests quotes from multiple providers for a corridor:

```csharp
// QuoteRequest now includes full corridor definition
var quoteRequest = new QuoteRequest(
    tenantId: "tenant-123", 
    sourceCountry: "US",
    sourceCurrency: "USD", 
    targetCountry: "DE",
    targetCurrency: "EUR", 
    amount: 1000m
);

// Each provider adapter creates a corridor-aware PricedRoute
var wiseRoute = wiseAdapter.GetQuote(quoteRequest);    // Returns PricedRoute with corridor
var niumRoute = niumAdapter.GetQuote(quoteRequest);    // Returns PricedRoute with corridor
var ccRoute = currencyCloudAdapter.GetQuote(quoteRequest); // Returns PricedRoute with corridor

// Add them to the request
quoteRequest.AddPricedRoute(wiseRoute);
quoteRequest.AddPricedRoute(niumRoute);
quoteRequest.AddPricedRoute(ccRoute);
```

### 2. Route Comparison & Selection
When clients need to compare options and pick the best:

```csharp
// Compare all available routes
var routes = quoteRequest.PricedRoutes;

var cheapestRoute = routes.OrderBy(r => r.GetTotalCost()).First();
var fastestRoute = routes.OrderBy(r => r.EstimatedDeliveryTime).First();
var bestRateRoute = routes.OrderByDescending(r => r.GetEffectiveRate()).First();

// Client selects their preferred route
quoteRequest.SelectRoute(cheapestRoute.RouteId);
```

### 3. Provider-Agnostic Processing
When Infrastructure layer needs to work with any provider:

```csharp
public async Task ExecuteTransfer(PricedRoute selectedRoute)
{
    // Same interface works for any provider
    var adapter = providerFactory.GetAdapter(selectedRoute.ProviderId);
    
    // Provider-specific data available when needed
    var wiseQuoteId = selectedRoute.ProviderSpecificData["wise_quote_id"];
    var niumRateLockId = selectedRoute.ProviderSpecificData["rate_lock_id"];
    
    return await adapter.ExecuteTransfer(selectedRoute);
}
```

## Key Design Benefits

### 1. Provider Normalization 
Different providers structure their APIs differently, but PricedRoute gives us a common format:

**Wise API Response**:
```json
{
  "id": "quote-123",
  "rate": 1.2750,
  "fee": 8.50,
  "deliveryEstimate": "2024-01-15T14:30:00Z"
}
```

**Nium API Response**:
```json
{
  "rateLockId": "nium-456", 
  "exchangeRate": 1.2745,
  "processingFee": 12.00,
  "estimatedTime": "PT2H30M"
}
```

**Both become PricedRoute** - same interface, different ProviderSpecificData.

### 2. Smart Calculations
Built-in business logic for real comparisons:

```csharp
// Total cost including fees (when fee is in source currency)
var totalCost = route.GetTotalCost();  // $1,000 + $8.50 = $1,008.50

// Effective rate including fee impact  
var effectiveRate = route.GetEffectiveRate();  // €1,275 ÷ $1,008.50 = 1.2641

// Real-time expiration checking
var isStillValid = !route.IsExpired();
```

### 3. Provider Flexibility
The `ProviderSpecificData` dictionary handles unique provider requirements:

```csharp
// Wise needs customerTransactionId for idempotency
wiseRoute.ProviderSpecificData["customerTransactionId"] = "cust-txn-789";
wiseRoute.ProviderSpecificData["wise_quote_id"] = "quote-123";

// Nium needs rate lock tracking
niumRoute.ProviderSpecificData["rate_lock_id"] = "nium-456";
niumRoute.ProviderSpecificData["off_market_rates"] = true;

// CurrencyCloud needs conversion tracking
ccRoute.ProviderSpecificData["conversion_id"] = "conv-789";
ccRoute.ProviderSpecificData["settlement_date"] = "2024-01-15";
```

## Business Value for Centro

### "Unbundling" FX Aggregators
Instead of accepting whatever rates/fees a bundled aggregator offers, Centro can:
- ✅ **Compare apples-to-apples** across providers using normalized PricedRoute
- ✅ **Route intelligently** based on cost, speed, or reliability per corridor
- ✅ **Add new providers** without changing core business logic
- ✅ **Optimize per client** based on their priorities (cost vs speed)

### Real-World Example
```
Client wants: $10,000 US-USD → GB-GBP (urgent)

Wise PricedRoute:     £7,845 (2 hours, £45 fee) - Uses domestic USD + SWIFT to UK
Nium PricedRoute:     £7,820 (4 hours, £25 fee) - Uses correspondent banking
CurrencyCloud Route:  £7,860 (1 hour, £65 fee) - Uses direct UK banking rails

Centro Logic: Client marked "urgent" → select CurrencyCloud (fastest)
Centro Value: Better outcome than any single aggregator could provide
```

Contrast with: $10,000 CA-USD → GB-GBP (different corridor)
```
Same currency pair, different corridor = different provider strengths:

Wise PricedRoute:     £7,820 (3 hours, £55 fee) - International USD routing slower
Nium PricedRoute:     £7,845 (2 hours, £35 fee) - Strong CA-USD corridor presence  
CurrencyCloud Route:  £7,835 (4 hours, £45 fee) - Less optimized for CA origination

Centro Logic: Nium wins for CA-USD/GB-GBP corridor
Centro Value: Corridor-specific provider optimization
```

## Provider-Specific Integration Patterns

### Wise Integration
```csharp
// Wise API call (simplified - they handle country routing internally)
var wiseQuote = await wiseClient.CreateQuote(new WiseQuoteRequest
{
    SourceCurrency = "USD",
    TargetCurrency = "EUR", 
    SourceAmount = 1000m,
    CustomerTransactionId = Guid.NewGuid().ToString()
});

// Convert to corridor-aware PricedRoute
var pricedRoute = new PricedRoute(
    providerId: "wise",
    providerName: "Wise",
    exchangeRate: wiseQuote.Rate,
    sourceAmount: wiseQuote.SourceAmount,
    targetAmount: wiseQuote.TargetAmount,
    sourceCountry: quoteRequest.SourceCountry,  // From original request
    sourceCurrency: wiseQuote.SourceCurrency,
    targetCountry: quoteRequest.TargetCountry,  // From original request
    targetCurrency: wiseQuote.TargetCurrency,
    fee: wiseQuote.Fee,
    feeCurrency: wiseQuote.FeeCurrency,
    estimatedDeliveryTime: TimeSpan.FromMinutes(wiseQuote.EstimatedDeliveryMinutes),
    expiresAt: wiseQuote.ExpiresAt,
    providerSpecificData: new Dictionary<string, object>
    {
        ["wise_quote_id"] = wiseQuote.Id,
        ["customer_transaction_id"] = wiseQuote.CustomerTransactionId,
        ["delivery_estimate"] = wiseQuote.DeliveryEstimate,
        ["corridor"] = $"{quoteRequest.SourceCountry}-{quoteRequest.SourceCurrency}/{quoteRequest.TargetCountry}-{quoteRequest.TargetCurrency}"
    }
);
```

### Nium Integration
```csharp
// Nium API call (corridor-aware)
var niumQuote = await niumClient.GetExchangeRate(new NiumRateRequest
{
    SourceCountry = quoteRequest.SourceCountry,    // Nium is corridor-aware
    SourceCurrency = "USD",
    DestinationCountry = quoteRequest.TargetCountry,
    DestinationCurrency = "EUR",
    Amount = 1000m,
    RateLockPeriod = "PT30M"
});

// Convert to corridor-aware PricedRoute
var pricedRoute = new PricedRoute(
    providerId: "nium",
    providerName: "Nium", 
    exchangeRate: niumQuote.ExchangeRate,
    sourceAmount: niumQuote.SourceAmount,
    targetAmount: niumQuote.DestinationAmount,
    sourceCountry: niumQuote.SourceCountry,
    sourceCurrency: niumQuote.SourceCurrency,
    targetCountry: niumQuote.DestinationCountry,
    targetCurrency: niumQuote.DestinationCurrency,
    fee: niumQuote.ProcessingFee,
    feeCurrency: niumQuote.FeeCurrency,
    estimatedDeliveryTime: TimeSpan.Parse(niumQuote.EstimatedProcessingTime),
    expiresAt: DateTime.UtcNow.Add(TimeSpan.Parse(niumQuote.RateLockPeriod)),
    providerSpecificData: new Dictionary<string, object>
    {
        ["rate_lock_id"] = niumQuote.RateLockId,
        ["rate_lock_period"] = niumQuote.RateLockPeriod,
        ["off_market_rates"] = niumQuote.OffMarketRates,
        ["processing_fee_breakdown"] = niumQuote.FeeBreakdown,
        ["corridor_type"] = niumQuote.CorridorType,  // e.g., "domestic", "regional", "international"
        ["payment_rails"] = niumQuote.PaymentRails   // e.g., "swift", "sepa", "local"
    }
);
```

### CurrencyCloud Integration
```csharp
// CurrencyCloud API - two-step process
var ccRate = await ccClient.GetDetailedRates(new CCRateRequest
{
    BuyCurrency = "EUR",
    SellCurrency = "USD",
    Amount = 1000m,
    FixedSide = "sell"
});

// Convert to PricedRoute
var pricedRoute = new PricedRoute(
    providerId: "currencycloud",
    providerName: "CurrencyCloud",
    exchangeRate: ccRate.ClientRate,
    sourceAmount: ccRate.ClientSellAmount,
    targetAmount: ccRate.ClientBuyAmount,
    sourceCurrency: ccRate.SellCurrency,
    targetCurrency: ccRate.BuyCurrency,
    fee: ccRate.Fee,
    feeCurrency: ccRate.FeeCurrency,
    estimatedDeliveryTime: TimeSpan.FromHours(ccRate.EstimatedDeliveryHours),
    expiresAt: DateTime.UtcNow.AddMinutes(ccRate.QuoteValidityMinutes),
    providerSpecificData: new Dictionary<string, object>
    {
        ["rate_id"] = ccRate.Id,
        ["mid_market_rate"] = ccRate.MidMarketRate,
        ["margin_rate"] = ccRate.MarginRate,
        ["settlement_date"] = ccRate.SettlementDate,
        ["fixed_side"] = ccRate.FixedSide
    }
);
```

## Common Patterns

### Route Optimization Logic
```csharp
public static class RouteOptimizer
{
    public static PricedRoute SelectOptimalRoute(
        IEnumerable<PricedRoute> routes, 
        OptimizationCriteria criteria,
        string corridor)
    {
        // Filter to matching corridor first
        var corridorRoutes = routes.Where(r => r.GetCorridor() == corridor);
        
        return criteria switch
        {
            OptimizationCriteria.LowestCost => 
                corridorRoutes.OrderBy(r => r.GetTotalCost()).First(),
                
            OptimizationCriteria.FastestDelivery => 
                corridorRoutes.OrderBy(r => r.EstimatedDeliveryTime).First(),
                
            OptimizationCriteria.BestRate => 
                corridorRoutes.OrderByDescending(r => r.GetEffectiveRate()).First(),
                
            OptimizationCriteria.Balanced => 
                corridorRoutes.OrderBy(r => CalculateCorridorScore(r)).First(),
                
            _ => throw new ArgumentException("Unknown optimization criteria")
        };
    }

    private static decimal CalculateCorridorScore(PricedRoute route)
    {
        // Corridor-specific scoring weights
        var corridorWeights = GetCorridorWeights(route.GetCorridor());
        
        var costScore = route.GetTotalCost();
        var speedScore = (decimal)route.EstimatedDeliveryTime.TotalHours * 100;
        
        return (costScore * corridorWeights.CostWeight) + 
               (speedScore * corridorWeights.SpeedWeight);
    }
    
    private static CorridorWeights GetCorridorWeights(string corridor)
    {
        return corridor switch
        {
            // High-volume corridors favor cost
            var c when c.StartsWith("US-USD/") => new(0.7m, 0.3m),
            
            // Emerging market corridors favor speed
            var c when c.Contains("/CN-CNY") => new(0.4m, 0.6m),
            
            // Default balanced
            _ => new(0.6m, 0.4m)
        };
    }
}

public record CorridorWeights(decimal CostWeight, decimal SpeedWeight);
```

### Expiration Management
```csharp
public static class RouteValidator
{
    public static IEnumerable<PricedRoute> GetValidRoutes(IEnumerable<PricedRoute> routes)
    {
        return routes.Where(r => !r.IsExpired()).ToList();
    }
    
    public static TimeSpan GetTimeToExpiration(PricedRoute route)
    {
        var timeLeft = route.ExpiresAt - DateTime.UtcNow;
        return timeLeft > TimeSpan.Zero ? timeLeft : TimeSpan.Zero;
    }
}
```

## Architecture Impact

**PricedRoute** is the foundation that makes Centro's corridor optimization possible - it transforms diverse provider APIs into comparable, actionable quotes that drive intelligent routing decisions.

This value object enables:
- **Provider Independence**: Add/remove providers without core logic changes
- **Intelligent Routing**: Compare routes using business-relevant metrics
- **Client Optimization**: Route based on client priorities and preferences
- **Audit Trails**: Track provider quotes for compliance and analytics
- **Scalable Architecture**: Clean separation between domain logic and provider implementations

## Related Documentation

- [Quote Domain README](../../src/Centro.Core/Quote/README.md) - Complete Quote domain model documentation
- [Provider API Navigation](provider-api-navigation.md) - Guide to integrating with FX provider APIs
- [Architecture Overview](../architecture/api-structure.md) - System architecture and design patterns