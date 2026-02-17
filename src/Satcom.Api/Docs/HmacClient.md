HMAC Client Signing Examples

This file shows how to sign requests so the server-side HMAC middleware can validate them.

Signing contract (server expects):

- Headers:
  - `x-timestamp`: unix seconds (UTC)
  - `x-signature`: base64(HMAC-SHA256(secret, canonicalString))
- canonicalString = METHOD + "\n" + PATH + "\n" + TIMESTAMP + "\n" + BODY_SHA256_HEX

Notes:

- PATH should include the path and any route segments, but not the host or query string.
- If your request has a body, compute SHA256 of the raw bytes and encode as lower-case hex. For empty bodies use the SHA256 of empty string.
- The server reads `ApiSigningSecret` from configuration.
- The timestamp must be within the allowed skew (default 300 seconds).

Node.js example (fetch + crypto):

```js
import crypto from 'crypto';
import fetch from 'node-fetch';

function sha256Hex(buf) {
  return crypto.createHash('sha256').update(buf).digest('hex');
}

function sign(secret, method, path, timestamp, bodyBuf) {
  const bodyHash = sha256Hex(bodyBuf || Buffer.from(''));
  const canonical = `${method.toUpperCase()}\n${path}\n${timestamp}\n${bodyHash}`;
  const sig = crypto
    .createHmac('sha256', secret)
    .update(Buffer.from(canonical, 'utf8'))
    .digest('base64');
  return sig;
}

async function call() {
  const secret = process.env.API_SIGNING_SECRET;
  const method = 'POST';
  const path = '/v1/some/endpoint';
  const body = JSON.stringify({ hello: 'world' });
  const ts = Math.floor(Date.now() / 1000).toString();
  const signature = sign(secret, method, path, ts, Buffer.from(body, 'utf8'));

  const res = await fetch('http://localhost:5143' + path, {
    method,
    body,
    headers: {
      'Content-Type': 'application/json',
      'x-timestamp': ts,
      'x-signature': signature,
    },
  });
  console.log(await res.text());
}

call().catch(console.error);
```

Curl example (using openssl to compute signature):

```bash
SECRET='my-secret'
METHOD='POST'
PATH='/v1/some/endpoint'
BODY='{"hello":"world"}'
TS=$(date +%s)
BODY_HASH=$(printf "%s" "$BODY" | openssl dgst -sha256 -binary | xxd -p -c 256)
CANONICAL="$METHOD\n$PATH\n$TS\n$BODY_HASH"
SIG=$(printf "%s" "$CANONICAL" | openssl dgst -sha256 -hmac "$SECRET" -binary | openssl base64)

curl -v -X $METHOD "http://localhost:5143$PATH" \
  -H "Content-Type: application/json" \
  -H "x-timestamp: $TS" \
  -H "x-signature: $SIG" \
  -d "$BODY"
```

Python example (requests):

```py
import time, hashlib, hmac, base64
import requests

secret = b"my-secret"
method = "POST"
path = "/v1/some/endpoint"
body = b'{"hello":"world"}'
ts = str(int(time.time()))
body_hash = hashlib.sha256(body).hexdigest()
canonical = f"{method}\n{path}\n{ts}\n{body_hash}".encode('utf8')
sig = base64.b64encode(hmac.new(secret, canonical, hashlib.sha256).digest()).decode()

res = requests.post('http://localhost:5143' + path, data=body, headers={'x-timestamp': ts, 'x-signature': sig, 'Content-Type': 'application/json'})
print(res.status_code, res.text)
```

Keep the secret safe. Rotate as needed and update server configuration.
