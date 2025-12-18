'use client'
import OrderQueue from "@/components/widgets/Catalog";
import {useState} from "react";

export default function Catalog() {
    const [hidden, setHidden] = useState(false);

    return hidden ? <OrderQueue hideWindow={setHidden} /> : <button onClick={() => setHidden(true)} >Открыть боковую очередь</button>;
}
