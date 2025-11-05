import type { FC } from 'react';
import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { AuthProvider } from './context/AuthContext';
import { SigninOidcCallback } from './components/SigninOidcCallback';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Admin } from './pages/Admin'
import { Login } from './pages/Login'
import { Home } from './pages/Home'
import { ROUTE_PATHS } from './routes/paths'
import { ADMIN_ROLE, MEMBER_ROLE } from './lib/roles'

export const App: FC = () => {
  return (
    <AuthProvider>
      <BrowserRouter>
        <Routes>
          <Route path={ROUTE_PATHS.CALLBACK} element={<SigninOidcCallback />} />
          <Route path={ROUTE_PATHS.HOME} element={<ProtectedRoute roles={[ADMIN_ROLE,MEMBER_ROLE]}><Home /></ProtectedRoute>} />
          <Route path={ROUTE_PATHS.ADMIN} element={<ProtectedRoute roles={[ADMIN_ROLE]}><Admin /></ProtectedRoute>} />
          <Route path={ROUTE_PATHS.LOGIN} element={<Login />} />
          <Route path={ROUTE_PATHS.ALL} element={<Login />} />
        </Routes>
      </BrowserRouter>
    </AuthProvider>
  );
};
