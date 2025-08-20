// components/filters/CheckboxFilter.tsx
"use client";

import { motion, AnimatePresence } from "framer-motion";
import { ChevronDown } from "lucide-react";
import { cn } from "@/lib/utils";
import { Checkbox, Label } from "@/components/ui";
import React from "react";

interface Props {
  title: string;
  options: string[];
  values: Record<string, boolean>;
  onChange: (key: string, value: boolean) => void;
  isOpen: boolean;
  toggleOpen: () => void;
}

export const CheckboxFilter = React.memo((props: Props) => {
  const { title, options, values, onChange, isOpen, toggleOpen } = props;
  return (
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
            className="space-y-1 flex flex-col text-filter-text"
            initial={{ height: 0, opacity: 0 }}
            animate={{ height: "auto", opacity: 1 }}
            exit={{ height: 0, opacity: 0 }}
            transition={{ duration: 0.2 }}
          >
            {options.map((opt) => (
              <div key={opt} className="flex items-center gap-2">
                <Checkbox
                  checked={values[opt]}
                  onCheckedChange={(checked) => onChange(opt, Boolean(checked))}
                />
                <Label>{opt}</Label>
              </div>
            ))}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
});
