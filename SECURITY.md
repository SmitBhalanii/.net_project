# Security Policy
- Hardcoded secrets are strictly prohibited in `appsettings.json`.
- All POST/PUT/DELETE requests must validate Anti-Forgery Tokens.
- Entity ownership is verified in controllers to prevent Insecure Direct Object References (IDOR).
- No sensitive payment information is stored locally.
