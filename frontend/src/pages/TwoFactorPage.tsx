import { useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

interface LocationState {
  userId?: string;
}

export const TwoFactorPage = () => {
  const [code, setCode] = useState('');
  const [error, setError] = useState('');
  const { verifyTwoFactor } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const state = location.state as LocationState;

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');
    if (!state?.userId) {
      setError('Usuário inválido.');
      return;
    }

    try {
      await verifyTwoFactor(state.userId, code);
      navigate('/');
    } catch {
      setError('Código inválido.');
    }
  };

  return (
    <div>
      <h1>2FA</h1>
      {error && <p>{error}</p>}
      <form onSubmit={handleSubmit}>
        <input value={code} onChange={(e) => setCode(e.target.value)} placeholder="Código" />
        <button type="submit">Validar</button>
      </form>
    </div>
  );
};
