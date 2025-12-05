import type { HttpMethod } from '../types/http'

/**
 * Generic function to handle API fetching with type safety.
 *
 * @param token Token for authentication.
 * @param url The full URL or relative path to the API endpoint.
 * @param method The HTTP method (GET, POST, PUT, DELETE, etc.).
 * @param body Optional request body for POST/PUT/PATCH requests.
 * @returns A promise that resolves to the expected data type (T).
 */
export async function fetchApi<T>(
  token: string,
  url: string,
  method: HttpMethod = 'GET',
  body?: unknown
): Promise<T> {
  
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`, 
  };

  const config: RequestInit = {
    method,
    headers,
    body: (body && method !== 'GET' && method !== 'DELETE') ? JSON.stringify(body) : undefined,
  };

  try {
    const response = await fetch(url, config);

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ message: response.statusText }));
      
      throw new Error(errorData.message || `API Error: ${response.status}`);
    }

    if (response.status === 204 || response.headers.get('content-length') === '0') {
      return {} as T;
    }

    return (await response.json()) as T;
  } catch (error) {
    console.error('Fetch API error:', error);
    throw error; 
  }
}