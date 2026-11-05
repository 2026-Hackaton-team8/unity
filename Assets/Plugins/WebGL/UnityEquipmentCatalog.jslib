mergeInto(LibraryManager.library, {
  DispatchEquipmentCatalog: function (jsonPtr) {
    var jsonText = UTF8ToString(jsonPtr);
    var payload;

    try {
      payload = JSON.parse(jsonText);
    } catch (error) {
      console.error("Unity sent invalid equipment catalog JSON.", error);
      return;
    }

    window.dispatchEvent(new CustomEvent("unity-equipment-catalog", {
      detail: payload
    }));
  }
});
