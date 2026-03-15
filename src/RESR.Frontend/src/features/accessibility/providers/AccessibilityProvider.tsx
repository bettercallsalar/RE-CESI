import { createContext, useContext, useEffect, useMemo, useState, type PropsWithChildren } from "react";

type VisualMode = "default" | "contrast";

interface AccessibilityContextValue {
  isContrastMode: boolean;
  visualMode: VisualMode;
  setContrastMode: (enabled: boolean) => void;
  toggleContrastMode: () => void;
}

const STORAGE_KEY = "resr.accessibility.visualMode";

const AccessibilityContext = createContext<AccessibilityContextValue | null>(null);

function getInitialVisualMode(): VisualMode {
  if (typeof window === "undefined") {
    return "default";
  }

  return window.localStorage.getItem(STORAGE_KEY) === "contrast" ? "contrast" : "default";
}

export function AccessibilityProvider({ children }: PropsWithChildren) {
  const [visualMode, setVisualMode] = useState<VisualMode>(getInitialVisualMode);

  useEffect(() => {
    document.documentElement.dataset.visualMode = visualMode;

    if (visualMode === "contrast") {
      window.localStorage.setItem(STORAGE_KEY, visualMode);
      return;
    }

    window.localStorage.removeItem(STORAGE_KEY);
  }, [visualMode]);

  const value = useMemo<AccessibilityContextValue>(() => ({
    isContrastMode: visualMode === "contrast",
    visualMode,
    setContrastMode(enabled) {
      setVisualMode(enabled ? "contrast" : "default");
    },
    toggleContrastMode() {
      setVisualMode((current) => current === "contrast" ? "default" : "contrast");
    }
  }), [visualMode]);

  return (
    <AccessibilityContext.Provider value={value}>
      {children}
    </AccessibilityContext.Provider>
  );
}

export function useAccessibility() {
  const context = useContext(AccessibilityContext);

  if (!context) {
    throw new Error("useAccessibility must be used within AccessibilityProvider");
  }

  return context;
}
