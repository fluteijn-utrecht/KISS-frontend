<template>
  <prompt-modal
    :dialog="dialog"
    message="Let op, je hebt het contactverzoek niet afgerond. Als je deze vraag verlaat, wordt het contactverzoek niet verstuurd."
  />
  <menu class="vragen-menu" v-if="vragen">
    <li>
      <utrecht-button
        appearance="subtle-button"
        class="icon-after plus new-question icon-only"
        type="button"
        title="Nieuwe vraag"
        @click="startNieuweVraag"
      ></utrecht-button>
    </li>
    <li v-for="(vraag, idx) in vragen" :key="idx">
      <utrecht-button
        appearance="subtle-button"
        class="icon-only"
        type="button"
        :disabled="vraag.isCurrent"
        :title="vraag.isCurrent ? 'Huidige vraag' : `Ga naar vraag ${idx + 1}`"
        @click="vraag.switchVraag"
      >
        {{ idx + 1 }}
      </utrecht-button>
    </li>
  </menu>
</template>
<script lang="ts" setup>
import { useContactmomentStore } from "@/stores/contactmoment";
import { useConfirmDialog } from "@vueuse/core";
import PromptModal from "@/components/PromptModal.vue";
import { nextTick, computed } from "vue";
import { Button as UtrechtButton } from "@utrecht/component-library-vue";

const contactmomentStore = useContactmomentStore();
const dialog = useConfirmDialog();

const vragen = computed(() =>
  contactmomentStore.huidigContactmoment?.vragen.map((vraag) => {
    return {
      isCurrent: contactmomentStore.huidigContactmoment?.huidigeVraag === vraag,
      async switchVraag() {
        contactmomentStore.switchVraag(vraag);
      },
    };
  })
);

async function startNieuweVraag() {
  contactmomentStore.startNieuweVraag();
  nextTick(() => {
    document.getElementById("cm-notitieblok")?.focus();
  });
}
</script>

<style lang="scss" scoped>
.vragen-menu {
  display: flex;
  flex-wrap: wrap;
  gap: var(--spacing-small);
  padding: var(--spacing-small);

  button {
    color: var(--color-white);

    &:disabled {
      background: var(--color-white);
      color: var(--color-headings);
      cursor: not-allowed;
    }
  }
}
</style>
