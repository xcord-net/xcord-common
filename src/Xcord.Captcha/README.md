# Xcord.Captcha - portable ghost-font captcha

Server-rendered motion-coherence captcha: a short code is drawn as an animated GIF whose
letters are formed only by the motion of low-contrast, background-matched dots. No single
frame is readable, and a max-projection of all frames stays diffuse (decoys dominate) - only
coherent directional motion reveals the code to a human. An offline audio rendering (spoken
characters + noise) is the accessible fallback.

The core depends only on ImageSharp. Storage is behind an interface with a zero-dependency
in-memory default; Redis is an optional adapter.

## Security model (be honest)

Defeats the common attack: commodity solvers that run single-image OCR / a vision API (there
is no readable frame). Recovering the code requires downloading the GIF and running
optical-flow / motion analysis - far more expensive and not offered off the shelf. It does
NOT claim to defeat a determined attacker who custom-builds a motion pipeline; pair it with
rate limiting and the single-use, TTL-expiring answer store (both provided/wired by the host).

## Copy into another project

Copy the whole `Xcord.Captcha/` folder. Keep or delete:

- `Storage/RedisCaptchaStore.cs` - delete if you do not use Redis (the in-memory store ships
  by default).
- `AspNetCore/` - delete if you wire the service yourself; then you may also drop the
  `FrameworkReference Microsoft.AspNetCore.App` from the csproj.

The core (`Rendering/`, `Storage/InMemoryCaptchaStore`) has no `Xcord.*`, Redis, or ASP.NET
coupling - a unit test (`PortabilityGuardTests`) enforces this.

## Wire it (3 steps)

1. `services.AddGhostFontCaptcha(o => o.Enabled = true);`
   Add `.UseRedisCaptchaStore();` if an `IConnectionMultiplexer` is registered.
2. `app.MapGhostFontCaptcha();` (default base path `/api/v1/auth/captcha`).
3. In your registration handler:
   `if (!await captcha.ValidateAsync(id, answer)) return BadRequest("CAPTCHA_FAILED");`

`Enabled = false` swaps in a no-op service (validation always passes, placeholder renders) for
dev/test.

## Endpoints

- `GET {base}` -> `{ captchaId, imageUrl, audioUrl }`
- `GET {base}/{id}.gif` -> `image/gif`
- `GET {base}/{id}.wav` -> `audio/wav`

`ValidateAsync` is single-use (a correct answer is consumed). Renders are reproducible for an
id (the render seed is stored) and do NOT consume the answer.

## Frontend

Render `<img src=imageUrl>` plus an audio toggle to `<audio src=audioUrl>`, and a text input.
A SolidJS component ships in the Xcord apps; porting to another framework is ~80 lines against
the 3 endpoints above.

## Notes

- Charset excludes look-alikes (`ACDEFHJKMNPRTUVWXY34679`); codes are case-insensitive.
- Audio clips are embedded 16 kHz mono WAVs generated offline (no runtime TTS dependency).
- Options: `Enabled`, `CodeLength` (default 5), `TtlMinutes` (default 5), `KeyPrefix`.
