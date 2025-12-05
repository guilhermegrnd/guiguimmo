import { useState, useCallback } from 'react';
import type { HttpMethod } from '../types/http'
import { fetchApi } from '../api/fetchApi'

interface ApiHookResult<T> {
  data: T | null;
  loading: boolean;
  error: Error | null;
  fetchData: (token: string, url: string, method?: HttpMethod, body?: unknown) => Promise<void>;
}

export function useApi<T = unknown>(): ApiHookResult<T> {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState<boolean>(false);
  const [error, setError] = useState<Error | null>(null);

  const fetchData = useCallback(
    async (token: string, url: string, method: HttpMethod = 'GET', body?: unknown) => {
      setLoading(true);
      setError(null);
      
      try {
        const result = await fetchApi<T>(token, url, method, body);
        setData(result);
      } catch (err) {
        setError(err as Error);
      } finally {
        setLoading(false);
      }
    }, []);

  return { data, loading, error, fetchData };
}