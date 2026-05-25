export const env = {
  apiUrl: import.meta.env.VITE_URL,
  domen: import.meta.env.VITE_URL?.replace(/^https?:\/\//, '').replace(/:\d+$/, ''),
};
