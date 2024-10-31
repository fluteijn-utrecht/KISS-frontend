import { asyncComputed } from "@vueuse/core";
import { ref } from "vue";

export const useAsync = <T>(
  handler: (signal: AbortSignal) => Promise<T> | undefined,
) => {
  const loading = ref(false);
  const error = ref(false);

  const data = asyncComputed<T | undefined>(
    (onCancel) => {
      const controller = new AbortController();
      onCancel(() => controller.abort());
      error.value = false;
      return handler(controller.signal);
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
