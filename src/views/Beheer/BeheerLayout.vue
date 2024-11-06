<template>
  <nav>
    <router-link to="/">Home</router-link>
    <router-link to="/Beheer/NieuwsEnWerkinstructies">
      Nieuws en werkinstructies
    </router-link>
    <router-link to="/Beheer/Skills">Skills</router-link>
    <router-link to="/Beheer/Links">Links</router-link>
    <router-link to="/Beheer/gespreksresultaten"
      >Gespreksresultaten</router-link
    >
    <router-link to="/Beheer/Kanalen">Kanalen</router-link>
    <router-link to="/Beheer/Contactverzoekformulieren"
      >Formulieren contactverzoek</router-link
    >
  </nav>
  <main>
    <application-message v-if="beheerDbEmpty" messageType="warning">
      <span>
        Wilt u KISS vullen met voorbeelddata voor Gespreksresultaten, Skills,
        Nieuws en Werkinstructies en Links?
      </span>

      <button
        type="button"
        class="utrecht-button start-button"
        @click="seedData"
      >
        Voorbeelddata aanmaken
      </button>
    </application-message>

    <router-view v-else />
  </main>
</template>

<script setup lang="ts">
import { onMounted, ref, watchEffect } from "vue";
import { useRouter } from "vue-router";
import { useCurrentUser } from "@/features/login";
import { toast } from "@/stores/toast";
import ApplicationMessage from "../../components/ApplicationMessage.vue";

const user = useCurrentUser();
const router = useRouter();

const beheerDbEmpty = ref(false);

const seedData = async () => {
  const { status } = await fetch("/api/seed");

  if (status !== 200) {
    toast({
      text: "Er is een fout opgetreden bij het vullen van KISS met voorbeelddata.",
      type: "error",
    });

    seedCheck();
  } else {
    toast({
      text: "KISS is succesvol gevuld met voorbeelddata.",
    });

    beheerDbEmpty.value = false;
  }
};

const seedCheck = async () => {
  const { status } = await fetch("/api/seed/check");

  beheerDbEmpty.value = status === 200;
};

watchEffect(() => {
  if (user.success && user.data.isLoggedIn && !user.data.isRedacteur) {
    router.push("/");
  }
});

onMounted(() => seedCheck());
</script>

<style lang="scss" scoped>
nav {
  display: flex;
  gap: var(--spacing-default);
  margin-bottom: var(--spacing-large);
}

span {
  display: block;
  margin-block-end: var(--spacing-default);
}

.start-button {
  --utrecht-button-background-color: var(--color-white);
  --utrecht-button-color: var(--color-accent-text);
}

main {
  max-width: var(--section-width-large);
}

:deep(.listItem) {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-block: var(--spacing-small);
  border-bottom: 2px solid var(--color-tertiary);
}

:deep(menu) {
  margin-block-start: var(--spacing-default);
}

:deep(.header-wrapper) {
  display: flex;
  align-items: center;
  justify-content: space-between;
}

:deep(a.utrecht-button) {
  text-decoration: none;
}

:deep(form) {
  max-inline-size: 40rem;
}

:deep(fieldset) {
  display: flex;
  flex-direction: column;

  > label {
    display: flex;
    gap: var(--spacing-small);
    align-items: baseline;

    > input {
      margin: 0;
      padding: 0;
      scale: 1.25;
      accent-color: var(--color-primary);
    }
  }
}
</style>
