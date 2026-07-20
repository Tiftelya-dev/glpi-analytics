import { useEffect, useState } from 'react';
import {
  LineChart, Line, XAxis, YAxis, CartesianGrid,
  Tooltip, Legend, ResponsiveContainer
} from 'recharts';
import { getStatsByPeriod } from '../api/ticketsApi';
import dayjs from 'dayjs';

export default function PeriodChart({ start, end }) {
  const [data, setData] = useState([]);

  useEffect(() => {
    getStatsByPeriod(start, end).then(res => {
      setData(res.data.map(d => ({
        date: dayjs(d.date).format('DD.MM'),
        count: d.count
      })));
    });
  }, [start, end]);  // перезагружается когда меняются даты

  return (
    <div className="chart-block">
      <h2>Заявки за период</h2>
      <ResponsiveContainer width="100%" height={300}>
        <LineChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="date" />
          <YAxis allowDecimals={false} />
          <Tooltip />
          <Legend />
          <Line type="monotone" dataKey="count" name="Кол-во заявок" stroke="#4f86f7" strokeWidth={2} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}