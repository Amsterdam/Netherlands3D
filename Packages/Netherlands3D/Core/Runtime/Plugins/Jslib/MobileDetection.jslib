mergeInto(LibraryManager.library, {
  isMobile: function() {
    return /(iPhone|iPod|iPad|Android|BlackBerry|Windows Phone)/i.test(navigator.userAgent);
  }
});