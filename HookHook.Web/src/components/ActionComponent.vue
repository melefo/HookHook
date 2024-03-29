<template>
  <div>
    <Listbox v-model="selectedService">
      <div class="relative mt-1">
        <ListboxButton
          class="relative p-2 text-left text-black dark:text-white"
        >
          <span
            v-if="selectedService === null"
            class="block border-0 border-b-2 border-[#FD9524]"
          >
            Select a service
          </span>
        <div v-else class="flex w-[50px] h-[50px] rounded-xl" :style="{ 'background-color': color(selectedService.name.toLowerCase()) }">
          <img
            class="w-8 h-8 m-auto"
            :src="require(`@/assets/img/coloredsvg/${selectedService.name.toLowerCase()}.svg`)"
          />
        </div>
        </ListboxButton>
        <ListboxOptions
          class="
            z-10
            absolute
            py-1
            overflow-auto
            text-base
            bg-white
            rounded-md
            shadow-lg
            max-h-60
            ring-1 ring-black ring-opacity-5
            focus:outline-none
            sm:text-sm
            min-w-[80px]
            max-w-[80px]
          "
        >
          <ListboxOption
            v-slot="{ active }"
            v-for="serviceChose in services"
            :key="serviceChose.name"
            :value="serviceChose"
            as="template"
            @click="$emit('actionChange', serviceChose)"
          >
              <li class="cursor-pointer select-none relative py-2 pl-4 pr-4 hover:bg-[#A3E7EE]" :class="[active ? 'bg-[#F09113]' : '']" >
              <img
                class="w-10 h-10 m-auto"
                :src="require(`@/assets/img/${serviceChose.name.toLowerCase()}.svg`)"
              />
            </li>
          </ListboxOption>
        </ListboxOptions>
      </div>
    </Listbox>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import {
  Listbox,
  ListboxButton,
  ListboxOptions,
  ListboxOption,
} from "@headlessui/vue";
import { mapActions } from "vuex";

export default defineComponent({
  name: "ActionComponent",
  components: { Listbox, ListboxButton, ListboxOptions, ListboxOption },
  methods: {
    ...mapActions("about", ["get"]),
    color(name: string) {
      name = name.toLowerCase();
      switch (name) {
        case "twitter":
          return "#A3E7EE";
        case "spotify":
          return "#B4E1DC";
        case "discord":
          return "#D9D1EA";
        case "github":
          return "#F5CDCB";
        case "google":
          return "#F8CBAA";
        case "twitch":
          return "#FFFFC7";
      }
    },
  },
  props: {
    type: {
      type: String,
      required: true
    }
  },
  computed: {
    services() {
      const services = this.$store.state.about.info?.server?.services;
      if (services === undefined || services === null)
        return [];
      if (this.type.toLowerCase() === "action")
        return services.filter((filter: any) => filter.actions !== null && filter.actions.length > 0);
      if (this.type.toLowerCase() === "reaction")
        return services.filter((filter: any) => filter.reactions !== null && filter.reactions.length > 0);
      return services;
    }
  },
  data: function () {
    return {
      selectedService: null,
    };
  },
  created: async function () {
    this.get();
  },
});
</script>