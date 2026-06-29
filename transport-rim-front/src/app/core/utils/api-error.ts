import { HttpErrorResponse } from '@angular/common/http';

/**
 * Extracts a human-readable message from an API error response.
 *
 * Handles two distinct shapes the backend can return:
 * - our own controllers: `{ message: "..." }`
 * - ASP.NET Core's automatic [ApiController] model validation: `{ errors: { Field: ["..."] } }`
 */
export function extractApiErrorMessage(err: unknown, fallback: string): string {
  const body: any = err instanceof HttpErrorResponse ? err.error : (err as { error?: unknown })?.error;

  if (body?.message) {
    return body.message;
  }

  if (body?.errors) {
    const firstField = Object.values(body.errors)[0];
    if (Array.isArray(firstField) && firstField.length > 0) {
      return firstField[0];
    }
  }

  return fallback;
}
