<script lang="ts">
  // Landing menu — same two choices as the legacy index.html, same logo + CSS.
  const open = (href: string) => window.open(href);
</script>

<img src="img/logo.png" class="logo" alt="logotype" />
<div align="center" style="padding-top: 5%">
  <p role="link" tabindex="0" onclick={() => open("information.html")}>Telemachus UI</p>
  <hr />
  <p role="link" tabindex="0" onclick={() => open("houston/index.html")}>Houston &amp; MKON UI</p>
  <hr />
  <p role="link" tabindex="0" onclick={() => open("openmct/index.html")}>OpenMCT</p>
</div>
