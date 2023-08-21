<template>
  <details @click="toggleDetails">
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
    <!-- <p
      v-for="({ medewerker, completed }, i) in contactmoment.contactverzoeken"
      :key="i"
    >
      Contactverzoek verstuurd aan {{ medewerker }}. Dit verzoek
      {{ completed ? "is afgerond" : "staat open" }}.
    </p> -->
  </details>
</template>

<script setup lang="ts">
import { formatDateOnly, formatTimeOnly } from "@/helpers/date";
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

// toggle <details> open status on click anywhere within <details>, not only on <summary>
const toggleDetails = (e: Event) => {
  e.preventDefault();
  if (e.currentTarget instanceof HTMLDetailsElement) {
    e.currentTarget.open = !e.currentTarget.open;
  }
};

const cmDetails = useContactmomentDetails(() => props.contactmoment.url);
</script>

<style lang="scss" scoped>
summary {
  list-style: none;
  display: grid;
  grid-template-columns: var(--columns);
  gap: var(--gap);
  padding-block-start: var(--spacing-default);
  padding-block-end: var(--spacing-default);
}

details {
  display: grid;
  gap: var(--spacing-default);
  background: var(--color-white);

  &[open],
  &:hover {
    background-color: var(--color-secondary);
  }

  > * {
    padding-inline: var(--gap);
  }
}

dt {
  font-weight: bold;
}

dl {
  padding-inline-start: var(--spacing-default);
  display: grid;
  column-gap: var(--gap);
  row-gap: var(--spacing-default);
  grid-template-columns: var(--column-width) 1fr;
  padding-block: var(--spacing-large);
}

.tekst {
  max-width: 90ch;
  white-space: pre-wrap;
}
</style>
