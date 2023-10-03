export function fullName(name: any) {
  const {
    voorletters = "",
    voornaam = "",
    voorvoegselAchternaam = "",
    achternaam = "",
  } = name || {};

  return [voornaam || voorletters, voorvoegselAchternaam, achternaam]
    .filter(Boolean)
    .join(" ");
}

export const capitalizeFirstLetter = (val: string) =>
  `${val?.[0]?.toLocaleUpperCase() || ""}${val?.substring(1) || ""}`;
