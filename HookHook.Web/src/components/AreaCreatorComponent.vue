<template>
  <div class="p-2">
    <div>
      <span class="text-red-500" v-if="error !== null">
          {{ error }}
          <br/>
      </span>
      <span class="text-red-500" v-if="errors && errors.Name">
          {{errors.Name[0]}}
          <br/>
      </span>
      <span class="text-red-500" v-if="errors && errors.Action">
          {{errors.Action[0]}}
          <br/>
      </span>
      <span class="text-red-500" v-if="errors && errors.Reaction">
          {{errors.Reaction[0]}}
          <br/>
      </span>
      <label for="name" class="dark:text-white text-black">AREA name :</label>
      <input
        id="name"
        :class="[
          name != null && name.length > 0
            ? 'border-[#A3E7EE]'
            : 'border-[#FD9524]',
        ]"
        v-model="name"
        class="
          dark:text-white
          text-black
          appearance-none
          bg-transparent
          border-0 border-b-2
          w-1/2
          py-1
          px-2
          focus:outline-none
        "
        type="text"
        placeholder="My new area"
      />
    </div>
    <SubAreaComponent
      @updateInfo="updateAction"
      reaction-index="-1"
      :service-details="serviceDetails"
      verb="When"
      areaType="Action"
    />

    <div
      v-for="(reaction, index) in reactions"
      :key="index"
      class="mt-4 flex justify-between"
    >
      <SubAreaComponent
        :service-details="serviceDetails"
        @updateInfo="updateReaction"
        :reaction-index="index"
        verb="Do"
        area-type="Reaction"
      />
      <button
        v-if="index !== 0"
        class="text-black dark:text-white pt-2"
        @click="removeReaction(index)"
      >
        <TrashIcon
          class="
            h-10
            dark:bg-[#181A1E]
            bg-[#f0f0f0]
            dark:text-[#f0f0f0]
            text-[#181A1E]
            rounded-md
            p-1.5
            mx-2
            duration-200
            hover:scale-105
          "
        />
      </button>
    </div>
    <button
      v-if="!hasNull(this.reactions[this.reactions.length - 1])"
      class="text-white pt-6"
      @click="addReaction()"
    >
      <PlusIcon
        class="
          h-10
          dark:bg-[#181A1E]
          bg-[#f0f0f0]
          dark:text-[#f0f0f0]
          text-[#181A1E]
          rounded-md
          p-1.5
          mx-2
          duration-200
          hover:scale-105
        "
      />
    </button>
    <br />
    <div class="dark:text-white text-black">
      Trigger every
      <input
        :class="[
          minutes != null && minutes >= 1 && minutes <= 1440
            ? 'border-[#A3E7EE]'
            : 'border-[#FD9524]',
        ]"
        class="
          dark:text-white
          text-black
          appearance-none
          bg-transparent
          border-0 border-b-2
          py-1
          px-2
          focus:outline-none
        "
        min="1"
        max="1440"
        @keyup="validateMinutes"
        v-model="minutes"
        type="number"
      />
      {{ minutes === 1 ? 'minute' : 'minutes' }}
    </div>
    <button
      class="
        dark:bg-[#181A1E]
        bg-[#f0f0f0]
        dark:text-[#f0f0f0]
        text-[#181A1E]
        rounded-md
        p-1.5
        mx-2
        mt-4
        duration-200
        hover:scale-105
      "
      @click="createArea()"
    >
    <div class="flex flex-row items-center">
      <SaveIcon class="h-8" /><span class="mx-2">Create</span>
    </div>
    </button>
  </div>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { mapActions } from "vuex";
import SubAreaComponent from "@/components/SubAreaComponent.vue";
import { TrashIcon, PlusIcon, SaveIcon } from "@heroicons/vue/solid";

export default defineComponent({
  name: "DropdownComponent",
  components: { SubAreaComponent, TrashIcon, PlusIcon, SaveIcon },
  methods: {
    ...mapActions("area", ["getServices", "createAreaRequest"]),
    validateMinutes() {
      this.minutes = Math.abs(this.minutes);
      if (this.minutes < 1) {
        this.minutes = 1;
      }
      if (this.minutes > 1440) {
        this.minutes = 1440;
      }
    },
    updateAction({ type, params, accountId }: any) {
      this.action = { type: type, arguments: [...params], accountId };
    },
    updateReaction({ type, params, index, accountId }: any) {
      this.reactions[index] = { type: type, arguments: [...params], accountId };
    },
    async createArea() {
      // todo call the store, check for errors
      const { error, errors } = await this.createAreaRequest({
        action: this.action,
        reactions: this.reactions,
        minutes: this.minutes,
        name: this.name
      });
      this.error = error || null;
      this.errors = errors || null;
    },
    addReaction() {
      this.reactions.push(null);
    },
    removeReaction(index: number) {
      this.reactions.splice(index, 1);
    },
    hasNull(obj: any) {
      if (obj === null) return true;
      for (var key in obj.arguments) {
        if (obj.arguments[key] === null) return true;
      }
      return false;
    },
  },
  computed: {
    serviceDetails() {
      const that: any = this;
      return that.$store.state.area.services;
    }
  },
  data: function () {
    return {
      action: null as any,
      reactions: [null] as any[],
      error: null as any,
      errors: [null] as any[],
      minutes: 5 as number,
      name: null,
    };
  },
  setup() {},
  created: async function () {
    this.getServices();
  },
});
</script>