# Strong Password Generator API

ASP.NET Core REST API for generating strong passwords using three distinct generation strategies, designed for varying security, compliance, and usability needs.

Written with assistance for lab/internship reference.

---

## Table of Contents

- [Overview](#overview)
- [Goals and Non-Goals](#goals-and-non-goals)
- [API Reference](#api-reference)
  - [Base URL](#base-url)
  - [Character Sets](#character-sets)
  - [Policy Options](#policy-options)
  - [Endpoints](#endpoints)
- [Generation Methods](#generation-methods)
- [Validation Rules](#validation-rules)
- [Error Handling](#error-handling)
- [Security Requirements](#security-requirements)
- [Project Structure](#project-structure)
- [Configuration](#configuration)
- [Getting Started](#getting-started)

---

## Overview

This service exposes a REST API to generate strong passwords using three different generation methods. Each method targets a different use case — from human-friendly passphrases to high-entropy machine secrets.

## Goals and Non-Goals

### Goals

- Generate passwords with configurable length and character rules
- Offer 3 distinct generation strategies
- Provide basic strength metadata (entropy estimate, charset flags, policy compliance)

### Non-Goals

- Storing generated passwords
- User authentication or identity management
- Password breach checking (can be added later)

---

## API Reference

### Base URL

```
/api/v1
```

- Content-Type: `application/json`
- All endpoints are idempotent only in terms of output format — each call returns new random results

### Character Sets

| Name      | Characters                                       |
|-----------|--------------------------------------------------|
| `lower`   | `a-z`                                            |
| `upper`   | `A-Z`                                            |
| `digits`  | `0-9`                                            |
| `symbols` | `!@#$%^&*()-_=+[]{};:,.<>?` (configurable in appsettings) |

### Policy Options

These fields are accepted in the request body:

| Field              | Type   | Default | Description                                                                 |
|--------------------|--------|---------|-----------------------------------------------------------------------------|
| `length`           | int    | 16      | Total password length (min 8, max 256)                                      |
| `includeLower`     | bool   | true    | Include lowercase letters                                                   |
| `includeUpper`     | bool   | true    | Include uppercase letters                                                   |
| `includeDigits`    | bool   | true    | Include digits                                                              |
| `includeSymbols`   | bool   | true    | Include symbols                                                             |
| `excludeSimilar`   | bool   | true    | Exclude visually similar characters (`O 0 I l 1 \|`)                       |
| `excludeAmbiguous` | bool   | false   | Exclude ambiguous characters (`{ } [ ] ( ) / \ ' " ~ , ; : . < >`)        |
| `requiredSets`     | int    | 3       | Minimum number of enabled categories that must appear at least once          |
| `count`            | int    | 1       | Number of passwords to return (max 50)                                      |

### Response Metadata

Each generated password includes:

| Field             | Type   | Description                                                               |
|-------------------|--------|---------------------------------------------------------------------------|
| `entropyBits`     | double | Estimated entropy: `log2(charsetSize ^ length)`, adjusted for constraints |
| `policySatisfied` | bool   | Whether all policy rules are met                                          |
| `composition`     | object | Counts of each character set in the password                              |

---

### Endpoints

#### 1. Generate Passwords

```
POST /api/v1/passwords/generate
```

**Request Body:**

```json
{
  "method": "policy",
  "length": 20,
  "count": 3,
  "includeLower": true,
  "includeUpper": true,
  "includeDigits": true,
  "includeSymbols": true,
  "excludeSimilar": true,
  "excludeAmbiguous": false,
  "requiredSets": 4,

  "passphrase": {
    "wordCount": 4,
    "separator": "-",
    "capitalizeMode": "first",
    "appendNumber": true,
    "appendSymbol": true
  }
}
```

**Notes:**

- `method` enum: `policy` | `uniform` | `passphrase`
- The `passphrase` object is only used when `method = passphrase`
- For `policy` and `uniform`, `length` is required
- For `passphrase`, `length` is ignored (derived from word count)

**Response:**

```json
{
  "method": "policy",
  "results": [
    {
      "password": "qF9!kZ3mT2@aX8#pL7wN",
      "entropyBits": 124.3,
      "policySatisfied": true,
      "composition": { "lower": 6, "upper": 6, "digits": 4, "symbols": 4 }
    }
  ],
  "generatedAtUtc": "2026-02-16T07:15:22Z"
}
```

---

#### 2. Get Service Capabilities

```
GET /api/v1/passwords/capabilities
```

**Response:**

```json
{
  "methods": ["policy", "uniform", "passphrase"],
  "limits": { "minLength": 8, "maxLength": 256, "maxCount": 50 },
  "symbolSet": "!@#$%^&*()-_=+[]{};:,.<>?",
  "excludedSimilarDefault": "O0Il1|",
  "passphrase": {
    "minWordCount": 3,
    "maxWordCount": 8,
    "defaultSeparator": "-"
  }
}
```

---

## Generation Methods

### Method A — Balanced Policy Generator (`policy`)

**Purpose:** Default "strong + usable" passwords with enforced complexity rules.

**How it works:**

1. Ensures at least 1 character from each required category
2. Fills remaining characters uniformly from the allowed pool
3. Shuffles using cryptographic RNG

**Best for:** General applications, admin accounts, user accounts.

---

### Method B — High-Entropy Uniform Generator (`uniform`)

**Purpose:** Maximum entropy with minimal constraints.

**How it works:**

1. Selects every character independently from the allowed pool using crypto RNG
2. No guaranteed inclusion per category unless explicitly requested

**Best for:** Machine-generated secrets, API keys, vault entries.

---

### Method C — Passphrase / Diceware-like Generator (`passphrase`)

**Purpose:** Memorable but strong passwords.

**How it works:**

1. Picks N words from a server-side wordlist
2. Optionally adds a digit and/or symbol, applies casing rules

**Format examples:**

- `river-hammer-cactus-7!`
- `Orbit!Frost-Delta-9`

**Best for:** Human-friendly passwords, support desks, onboarding.

---

## Validation Rules

| Rule                                                        | Error                                          |
|-------------------------------------------------------------|-------------------------------------------------|
| At least one character category must be enabled              | `VALIDATION_ERROR`                              |
| `requiredSets` cannot exceed the number of enabled sets      | `VALIDATION_ERROR`                              |
| `length` must be >= `minLength` for `policy` when `requiredSets > 1` | `VALIDATION_ERROR`                    |
| For `passphrase`, `wordCount` must be within allowed range   | `VALIDATION_ERROR`                              |
| `method` must be `policy`, `uniform`, or `passphrase`        | `UNKNOWN_METHOD`                                |

---

## Error Handling

All errors return a structured JSON body.

**Validation error (HTTP 400):**

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "requiredSets cannot exceed enabled character sets",
    "details": ["requiredSets=4 but enabledSets=3"]
  }
}
```

**Unknown method (HTTP 400):**

```json
{
  "error": {
    "code": "UNKNOWN_METHOD",
    "message": "method must be policy|uniform|passphrase"
  }
}
```

---

## Security Requirements

### Randomness

- **Use:** `System.Security.Cryptography.RandomNumberGenerator`
- **Avoid:** `System.Random` — it is not cryptographically secure

### Logging

- Never log generated passwords
- If request logging exists, redact `results[].password` from all output

### Rate Limiting (Recommended)

- 60 requests/min/IP (tunable via configuration)

### Transport

- HTTPS only in production

---

## Project Structure

```
PasswordGenerator/
├── src/
│   ├── PasswordGenerator.Api/           # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   │   └── PasswordsController.cs   # POST generate, GET capabilities
│   │   ├── DTOs/
│   │   │   ├── GenerateRequest.cs
│   │   │   ├── GenerateResponse.cs
│   │   │   ├── CapabilitiesResponse.cs
│   │   │   └── ErrorResponse.cs
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   ├── PasswordGenerator.Core/          # Domain logic
│   │   ├── Interfaces/
│   │   │   └── IPasswordGenerator.cs
│   │   ├── Models/
│   │   │   ├── PasswordRequest.cs
│   │   │   ├── PasswordResult.cs
│   │   │   └── PassphraseOptions.cs
│   │   └── Validation/
│   │       └── RequestValidator.cs
│   │
│   └── PasswordGenerator.Infrastructure/  # Implementation
│       ├── Generators/
│       │   ├── PolicyPasswordGenerator.cs
│       │   ├── UniformPasswordGenerator.cs
│       │   └── PassphrasePasswordGenerator.cs
│       ├── Helpers/
│       │   └── CryptoRng.cs
│       └── WordList/
│           └── eff-large-wordlist.txt
│
├── tests/
│   └── PasswordGenerator.Tests/
│       ├── PolicyGeneratorTests.cs
│       ├── UniformGeneratorTests.cs
│       ├── PassphraseGeneratorTests.cs
│       └── ValidationTests.cs
│
├── PasswordGenerator.sln
└── README.md
```

### Layer Responsibilities

| Layer              | Responsibility                                              |
|--------------------|-------------------------------------------------------------|
| **Api**            | Controllers, DTOs, middleware, request/response mapping      |
| **Core**           | Interfaces, domain models, validation logic                  |
| **Infrastructure** | Generator implementations, RNG helpers, wordlist loading     |

### Core Interface

```csharp
public interface IPasswordGenerator
{
    Task<IReadOnlyList<PasswordResult>> GenerateAsync(
        PasswordRequest request,
        CancellationToken ct);
}
```

**Implementations:**

- `PolicyPasswordGenerator` — enforces category inclusion, then fills and shuffles
- `UniformPasswordGenerator` — pure random selection from the allowed pool
- `PassphrasePasswordGenerator` — picks words from wordlist, applies formatting

---

## Configuration

**appsettings.json:**

```json
{
  "PasswordGenerator": {
    "SymbolSet": "!@#$%^&*()-_=+[]{};:,.<>?",
    "SimilarCharacters": "O0Il1|",
    "AmbiguousCharacters": "{}[]()/'\"~,;:.<>",
    "MinLength": 8,
    "MaxLength": 256,
    "MaxCount": 50,
    "Passphrase": {
      "WordListPath": "WordList/eff-large-wordlist.txt",
      "MinWordCount": 3,
      "MaxWordCount": 8,
      "DefaultSeparator": "-"
    }
  }
}
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later

### Run Locally

```bash
# Clone the repository
git clone <repo-url>
cd PasswordGenerator

# Restore and run
dotnet restore
dotnet run --project src/PasswordGenerator.Api

# API is available at https://localhost:5001/api/v1/passwords
```

### Run Tests

```bash
dotnet test
```

### Quick Test with curl

```bash
# Generate 3 passwords using the policy method
curl -X POST https://localhost:5001/api/v1/passwords/generate \
  -H "Content-Type: application/json" \
  -d '{
    "method": "policy",
    "length": 20,
    "count": 3,
    "includeSymbols": true,
    "requiredSets": 4
  }'

# Get service capabilities
curl https://localhost:5001/api/v1/passwords/capabilities
```
