<template>
    <div class="mt-8 gridCreator text-black dark:text-white">
      <div class="md:grid grid-cols-5 gap-4">
        <p class="col-span-2">Creator</p>
        <Bloc class="rounded-xl text-black col-span-2 min-h-[75vh] md:max-h-[75vh] overflow-auto row-start-2">
            <AreaCreatorComponent/>
        </Bloc>
        <p class="col-span-3">Your AREAs</p>
        <CarouselComponent class="md:max-h-[75vh] overflow-y-auto col-span-3 row-start-2 lg:grid grid-cols-1 grid-rows-1 lg:grid-cols-2 lg:grid-rows-2 xl:grid-cols-3 gap-4 overflow-auto" />
      </div>
    </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import CarouselComponent from "@/components/CarouselComponent.vue";
import AreaCreatorComponent from '@/components/AreaCreatorComponent.vue';
import Bloc from "@/components/BlocComponent.vue";

export default defineComponent({
  components: { CarouselComponent, AreaCreatorComponent, Bloc },
  methods: {

  },
  computed: {
  },
  data() {
    return {
      filters: ["Use a tweet reaction", "Retweet a tweet from a specific user"],
      selectedValue: null,
    }
  },
});
</script>