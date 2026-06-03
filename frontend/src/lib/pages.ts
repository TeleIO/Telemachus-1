// Single registry of the bundled UI pages. Dedups the page lists that were
// hand-written separately in index.html and information.html.

export interface PageInfo {
  id: string;
  title: string;
  href: string;
  description: string;
}

export const PAGES: PageInfo[] = [
  {
    id: "console",
    title: "Graphs and Tables",
    href: "console.html",
    description: "Display 3 graphs and a list of textual data.",
  },
  {
    id: "map",
    title: "Kerbal Maps",
    href: "map.html",
    description: "This page shows your active vessel's current location using Kerbal Maps.",
  },
  {
    id: "d-pad",
    title: "D-Pad",
    href: "d-pad.html",
    description: "This page allows you to pitch, yaw, roll and translate your craft.",
  },
  {
    id: "touchball-pyr",
    title: "Touchball Pitch, Yaw, Roll",
    href: "touchball-pyr.html",
    description: "This page allows you to adjust the pitch, yaw and roll of your craft.",
  },
  {
    id: "flight-control",
    title: "Basic Flight Control",
    href: "flight-control.html",
    description:
      "Use this interface to send basic commands to your vessel, such as staging, setting the throttle and toggling ASAS.",
  },
  {
    id: "smart-ass",
    title: "Smart A.S.S.",
    href: "smart-ass.html",
    description:
      "This page interfaces with MechJeb2 and allows you to use some of the Smart A.S.S. functions. If you do not have MechJeb2 installed then it will not function.",
  },
  {
    id: "speech",
    title: "Speech Commands",
    href: "speech.html",
    description: "This page demonstrates the possibility of voice control for KSP.",
  },
  {
    id: "houston",
    title: "Houston & MKON UI",
    href: "houston/index.html",
    description: "This page opens Houston and MKON UI.",
  },
];
