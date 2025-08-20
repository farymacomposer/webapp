''
import { useOrdersSignalR } from "@/hooks/useSignalR";

export default function StreamPage() {
  const { orders, connected } = useOrdersSignalR();

  const sortedOrders = [...orders].sort(
    (a, b) =>
      (a.currentPosition?.queueIndex || 0) -
      (b.currentPosition?.queueIndex || 0)
  );

  return (
    <div className="p-4 space-y-4">
      <h1>Stream ({connected ? "Connected" : "Disconnected"})</h1>
      <ul>
        {sortedOrders.map((o) => (
          <li key={o.id}>
            {o.mainNickname} — статус: {o.status} — очередь:{" "}
            {o.currentPosition?.queueIndex ?? "-"}
          </li>
        ))}
      </ul>
    </div>
  );
}
