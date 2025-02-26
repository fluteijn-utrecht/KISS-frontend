import { fetchKlantByIdOk2 } from "@/services/openklant2";
import { fetchKlantByIdOk1 } from "@/services/openklant1";
import {
  registryVersions,
  type Systeem,
} from "@/services/environment/fetch-systemen";

export const fetchKlantById = ({
  id,
  systeem,
}: {
  id: string;
  systeem: Systeem;
}) =>
  systeem.registryVersion === registryVersions.ok1
    ? fetchKlantByIdOk1(systeem.identifier, id)
    : fetchKlantByIdOk2(systeem.identifier, id);
