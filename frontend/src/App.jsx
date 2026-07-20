import { useState } from 'react';
import PeriodChart from './components/PeriodChart';
import CategoryChart from './components/CategoryChart';
import TicketsList from './components/TicketsList';
import dayjs from 'dayjs';
import './App.css';

export default function App() {
  const [start, setStart] = useState(dayjs().subtract(30, 'day').format('YYYY-MM-DD'));
  const [end, setEnd]     = useState(dayjs().format('YYYY-MM-DD'));
  const [applied, setApplied] = useState({ start, end });

  const handleApply = () => {
    setApplied({ start, end });
  };

  return (
    <div className="app">
      <header className="app-header">
        <h1>📋 GLPI — Аналитика заявок</h1>
      </header>
      <main>

        {/* Единый фильтр дат для всех графиков */}
        <div className="chart-block">
          <div className="filters">
            <label>С: <input type="date" value={start} onChange={e => setStart(e.target.value)} /></label>
            <label>По: <input type="date" value={end} onChange={e => setEnd(e.target.value)} /></label>
            <button onClick={handleApply}>Применить</button>
          </div>
        </div>

        <PeriodChart start={applied.start} end={applied.end} />
        <CategoryChart start={applied.start} end={applied.end} />
        <TicketsList />

      </main>
    </div>
  );
}