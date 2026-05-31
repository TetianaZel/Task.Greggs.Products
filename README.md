# Greggs.Products

[![CI](https://github.com/TetianaZel/Task.Greggs.Products/actions/workflows/ci.yml/badge.svg)](https://github.com/TetianaZel/Task.Greggs.Products/actions/workflows/ci.yml)

My solution to the Greggs take-home task. The original brief is preserved at the bottom of this file.

## Run it

```
dotnet test
dotnet run --project Greggs.Products.Api
# https://localhost:5001/swagger
# https://localhost:5001/product?pageSize=3&currency=EUR
```

## The endpoint

`GET /product?pageStart=0&pageSize=5&currency=GBP`

```
[
  { "name": "Sausage Roll",       "price": 1.11, "currency": "EUR" },
  { "name": "Vegan Sausage Roll", "price": 1.22, "currency": "EUR" }
]
```

Unknown currency or negative paging returns an RFC 7807 `application/problem+json` response.

## How it's wired

```
ProductController ──► IProductService ──► IDataAccess<Product>
                              │
                              └──► ICurrencyConverter ──► IOptions<CurrencyOptions>
```

- **Controller** — HTTP only, no try/catch. All error translation is owned by the middleware.
- **`IProductService`** — orchestrates fetch + convert + project to DTO. Throws `ValidationException` for bad input.
- **`ICurrencyConverter`** — `FixedRateCurrencyConverter` reads rates from `appsettings.json` (`Currency:ExchangeRates`). Decimal arithmetic, banker's rounding to 2dp.
- **`ProductDto`** — public contract, decoupled from the internal `Product` model.
- **`ExceptionHandlingMiddleware`** — centralised error handling. Maps `ValidationException` -> `400`, everything else -> `500`, both as `ProblemDetails`. Hides internal messages on `500`.
- **`Constants.ErrorMessages` / `Constants.Defaults`** — single source of truth for user-facing strings and default values; tests assert against the same constants.

Configuration:

```
"Currency": {
  "BaseCurrency": "GBP",
  "ExchangeRates": { "EUR": 1.11 }
}
```

## Projects

| Project | Target | Purpose |
|---|---|---|
| `Greggs.Products.Api` | net6.0 | The web API. |
| `Greggs.Products.UnitTests` | net6.0 | xUnit + Moq. Service, converter, controller, middleware. |
| `Greggs.Products.IntegrationTests` | net8.0 | `WebApplicationFactory` boots the real pipeline. |

`TestBase` in the integration project pins configuration via `AddInMemoryCollection`, so tests don't depend on whatever's in `appsettings.json`.

CI runs `restore` → `build` → `test` on every push and PR. Badge above reflects `main`.

## Things I chose not to do

These were considered and deliberately left out:

- **Live FX provider** — would drag in `HttpClient` lifetime, Polly, distributed caching, fallback behaviour, secrets. The `ICurrencyConverter` seam is there if it's ever needed.
- **MediatR / AutoMapper / CQRS** — pure ceremony at this scale.
- **API versioning, auth, output caching, health checks** — out of scope; trivial to add behind the existing seams.
- **Upgrade to .NET 8 LTS** — out of scope of the user stories. Would warrant its own PR (and I'd audit the test SDK versions while at it).

## Known limitations

- The GBP->EUR rate is **stale by design** — config-driven, no audit trail beyond Git history. Fine for the brief, wrong for live e-commerce.
- `IOptions<T>` is snapshot-at-startup; rate changes need a restart. Swap for `IOptionsMonitor<T>` if that matters.

## If I had another hour

1. Response caching on `/product` keyed by `pageStart|pageSize|currency`.
2. A live FX adapter behind `ICurrencyConverter` with Polly retry + `IDistributedCache`.

---
---

# Original brief

## Introduction
Hello and welcome to the Greggs Products repository, thanks for finding it!

## The Solution
So at the moment the api is currently returning a random selection from a fixed set of Greggs products directly 
from the controller itself. We currently have a data access class and it's interface but 
it's not plugged in (please ignore the class itself, we're pretending it hits a database),
we're also going to pretend that the data access functionality is fully tested so we don't need 
to worry about testing those lines of functionality.

We're mainly looking for the way you work, your code structure and how you would approach tackling the following 
scenarios.

## User Stories
Our product owners have asked us to implement the following stories, we'd like you to have 
a go at implementing them. You can use whatever patterns you're used to using or even better 
whatever patterns you would like to use to achieve the goal. Anyhow, back to the 
user stories:

### User Story 1
**As a** Greggs Fanatic<br/>
**I want to** be able to get the latest menu of products rather than the random static products it returns now<br/>
**So that** I get the most recently available products.

**Acceptance Criteria**<br/>
**Given** a previously implemented data access layer<br/>
**When** I hit a specified endpoint to get a list of products<br/>
**Then** a list or products is returned that uses the data access implementation rather than the static list it current utilises

### User Story 2
**As a** Greggs Entrepreneur<br/>
**I want to** get the price of the products returned to me in Euros<br/>
**So that** I can set up a shop in Europe as part of our expansion

**Acceptance Criteria**<br/>
**Given** an exchange rate of 1GBP to 1.11EUR<br/>
**When** I hit a specified endpoint to get a list of products<br/>
**Then** I will get the products and their price(s) returned
