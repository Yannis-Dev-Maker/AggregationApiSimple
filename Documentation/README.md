# 🌐 Agile API Aggregator

Agile Aggregator is a unified .NET 6 web API that consolidates data from multiple public APIs (NewsAPI, Spotify, OpenWeatherMap) into a single endpoint. It includes a user-friendly frontend hosted separately and a Kestrel-powered backend with HTTPS support.

---

## 📐 Architecture

```
                       ┌──────────────────────┐
                       │     User Browser     │
                       └────────┬─────────────┘
                                │
                       https://www.pc-soft.gr/api-calls
                                │
               ┌──────────────────────────────────────┐
               │              Frontend                │
               │   - index.html (HTML/CSS/JS)         │
               │   - Calls /api/aggregation endpoint  │
               └──────────────────────────────────────┘
                                │
                                ▼
       https://api.pc-soft.gr:5001/api/aggregation?...
                                │
           ┌───────────────────────────────────────┐
           │               Backend                 │
           │      ASP.NET Core (Kestrel)           │
           │                                       │
           │  AggregationController                │
           │    ├── NewsService                    │
           │    ├── SpotifyService                 │
           │    └── WeatherService                 │
           └───────────────────────────────────────┘
```

---

## 🧩 Components

### ✅ API Endpoint: `/api/aggregation`

**Query Parameters:**

- `keyword` (default: "Athens")
- `count` (default: 5)
- `page` (default: 1)
- `sortBy` (default: "publishedAt") — *affects news only*
- `fromDate` (optional, format yyyy-MM-dd) — *affects news only*
- `services` (default: "news,spotify,weather")

### ✅ Services

- **NewsService**: Uses NewsAPI
- **SpotifyService**: Auth with Client Credentials flow
- **WeatherService**: Uses OpenWeatherMap

Each returns:
```csharp
public class AggregatedResult {
  public string Title;
  public string Description;
  public string Url;
  public string? ImageUrl;
  public DateTime Date;
}
```

---

## ⚙️ Deployment

- Backend: Windows Server 2019, .NET 6 Kestrel
- Ports forwarded: 5000 (HTTP), 5001 (HTTPS)
- Public Domain: `api.pc-soft.gr`
- Frontend Hosted at: [https://www.pc-soft.gr/api-calls](https://www.pc-soft.gr/api-calls)
- Certificate: Let's Encrypt (Certify The Web)

---

## 🧪 Unit Tests

Implemented using **xUnit** + **Moq** under `AgileActors.Tests`:

- `NewsServiceTests`
- `SpotifyServiceTests`
- `WeatherServiceTests`
- `AggregationControllerTests`

---

## ✅ Features

| Feature                                      | Status  |
|---------------------------------------------|---------|
| Fetch from 3 external APIs                  | ✅      |
| Aggregated unified endpoint                 | ✅      |
| Parameterized filtering + sorting           | ✅ (news only) |
| Error handling & fallback                   | ✅      |
| HTTPS via Let's Encrypt                     | ✅      |
| UI with filters, sort, date, pagination     | ✅      |
| Parallel API calls                          | ✅      |
| Unit tests (services + controller)          | ✅      |
| Hosted frontend + backend separately        | ✅      |

---

## 🧠 Future Improvements

- ❌ Caching (deliberately skipped)
- ❌ Request statistics & performance buckets

---

## 🙌 Credits

- **Developed by:** Yannis Thanassekos
- **Hosting & DNS:** HyperHosting
- **External APIs:** NewsAPI.org, OpenWeatherMap, Spotify Web API
- **Certificate:** Let's Encrypt via CertifyTheWeb

---
