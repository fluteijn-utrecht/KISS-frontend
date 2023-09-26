<template>
  {{ onderwerp }}
</template>

<script lang="ts" setup>
import { ref } from "vue";
import type { Contactverzoek } from "./service";

import { fetchLoggedIn } from "@/services";
import { onMounted } from "vue";

const props = defineProps<{
  contactverzoek: any;
}>();

const emit = defineEmits(["update:onderwerp"]);

const onderwerp = ref("");

const convertToApiUrl = (fullUrl: string) => {
  try {
    const url = new URL(fullUrl);
    const id = encodeURIComponent(fullUrl);
    return `/api/contactmomentdetails?id=${id}`;
  } catch (error) {
    console.error(`Error parsing URL: ${fullUrl}`);
    return "";
  }
};

onMounted(async () => {
  if (props.contactverzoek) {
    const contactmomentUrl = props.contactverzoek.contactmoment;
    if (contactmomentUrl) {
      const apiUrl = convertToApiUrl(contactmomentUrl);
      await fetchLoggedIn(apiUrl, {})
        .then((r: any) => {
          if (!r.ok) {
            throw new Error();
          }
          return r.json();
        })
        .then((contactmomentData: any) => {
          if (contactmomentData.vraag) {
            onderwerp.value = contactmomentData.vraag;
            emit("update:onderwerp", onderwerp.value);
          } else {
            onderwerp.value = contactmomentData.specifiekeVraag;
            emit("update:onderwerp", onderwerp.value);
          }
        })
        .catch(() => {
          onderwerp.value = "Error fetching onderwerp";
        });
    }
  }
});
</script>
