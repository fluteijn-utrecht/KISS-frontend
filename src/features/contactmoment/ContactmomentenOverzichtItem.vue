<template>

    <summary>
      <span aria-describedby="datum-header" class="first-column">
        <DutchDate v-if="registratieDatum" :date="registratieDatum" />
      </span>
      <span aria-describedby="medewerker-header">{{
        fullName(contactmoment.medewerkerIdentificatie)
      }}</span>
      <span aria-describedby="kanaal-header">{{ contactmoment.kanaal }}</span>
      <span aria-describedby="gespreksresultaat-header">
        <ServiceDataWrapper :data="cmDetails">
          <template #success="{ data }">
            {{ data?.gespreksresultaat }}
          </template>
        </ServiceDataWrapper>
      </span>
    </summary>
    <dl>
      <dt>Starttijd</dt>
      <dd>
        <DutchTime v-if="registratieDatum" :date="registratieDatum" />
      </dd>
      <slot
        name="object"
        v-for="object in contactmoment.objectcontactmomenten"
        :key="object.object"
        :object="object"
      />
      <ServiceDataWrapper :data="cmDetails">
        <template #success="{ data: contactmoment }">
          <template v-if="contactmoment?.vraag">
            <dt>Vraag</dt>
            <dd class="tekst">{{ contactmoment.vraag }}</dd>
          </template>
          <template v-if="contactmoment?.specifiekeVraag">
            <dt>Specifieke vraag</dt>
            <dd class="tekst">{{ contactmoment.specifiekeVraag }}</dd>
          </template>
        </template>
      </ServiceDataWrapper>

      <dt>Notitie</dt>
      <dd class="tekst">{{ contactmoment.tekst }}</dd>
    </dl>

</template>

<script setup lang="ts">
import type { ContactmomentViewModel } from "../shared/types";
import { fullName } from "@/helpers/string";
import { useContactmomentDetails } from "./service";
import ServiceDataWrapper from "@/components/ServiceDataWrapper.vue";
import DutchDate from "@/components/DutchDate.vue";
import DutchTime from "@/components/DutchTime.vue";
import { computed } from "vue";
const props = defineProps<{ contactmoment: ContactmomentViewModel }>();

const registratieDatum = computed(
  () =>
    props.contactmoment.registratiedatum &&
    new Date(props.contactmoment.registratiedatum)
);



const cmDetails = useContactmomentDetails(() => props.contactmoment.url);
</script>

<style lang="scss" scoped>

</style>
