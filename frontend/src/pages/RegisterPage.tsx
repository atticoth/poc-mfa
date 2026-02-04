import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export const RegisterPage = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    setError('');
    setMessage('');

    if (password !== confirmPassword) {
      setError('As senhas não conferem.');
      return;
    }

    try {
      await register(email, password);
      setMessage('Cadastro realizado com sucesso. Faça login.');
      navigate('/login');
    } catch {
      setError('Falha ao cadastrar. Verifique os dados.');
    }
  };

  return (
    <div>
      <h1>Cadastro</h1>
      {error && <p>{error}</p>}
      {message && <p>{message}</p>}
      <form onSubmit={handleSubmit}>
        <input value={email} onChange={(e) => setEmail(e.target.value)} placeholder="Email" />
        <input
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          placeholder="Senha"
          type="password"
        />
        <input
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          placeholder="Confirmar senha"
          type="password"
        />
        <button type="submit">Criar conta</button>
      </form>
    </div>
  );
};
