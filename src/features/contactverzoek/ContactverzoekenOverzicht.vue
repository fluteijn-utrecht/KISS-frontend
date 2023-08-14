<template>
  <section v-if="contactverzoeken.length">
    <div class="header">
      <span>Datum</span>
      <span>Status</span>
      <span>Behandelaar</span>
      <span>Afgerond op</span>
      <span class="chevron"></span>
    </div>

    <div
      v-for="(contactverzoek, idx) in contactverzoeken"
      :key="idx"
      class="verzoek-item"
    >
      <button
        @click="toggleItemContent(idx)"
        class="verzoek-item-header"
        :class="{ 'is-active': activeContactverzoeken[idx] }"
      >
        <span>{{ contactverzoek.record.data.registratiedatum }}</span>
        <span>{{ contactverzoek.record.data.status }}</span>
        <span>{{ contactverzoek.record.data.actor.identificatie }}</span>
        <span>???</span>
        <span class="chevron icon-after chevron-down"></span>
      </button>

      <div
        class="verzoek-item-content"
        :class="{ 'is-active': activeContactverzoeken[idx] }"
      >
        <dl>
          <dt>Toelichting voor de collega</dt>
          <dd>
            {{ contactverzoek.record.data.toelichting }}
          </dd>
        </dl>

        <dl
          v-if="contactverzoek.record.data.betrokkene.persoonsnaam?.achternaam"
        >
          <dt>Klant</dt>
          <dd>
            {{
              [
                contactverzoek.record.data.betrokkene.persoonsnaam?.voornaam,
                contactverzoek.record.data.betrokkene.persoonsnaam
                  ?.voorvoegselAchternaam,
                contactverzoek.record.data.betrokkene.persoonsnaam?.achternaam,
              ]
                .filter(Boolean)
                .join(" ")
            }}
          </dd>
        </dl>

        <dl v-if="contactverzoek.record.data.betrokkene.organisatie">
          <dt>Organisatie</dt>
          <dd>{{ contactverzoek.record.data.betrokkene.organisatie }}</dd>
        </dl>

        <dl
          v-for="(adres, idx) in contactverzoek.record.data.betrokkene
            .digitaleAdressen"
          :key="idx"
        >
          <dt>
            {{ adres.omschrijving || adres.soortDigitaalAdres || "Contact" }}
          </dt>
          <dd>
            {{ adres.adres }}
          </dd>
        </dl>

        <slot
          name="contactmoment"
          :url="contactverzoek.record.data.contactmoment"
        ></slot>
      </div>
    </div>
  </section>

  <div v-else>Geen contactverzoeken gevonden.</div>
</template>

<script lang="ts" setup>
import { ref } from "vue";
import type { Contactverzoek } from "./service";
import { watch } from "vue";

const props = defineProps<{
  contactverzoeken: Contactverzoek[];
}>();

const activeContactverzoeken = ref([] as boolean[]);

watch(
  () => props.contactverzoeken.length - activeContactverzoeken.value.length,
  (diff) => {
    for (let index = 0; index < diff; index++) {
      activeContactverzoeken.value.push(false);
    }
  }
);

const toggleItemContent = (idx: number) => {
  activeContactverzoeken.value[idx] = !activeContactverzoeken.value[idx];
};
</script>

<style lang="scss" scoped>
.header {
  display: flex;
  background-color: var(--color-tertiary);
  color: var(--color-white);
}

.header > *,
.verzoek-item-header > * {
  flex: 1;
  max-width: 250px;
  padding: var(--spacing-default);
}

.verzoek-item-header {
  all: unset;
  width: 100%;
  display: flex;
  border-bottom: 1px solid var(--color-tertiary);

  &:focus-visible {
    outline: auto currentcolor;
  }

  &.is-active {
    background-color: var(--color-secondary);

    .chevron::after {
      transform: rotate(180deg);
    }
  }

  &:hover {
    background-color: var(--color-secondary);
  }
}

.verzoek-item-content {
  background-color: var(--color-secondary);

  :deep(dl) {
    display: flex;
    padding: var(--spacing-default);

    dt {
      font-weight: bold;
    }

    > * {
      flex: 1;
    }
  }

  :deep(dt) dl > dt {
    max-width: 150px;
  }

  &:not(.is-active) {
    display: none;
  }
}

.chevron {
  display: flex;
  max-width: 50px;
  align-items: center;
  margin-inline-start: auto;

  &::after {
    transition: transform 250ms;
  }
}
</style>
