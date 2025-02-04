import {
  fetchSystemen,
  klantinteractieVersions,
} from "@/services/environment/fetch-systemen";

export const getRegisterDetails = async () => {
  const systemen = await fetchSystemen();
  const defaultSysteem = systemen.find(({ isDefault }) => isDefault);

  if (!defaultSysteem) {
    throw new Error("Geen default register gevonden");
  }

  return {
    systeemId: defaultSysteem.identifier,
    useKlantInteractiesApi:
      defaultSysteem.klantinteractieVersion === klantinteractieVersions.ok2,
  };
};
