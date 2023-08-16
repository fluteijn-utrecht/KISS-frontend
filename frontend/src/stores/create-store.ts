import {
  inject,
  provide,
  ref,
  type InjectionKey,
  type Ref,
  type UnwrapRef,
} from "vue";

const injectionKey = Symbol() as InjectionKey<StoreImplementation>;
const storeMap = new Map<string, Store<unknown>>();

export type CreateStoreParams<T> = {
  stateId: string;
  stateFactory: () => T;
  onExistingState?: (state: UnwrapRef<T>) => void | Promise<void>;
  onNewState?: (state: UnwrapRef<T>) => void | Promise<void>;
};

export type Store<T> = Ref<UnwrapRef<T>> & { reset: () => void };

export type StoreImplementation = <T>(params: CreateStoreParams<T>) => Store<T>;

export function defaultStoreImplementation<T>({
  stateId,
  stateFactory,
  onExistingState,
  onNewState,
}: CreateStoreParams<T>): Store<T> {
  let store = storeMap.get(stateId) as Store<T> | undefined;

  if (!store) {
    store = Object.assign(ref(stateFactory()), {
      reset() {
        if (store) {
          store.value = stateFactory() as UnwrapRef<T>;
        }
      },
    });
    storeMap.set(stateId, store);
    onNewState?.(store.value);
  } else {
    onExistingState?.(store.value);
  }

  return store;
}

export function provideStoreImplementation(
  implementation: StoreImplementation
) {
  provide(injectionKey, implementation);
}

export function ensureState<T>(params: CreateStoreParams<T>): Store<T> {
  const implementation = inject(injectionKey, defaultStoreImplementation);
  return implementation(params) as Store<T>;
}

export function resetState(stateId: string) {
  const mapValue = storeMap.get(stateId);
  if (mapValue) {
    mapValue.reset();
  }
}

export function resetAllState() {
  for (const store of storeMap.values()) {
    store.reset();
  }
}
