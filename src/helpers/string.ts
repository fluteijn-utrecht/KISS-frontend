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
