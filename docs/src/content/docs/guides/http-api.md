---
title: HTTP API
description: How to query telemetry data over HTTP.
---

Telemachus exposes two HTTP interfaces for querying telemetry data.

## REST API

Each API key has its own endpoint:

```bash
# Single value
curl http://localhost:8085/api/v.altitude
# → { "v.altitude": 12500.0 }

# With parameters (passed as query string)
curl "http://localhost:8085/api/o.orbitalSpeedAt?1200.0"
# → { "o.orbitalSpeedAt": 2245.5 }
```

### Actions (POST)

Action endpoints (staging, throttle, SAS, etc.) accept POST requests:

```bash
# Activate next stage
curl -X POST http://localhost:8085/api/f.stage

# Set throttle to 50%
curl -X POST "http://localhost:8085/api/f.setThrottle?0.5"

# Toggle SAS on
curl -X POST "http://localhost:8085/api/f.sas?true"
```

## Batch API

Query multiple keys in a single request:

```bash
curl "http://localhost:8085/telemachus/datalink?key1=v.altitude&key2=o.PeA&key3=v.verticalSpeed"
# → { "v.altitude": 12500.0, "o.PeA": 75000.0, "v.verticalSpeed": 125.3 }
```

Keys are numbered `key1`, `key2`, etc. The response contains all requested values as properties in a single JSON object.

### Parameters in batch mode

Use bracket notation for parameterized keys:

```bash
curl "http://localhost:8085/telemachus/datalink?key1=o.orbitalSpeedAt[1200.0]&key2=b.name[1]"
```

### POST body

You can also POST a JSON body:

```bash
curl -X POST http://localhost:8085/telemachus/datalink \
  -H "Content-Type: application/json" \
  -d '{"key1": "v.altitude", "key2": "o.PeA"}'
```

## Error Handling

- Unknown API keys are collected in an `"unknown"` array in the response
- Errors are returned in an `"errors"` object with the key mapped to the error message
- The REST `/api/{key}` endpoint returns HTTP 404 for unknown keys

## Discovering Available Keys

Query the full API listing:

```bash
# All API keys with metadata
curl http://localhost:8085/api/a.api

# Full schema (JSON)
curl http://localhost:8085/api/a.schema

# Check installed mod integrations
curl http://localhost:8085/api/a.mods
```
