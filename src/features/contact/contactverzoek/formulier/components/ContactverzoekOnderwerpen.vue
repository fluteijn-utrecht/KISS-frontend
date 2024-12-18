<template>
  <label
    class="utrecht-form-label"
    v-if="
      organisatorischeEenheidVragenSets &&
      organisatorischeEenheidVragenSets.length > 0
    "
  >
    <span>Onderwerp</span>

    <select
      class="utrecht-select utrecht-select--html-select"
      name="VragenSets"
      v-model="vragenSetId"
      @change="setOnderwerp"
    >
      <option value="" selected>Geen</option>
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
import { computed, ref, watch } from "vue";
import type {
  ContactVerzoekVragenSet,
  TypeOrganisatorischeEenheid,
} from "@/features/contact/components/types";
import { watchEffect } from "vue";

const props = defineProps<{
  organisatorischeEenheidId?: string; //de organisatorische eenheid waarvan vragensets getoond mogen worden in de keuzelijst
  organisatorischeEenheidSoort?: TypeOrganisatorischeEenheid; //het soort organisatorische eenheid waarvan vragensets getoond mogen worden in de keuzelijst
  vragenSets: ContactVerzoekVragenSet[]; //alle vragensets
  modelValue?: ContactVerzoekVragenSet; //de (voor)geselecteerde vragenset
  prefill: boolean;
}>();

const emit = defineEmits<{
  (e: "update:modelValue", v?: ContactVerzoekVragenSet): void;
  (e: "change"): void;
}>();

//subset van vragensets horende bij de geselecteerde afdeling
const organisatorischeEenheidVragenSets = computed(() => {
  const selectedOrganisatorischeEenheidId = props.organisatorischeEenheidId;
  const selectedOrganisatorischeEenheidSoort =
    props.organisatorischeEenheidSoort;

  return props.vragenSets.filter(
    (s) =>
      s.organisatorischeEenheidId == selectedOrganisatorischeEenheidId &&
      s.organisatorischeEenheidSoort == selectedOrganisatorischeEenheidSoort,
  );
});

//tbv modelbinding van de (voor)gekozen vragenset
const vragenSetId = ref<number | undefined>();

watchEffect(() => {
  //eerder gekozen waarde voorselecteren
  vragenSetId.value = props.modelValue?.id;
});

watch(
  [
    () => props.organisatorischeEenheidId,
    () => props.organisatorischeEenheidSoort,
  ],
  ([vIdNew, vSoortNew], [vIdOld, vSoortOld]) => {
    if (vIdNew !== vIdOld || vSoortNew !== vSoortOld) {
      vragenSetId.value = undefined;
      emit("update:modelValue", undefined);
    }
  },
  { immediate: true },
);

watch(
  organisatorischeEenheidVragenSets,
  (v) => {
    if (v && v.length > 0 && !vragenSetId.value && props.prefill) {
      vragenSetId.value = v[0].id;
      emit("update:modelValue", v[0]);
    }
  },
  { immediate: true },
);

const setOnderwerp = () => {
  //wanneer een item uit de lijst gekozen is, de bijbehorende vragenset opzoeken en emitten
  const vragenset = props.vragenSets.find((x) => {
    return x.id === vragenSetId?.value;
  });
  emit("update:modelValue", vragenset);
  emit("change");
};
</script>
