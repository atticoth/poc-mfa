import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const LoginPage = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');
    try {
      const result = await login(email, password);
      if (result.requiresTwoFactor && result.userId) {
        navigate('/2fa', { state: { userId: result.userId } });
      } else {
        navigate('/');
      }
    } catch {
      setError('Falha no login.');
    }
  };

  return (
    <div>
      <h1>Login</h1>
      {error && <p>{error}</p>}
      <form onSubmit={handleSubmit}>
        <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
        <input
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Senha"
          type="password"
        />
        <button type="submit">Entrar</button>
      </form>
      <button type="button" onClick={() => navigate('/register')}>
        Criar conta
      </button>
    </div>
  );
};
