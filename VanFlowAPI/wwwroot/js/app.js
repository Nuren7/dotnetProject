const { createApp } = Vue;

createApp({
  data: () => ({
    orders: [],
    loading: false,
    saving: false,
    error: "",
    form: {
      customerName: "",
      vehicleModel: "",
      buildNumber: "",
      requestedDeliveryDate: "",
    },
  }),
  computed: {
    activeBuilds() {
      return this.orders.filter((order) => order.status === "InProduction")
        .length;
    },
    queuedOrders() {
      return this.orders.filter((order) => order.status === "Queued").length;
    },
    syncedOrders() {
      return this.orders.filter((order) => order.erpStatus.includes("Synced"))
        .length;
    },
    syncLabel() {
      return this.orders.some((order) => order.erpStatus === "Awaiting sync")
        ? "Syncing"
        : "Healthy";
    },
  },
  methods: {
    async loadOrders() {
      this.loading = true;
      this.error = "";
      try {
        const response = await fetch("/api/orders");
        if (!response.ok) throw new Error("Could not load the order manifest.");
        this.orders = await response.json();
      } catch (error) {
        this.error = error.message;
      } finally {
        this.loading = false;
      }
    },
    async createOrder() {
      this.saving = true;
      this.error = "";
      try {
        const response = await fetch("/api/orders", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify(this.form),
        });
        if (!response.ok) {
          const details = await response.json();
          throw new Error(details.title || "Could not log the vehicle.");
        }
        this.form = {
          customerName: "",
          vehicleModel: "",
          buildNumber: "",
          requestedDeliveryDate: "",
        };
        await this.loadOrders();
      } catch (error) {
        this.error = error.message;
      } finally {
        this.saving = false;
      }
    },
    formatDate(value) {
      return new Intl.DateTimeFormat(undefined, {
        month: "short",
        day: "numeric",
        year: "numeric",
      }).format(new Date(value));
    },
    statusClass(status) {
      return status.toLowerCase().replace(" ", "-");
    },
  },
  mounted() {
    this.loadOrders();
    setInterval(this.loadOrders, 10000);
  },
}).mount("#app");
