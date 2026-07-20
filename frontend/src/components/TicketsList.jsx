import { useEffect, useState } from 'react';
import { getTickets, syncTickets } from '../api/ticketsApi';
import dayjs from 'dayjs';

const PRIORITY_COLOR = {
  Critical: '#e74c3c',
  High: '#e67e22',
  Moderate: '#f1c40f',
  Low: '#2ecc71',
  Unknown: '#95a5a6',
};

export default function TicketsList() {
  const [tickets, setTickets] = useState([]);
  const [type, setType] = useState('');
  const [priority, setPriority] = useState('');
  const [syncing, setSyncing] = useState(false);

  const load = async () => {
    const res = await getTickets(type || undefined, priority || undefined);
    setTickets(res.data);
  };

  const handleSync = async () => {
    setSyncing(true);
    await syncTickets();
    await load();
    setSyncing(false);
  };

  useEffect(() => { load(); }, []);

  return (
    <div className="chart-block">
      <div className="tickets-header">
        <h2>Список заявок</h2>
        <button onClick={handleSync} disabled={syncing}>
          {syncing ? 'Синхронизация...' : '🔄 Синхронизировать'}
        </button>
      </div>

      <div className="filters">
        <select value={type} onChange={e => setType(e.target.value)}>
          <option value="">Все типы</option>
          <option value="Incident">Инцидент</option>
          <option value="Request">Запрос</option>
        </select>
        <select value={priority} onChange={e => setPriority(e.target.value)}>
          <option value="">Все приоритеты</option>
          <option value="Critical">Критичный</option>
          <option value="High">Высокий</option>
          <option value="Moderate">Умеренный</option>
          <option value="Low">Низкий</option>
        </select>
        <button onClick={load}>Фильтровать</button>
      </div>

      <table className="tickets-table">
        <thead>
          <tr>
            <th>ID</th>
            <th>Название</th>
            <th>Тип</th>
            <th>Приоритет</th>
            <th>Статус</th>
            <th>Дата создания</th>
          </tr>
        </thead>
        <tbody>
          {tickets.map(t => (
            <tr key={t.id}>
              <td>{t.glpiId}</td>
              <td>{t.title}</td>
              <td>{t.type === 'Incident' ? 'Инцидент' : 'Запрос'}</td>
              <td>
                <span style={{
                  background: PRIORITY_COLOR[t.priority],
                  color: '#fff',
                  padding: '2px 10px',
                  borderRadius: '12px',
                  fontSize: '0.85em'
                }}>
                  {t.priority}
                </span>
              </td>
              <td>{t.status}</td>
              <td>{dayjs(t.createdAt).format('DD.MM.YYYY HH:mm')}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}