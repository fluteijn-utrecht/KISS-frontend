<template>
  <service-data-search
    :get-data="useAfdelingen"
    :map-value="(x) => x.naam"
    :map-description="(x) => x.identificatie"
    :model-value="modelValue"
    @update:modelValue="$emit('update:modelValue', $event)"
    v-bind="$attrs"
  />
</template>

<script lang="ts">
import {
  ServiceResult,
  fetchLoggedIn,
  parseJson,
  parsePagination,
  throwIfNotOk,
  type PaginatedResult,
} from "@/services";

interface Afdeling {
  id: string;
  identificatie: string;
  naam: string;
}
const getAfdelingenSearchUrl = (search: string | undefined) => {
  const searchParams = new URLSearchParams();
  searchParams.set("ordering", "record__data__naam");
  if (search) {
    searchParams.set("data_attrs", `naam__icontains__${search}`);
  }
  return "/api/afdelingen/api/v2/objects?" + searchParams;
};

const mapOrganisatie = (x: any) =>
  ({
    ...x.record.data,
    id: x.uuid,
  }) as Afdeling;

const afdelingenFetcher = (url: string): Promise<PaginatedResult<Afdeling>> =>
  fetchLoggedIn(url)
    .then(throwIfNotOk)
    .then(parseJson)
    .then((json) => parsePagination(json, mapOrganisatie));

export const fetchAfdelingen = (search: string) =>
  afdelingenFetcher(getAfdelingenSearchUrl(search));

function useAfdelingen(search: () => string | undefined) {
  const getUrl = () => getAfdelingenSearchUrl(search());
  return ServiceResult.fromFetcher(getUrl, afdelingenFetcher);
}
</script>

<script setup lang="ts">
import ServiceDataSearch from "./ServiceDataSearch.vue";
defineProps<{ modelValue: Afdeling | undefined }>();
defineEmits<{ "update:modelValue": [Afdeling | undefined] }>();
</script>
