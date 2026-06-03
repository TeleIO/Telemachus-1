// Console UI strings. console.html and ru_console.html were byte-identical apart
// from these labels — so the Console component renders once and just picks a
// locale (?lang=ru → Russian), instead of duplicating the whole page.

export interface Category {
  regex: string;
  label: string;
}

export interface ConsoleStrings {
  telemetry: string;
  add: string;
  saveLayout: string;
  deleteLayout: string;
  changeLayout: string;
  changeChart: string;
  categories: Category[];
  bodiesLabel: string;
  bodies: string[];
  other: Category;
}

const BODY_REGEX = (i: number) => `^b\\..*\\[${i}\\]`;

function withBodies(bodies: string[]): Category[] {
  return bodies.map((label, i) => ({ regex: BODY_REGEX(i), label }));
}

export const en: ConsoleStrings = {
  telemetry: "Telemetry",
  add: "Add",
  saveLayout: "Save layout",
  deleteLayout: "Delete layout",
  changeLayout: "Change Layout",
  changeChart: "Change Chart",
  categories: [
    { regex: "^v\\.", label: "Vessel" },
    { regex: "^o\\.", label: "Orbit" },
    { regex: "^n\\.", label: "Nav Ball" },
    { regex: "^s\\.", label: "Sensors" },
    { regex: "^tar\\.", label: "Target" },
    { regex: "^dock\\.", label: "Docking" },
    { regex: "^r\\.", label: "Resources" },
  ],
  bodiesLabel: "Celestial Bodies",
  bodies: ["Kerbol", "Kerbin", "Mun", "Minmus", "Moho", "Eve", "Duna", "Ike", "Jool", "Laythe", "Vall", "Bop", "Tylo", "Gilly", "Pol", "Dres", "Eeloo"],
  other: { regex: "^(?!([vbnos]|dock|tar)\\.)", label: "Other" },
};

export const ru: ConsoleStrings = {
  telemetry: "Telemetry",
  add: "Add",
  saveLayout: "Сохранить панель",
  deleteLayout: "Удалить панель",
  changeLayout: "Смена панели",
  changeChart: "Сменить график",
  categories: [
    { regex: "^v\\.", label: "Корабль" },
    { regex: "^o\\.", label: "Орбита" },
    { regex: "^n\\.", label: "Nav Ball" },
    { regex: "^s\\.", label: "Сенсоры" },
    { regex: "^tar\\.", label: "Цель" },
    { regex: "^dock\\.", label: "Стыковка" },
    { regex: "^r\\.", label: "Ресурсы" },
  ],
  bodiesLabel: "Небесные тела",
  bodies: ["Кербол", "Кербин", "Муна", "Минмус", "Мохо", "Ева", "Дюна", "Айк", "Джул", "Лайф", "Валли", "Боп", "Тайло", "Гилли", "Пол", "Дрес", "Илу"],
  other: { regex: "^(?!([vbnos]|dock|tar)\\.)", label: "Другое" },
};

export function strings(lang = ""): ConsoleStrings {
  const l = lang || new URLSearchParams(globalThis.location?.search).get("lang") || "";
  return l.startsWith("ru") ? ru : en;
}

/** Body labels for `withBodies`, exposed for the category builder. */
export function categoryList(s: ConsoleStrings): Category[] {
  return [...s.categories, ...withBodies(s.bodies), s.other];
}
