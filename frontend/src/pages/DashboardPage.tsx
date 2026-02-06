import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { api } from '../services/api';

export const DashboardPage = () => {
  const { logout, refresh } = useAuth();
  const navigate = useNavigate();
  const [message, setMessage] = useState('');

  const handleDisableTwoFactor = async () => {
    setMessage('');
    await api.post('/auth/2fa/disable');
    setMessage('2FA desabilitado.');
  };

  return (
    <div>
      <h1>Dashboard</h1>
      {message && <p>{message}</p>}
      <button onClick={() => refresh()}>Renovar token</button>
      <button onClick={() => navigate('/2fa/enable')}>Ativar 2FA</button>
      <button onClick={handleDisableTwoFactor}>Desabilitar 2FA</button>
      <button onClick={() => logout()}>Sair</button>
    </div>
  );
};
