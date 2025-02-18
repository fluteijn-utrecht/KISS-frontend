import {
  fetchSystemen,
  registryVersions,
} from "@/services/environment/fetch-systemen";

export const getRegisterDetails = async () => {
  const systemen = await fetchSystemen();
  const defaultSysteem = systemen.find(({ isDefault }) => isDefault);

  if (!defaultSysteem) {
    throw new Error("Geen default register gevonden");
  }

  return {
    defaultSystemId: defaultSysteem.identifier,
    useKlantInteractiesApi:
      defaultSysteem.registryVersion === registryVersions.ok2,
  };
};
