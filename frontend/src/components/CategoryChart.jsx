import { useEffect, useState } from 'react';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid,
  Tooltip, ResponsiveContainer
} from 'recharts';
import { getStatsByCategories } from '../api/ticketsApi';

export default function CategoryChart({ start, end }) {
  const [byType, setByType]         = useState([]);
  const [byPriority, setByPriority] = useState([]);

  useEffect(() => {
    getStatsByCategories(start, end).then(res => {
      setByType(res.data.byType.map(d => ({ name: d.category, count: d.count })));
      setByPriority(res.data.byPriority.map(d => ({ name: d.category, count: d.count })));
    });
  }, [start, end]);  // перезагружается когда меняются даты

  return (
    <div className="chart-block">
      <h2>По типу заявок</h2>
      <ResponsiveContainer width="100%" height={250}>
        <BarChart data={byType}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" />
          <YAxis allowDecimals={false} />
          <Tooltip />
          <Bar dataKey="count" name="Кол-во" fill="#4f86f7" />
        </BarChart>
      </ResponsiveContainer>

      <h2 style={{ marginTop: 24 }}>По приоритету</h2>
      <ResponsiveContainer width="100%" height={250}>
        <BarChart data={byPriority}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="name" />
          <YAxis allowDecimals={false} />
          <Tooltip />
          <Bar dataKey="count" name="Кол-во" fill="#f7944f" />
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}