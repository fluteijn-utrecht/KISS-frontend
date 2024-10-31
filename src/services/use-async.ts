import { asyncComputed } from "@vueuse/core";
import { ref } from "vue";

export const useAsync = <T>(handler: () => Promise<T> | undefined) => {
  const loading = ref(false);
  const error = ref(false);

  const data = asyncComputed<T | undefined>(
    () => {
      error.value = false;
      return handler();
    },
    undefined,
    {
      onError() {
        error.value = true;
      },
      evaluating: loading,
    },
  );
  return {
    loading,
    error,
    data,
  };
};
