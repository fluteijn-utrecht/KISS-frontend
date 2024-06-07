<template v-if="contactmomentStore.huidigContactmoment">
  <article>
    <prompt-modal
      :dialog="beforeStopDialog"
      message="Let op, je hebt het contactverzoek niet afgerond. Als je dit contactmoment afsluit, wordt het contactverzoek niet verstuurd."
    />
    <header>
      <h2>{{ klantInfo?.name || "onbekend" }}</h2>
      <p v-if="klantInfo?.contact">{{ klantInfo.contact }}</p>
    </header>
  </article>
</template>

<script setup lang="ts">
import { useContactmomentStore } from "@/stores/contactmoment";
import { useConfirmDialog } from "@vueuse/core";
import { computed } from "vue";
import PromptModal from "@/components/PromptModal.vue";
import { getKlantInfo } from "./helpers";

const beforeStopDialog = useConfirmDialog();

const contactmomentStore = useContactmomentStore();

const klantInfo = computed(() =>
  contactmomentStore.huidigContactmoment
    ? getKlantInfo(contactmomentStore.huidigContactmoment)
    : undefined,
);
</script>

<style lang="scss" scoped>
article {
  // padding-inline: var(--spacing-default);
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: var(--spacing-default);

  --utrecht-button-background-color: var(--color-error);
  --utrecht-button-hover-background-color: var(--color-error-hover);
  --utrecht-button-hover-color: var(--color-black);
  --utrecht-button-min-inline-size: auto;

  h2 {
    margin-block-start: 0;
  }

  p {
    color: inherit;
  }
}
</style>
