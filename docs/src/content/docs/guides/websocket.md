---
title: WebSocket Streaming
description: Subscribe to real-time telemetry updates over WebSocket.
---

For real-time telemetry, connect via WebSocket to receive continuous updates at a configurable rate.

## Connecting

```javascript
const ws = new WebSocket("ws://localhost:8085/datalink");
```

## Subscribing

Send a JSON message to subscribe to variables:

```javascript
ws.send(JSON.stringify({
  "+": ["v.altitude", "o.PeA", "v.verticalSpeed"],
  "rate": 500  // milliseconds between updates
}));
```

The server will start sending JSON objects at the specified rate:

```json
{ "v.altitude": 12500.0, "o.PeA": 75000.0, "v.verticalSpeed": 125.3 }
```

## Unsubscribing

Remove variables from your subscription:

```javascript
ws.send(JSON.stringify({
  "-": ["v.verticalSpeed"]
}));
```

## Changing Rate

Update the polling rate without changing subscriptions:

```javascript
ws.send(JSON.stringify({
  "rate": 200
}));
```

## Combined Messages

Subscribe, unsubscribe, and change rate in a single message:

```javascript
ws.send(JSON.stringify({
  "+": ["o.inclination", "o.eccentricity"],
  "-": ["v.altitude"],
  "rate": 1000
}));
```

## Example: Live Altitude Display

```html
<div id="altitude">--</div>
<script>
  const ws = new WebSocket("ws://localhost:8085/datalink");
  ws.onopen = () => {
    ws.send(JSON.stringify({
      "+": ["v.altitude"],
      rate: 200
    }));
  };
  ws.onmessage = (event) => {
    const data = JSON.parse(event.data);
    if (data["v.altitude"] !== undefined) {
      document.getElementById("altitude").textContent =
        Math.round(data["v.altitude"]) + " m";
    }
  };
</script>
```
