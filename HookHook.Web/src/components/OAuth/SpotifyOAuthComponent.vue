<template>
  <a href="/login" @click.prevent="handleSpotify">
    <img
      v-if="oauth"
      class="h-10"
      alt="spotify"
      src="@/assets/img/spotify.svg"
    />
    <div v-else>
      <slot />
    </div>
  </a>
</template>

<script lang="ts">
import { defineComponent } from "vue";
import { mapActions } from "vuex";

export default defineComponent({
  data() {
    return {
      error: null,
      errors: null,
    };
  },
  props: {
    oauth: {
      type: Boolean,
      default: true,
    },
  },
  methods: {
    ...mapActions("signIn", ["spotify"]),
    ...mapActions("service", ["addSpotify"]),
    async handleSpotify() {
      window.removeEventListener("message", this.receiveSpotify);

      var scopes = [
        "user-read-email",
        "user-read-private",
        "user-library-modify",
        "user-library-read",
        "playlist-modify-private",
        "playlist-read-private",
        "playlist-modify-public",
      ];

      const url = `https://accounts.spotify.com/authorize?client_id=${
        process.env.VUE_APP_SPOTIFY_CLIENTID
      }&redirect_uri=${
        process.env.VUE_APP_SPOTIFY_REDIRECT
      }&state=${Math.random()
        .toString(36)
        .slice(2)}&response_type=code&scope=${scopes.join(" ")}`;
      console.log(url);
      let popup = window.open(
        url,
        "oauthWindow",
        "width=500, height=750, left=20, top=20"
      );
      if (popup == null) {
        return;
      }
      popup.focus();
      window.addEventListener("message", this.receiveSpotify, false);
    },
    async receiveSpotify(event: any) {
      if (event.origin !== window.location.origin) {
        return;
      }
      let data = Object.fromEntries(new URLSearchParams(event.data));
      if (!data.code) {
        return;
      }
      window.removeEventListener("message", this.receiveSpotify);
      const info = this.oauth
        ? await this.spotify({
            code: data.code,
            redirect: process.env.VUE_APP_SPOTIFY_REDIRECT,
          })
        : await this.addSpotify({
            code: data.code,
            redirect: process.env.VUE_APP_SPOTIFY_REDIRECT,
          });
      this.errors = info.errors || null;
      this.error = info.error || null;
      if (this.oauth) {
        if (!this.error && !this.errors) {
          this.$router.push("/dashboard");
        }
      }
    },
  },
});
</script>