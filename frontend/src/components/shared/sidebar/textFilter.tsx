// components/filters/TextFilter.tsx
"use client";

import { motion, AnimatePresence } from "framer-motion";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";

interface Props {
  title: string;
  value: string;
  onChange: (v: string) => void;
  isOpen: boolean;
  toggleOpen: () => void;
}

export const TextFilter: React.FC<Props> = ({
  title,
  value,
  onChange,
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
          className="mb-2"
          initial={{ height: 0, opacity: 0 }}
          animate={{ height: "auto", opacity: 1 }}
          exit={{ height: 0, opacity: 0 }}
          transition={{ duration: 0.2 }}
        >
          <input
            type="text"
            value={value}
            onChange={(e) => onChange(e.target.value)}
            className="rounded px-2 py-1 bg-card border-none text-sm w-full text-black"
          />
        </motion.div>
      )}
    </AnimatePresence>
  </div>
);
