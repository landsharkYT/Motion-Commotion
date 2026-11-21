mergeInto(LibraryManager.library, {
  MC_Report: function (json) {
    try { window.__motionCommotion = JSON.parse(UTF8ToString(json)); }
    catch (error) { window.__motionCommotionTelemetryError = String(error); }
  }
});
