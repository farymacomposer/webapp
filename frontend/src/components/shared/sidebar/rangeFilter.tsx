// components/filters/RangeFilter.tsx
"use client";

import { motion, AnimatePresence } from "framer-motion";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface Props {
  title: string;
  from: number;
  to: number;
  onChangeFrom: (v: number) => void;
  onChangeTo: (v: number) => void;
  isOpen: boolean;
  toggleOpen: () => void;
}

export const RangeFilter: React.FC<Props> = ({
  title,
  from,
  to,
  onChangeFrom,
  onChangeTo,
  isOpen,
  toggleOpen,
}) => (
  <div>
    <button onClick={toggleOpen} className="flex items-center w-full mb-2">
      <ChevronDown
        className={cn("transition-transform", isOpen && "rotate-180")}
      />
      <h2 className="font-semibold">{title}</h2>
    </button>
    <AnimatePresence initial={false}>
      {isOpen && (
        <motion.div
          className="flex items-center gap-2 mb-2"
          initial={{ height: 0, opacity: 0 }}
          animate={{ height: "auto", opacity: 1 }}
          exit={{ height: 0, opacity: 0 }}
          transition={{ duration: 0.2 }}
        >
          <input
            type="number"
            min={1900}
            max={to}
            value={from}
            onChange={(e) => onChangeFrom(Number(e.target.value))}
            className="w-16 rounded px-2 py-1 bg-card border-none text-sm text-black"
          />
          <span>–</span>
          <input
            type="number"
            min={from}
            max={2100}
            value={to}
            onChange={(e) => onChangeTo(Number(e.target.value))}
            className="w-16 rounded px-2 py-1 bg-card border-none text-sm text-black"
          />
        </motion.div>
      )}
    </AnimatePresence>
  </div>
);
