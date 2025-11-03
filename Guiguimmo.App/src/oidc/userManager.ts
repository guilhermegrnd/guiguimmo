import { UserManager, type UserManagerSettings } from 'oidc-client-ts';

const oidcConfig: UserManagerSettings = {
  authority: 'https://localhost:5001',
  client_id: 'my-client-app',
  redirect_uri: `${globalThis.location.origin}/callback`,
  post_logout_redirect_uri: `${globalThis.location.origin}/`,
  scope: 'openid offline_access roles',
  response_type: 'code',
  client_secret: 'uma-chave-secreta-forte',
  automaticSilentRenew: true,
  filterProtocolClaims: true,
  loadUserInfo: true,
};

const userManager = new UserManager(oidcConfig);

export { userManager, oidcConfig };
