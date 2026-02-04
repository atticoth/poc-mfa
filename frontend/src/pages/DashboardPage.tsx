import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const DashboardPage = () => {
  const { logout, refresh } = useAuth();
  const navigate = useNavigate();

  return (
    <div>
      <h1>Dashboard</h1>
      <button onClick={() => refresh()}>Renovar token</button>
      <button onClick={() => navigate('/2fa/enable')}>Ativar 2FA</button>
      <button onClick={() => logout()}>Sair</button>
    </div>
  );
};
