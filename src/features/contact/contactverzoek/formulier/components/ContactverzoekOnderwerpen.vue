<template>
  <label
    class="utrecht-form-label"
    v-if="organisatorischeEenheidVragenSets.length > 0"
  >
    <span>Onderwerp</span>

    <select
      class="utrecht-select utrecht-select--html-select"
      name="VragenSets"
      v-model="vragenSetId"
      @change="setOnderwerp"
    >
      <option :value="0">Geen</option>
      <option
        v-for="item in organisatorischeEenheidVragenSets"
        :key="item.id"
        :value="item.id"
      >
        {{ item.titel }}
      </option>
    </select>
  </label>
</template>

<script setup lang="ts">
import { computed, useModel, watch } from "vue";
import type {
  ContactVerzoekVragenSet,
  TypeOrganisatorischeEenheid,
} from "@/features/contact/components/types";

const props = defineProps<{
  organisatorischeEenheidId?: string; // de organisatorische eenheid waarvan vragensets getoond mogen worden in de keuzelijst
  organisatorischeEenheidSoort?: TypeOrganisatorischeEenheid;
  vragenSets: ContactVerzoekVragenSet[]; // alle vragensets
  contactVerzoekVragenSet?: ContactVerzoekVragenSet; // de (voor)geselecteerde vragenset
  vragenSetId?: number;
  prefill: boolean;
}>();

const emit = defineEmits<{
  (e: "update:contactVerzoekVragenSet", v?: ContactVerzoekVragenSet): void;
  (e: "update:vragenSetId", v?: number): void;
  (e: "change"): void;
}>();

//subset van vragensets horende bij de geselecteerde afdeling
const organisatorischeEenheidVragenSets = computed(() => {
  return props.vragenSets.filter(
    (s) =>
      s.organisatorischeEenheidId == props.organisatorischeEenheidId &&
      s.organisatorischeEenheidSoort == props.organisatorischeEenheidSoort,
  );
});

//tbv modelbinding van de (voor)gekozen vragenset
const vragenSetId = useModel(props, "vragenSetId");
const vragenSetIdMap = new Map();

watch(
  organisatorischeEenheidVragenSets,
  (value) => {
    const mapId = vragenSetIdMap.get(props.organisatorischeEenheidSoort) ?? 0;

    // reset if no sets
    if (!value.length) {
      vragenSetId.value = 0;
      emit("update:contactVerzoekVragenSet", undefined);

      return;
    }

    // prefill if set changed
    if (props.prefill) {
      vragenSetId.value = value[0].id;
      emit("update:contactVerzoekVragenSet", value[0]);

      vragenSetIdMap.set(props.organisatorischeEenheidSoort, value[0].id);

      return;
    }

    // map set to id of soort
    if (mapId >= 0) {
      vragenSetId.value = mapId;

      emit(
        "update:contactVerzoekVragenSet",
        value.find((set) => set.id === mapId),
      );
    }
  },
  { immediate: true },
);

const setOnderwerp = () => {
  //wanneer een item uit de lijst gekozen is, de bijbehorende vragenset opzoeken en emitten
  const vragenset = props.vragenSets.find((x) => {
    return x.id === vragenSetId.value;
  });

  emit("update:contactVerzoekVragenSet", vragenset);
  emit("change");

  vragenSetIdMap.set(props.organisatorischeEenheidSoort, vragenSetId.value);
};
</script>
