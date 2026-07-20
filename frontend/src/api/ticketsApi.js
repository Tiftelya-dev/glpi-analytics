import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:5000/api',
});

export const getTickets = (type, priority) =>
  api.get('/tickets', { params: { type, priority } });

export const getStatsByPeriod = (start, end) =>
  api.get('/tickets/stats/period', { params: { start, end } });

export const getStatsByCategories = (start, end) =>
  api.get('/tickets/stats/categories', { params: { start, end } });

export const syncTickets = () =>
  api.post('/tickets/sync');