<template>
  {{ onderwerp }}
</template>

<script lang="ts" setup>
import { ref } from "vue";
import type { Contactverzoek } from "./service";

import { fetchLoggedIn } from "@/services";
import { onMounted } from "vue";

const props = defineProps<{
  contactverzoek: Contactverzoek;
}>();

const onderwerp = ref("");

onMounted(async () => {
  if (props.contactverzoek) {
    //todo: juiste gegevens ophalen
    //todo: loading etc regelen mbv servicedata

    //function enrichContactverzoekMetVraag(contactverzoek: Contactverzoek) {
    // let url = contactverzoek.contactmoment.replace(
    //   "https://open-klant.dev.kiss-demo.nl/contactmomenten",
    //   "",
    // );

    // url =
    //   "/api/contactmomentdetails?id=" +
    //   contactverzoek.contactmoment.split("/").pop();

    //"https%3A%2F%2Fopen-klant.dev.kiss-demo.nl%2Fklanten%2Fapi%2Fv1%2Fklanten%2F71bab8a8-2d79-4cb2-9ef2-4056b1263631
    //"https://open-klant.dev.kiss-demo.nl/contactmomenten/api/v1/contactmomenten/f4caefee-d545-4fb6-b3ea-46150160c029"
    const url =
      "/api/contactmomentdetails?id=https%3A%2F%2Fopen-klant.dev.kiss-demo.nl%2Fcontactmomenten%2Fapi%2Fv1%2Fcontactmomenten%2Ff4caefee-d545-4fb6-b3ea-46150160c029";
    await fetchLoggedIn(url, {})
      .then((r: any) => {
        if (!r.ok) {
          throw new Error();
        }
        return r.json();
      })
      .then((x: any) => {
        onderwerp.value = x + "...test";
      });
  }
});
</script>
