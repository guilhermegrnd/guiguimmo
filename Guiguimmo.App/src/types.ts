export interface IOidcUser {
  id_token?: string;
  access_token?: string;
  expires_at?: number;
  profile: Record<string, unknown> & {
    name?: string;
    sub: string;
    role?: string[];
  };
}