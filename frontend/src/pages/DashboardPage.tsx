import { useAuth } from '../context/AuthContext';

export const DashboardPage = () => {
  const { logout, refresh } = useAuth();

  return (
    <div>
      <h1>Dashboard</h1>
      <button onClick={() => refresh()}>Renovar token</button>
      <button onClick={() => logout()}>Sair</button>
    </div>
  );
};
